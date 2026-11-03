using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ResultadoRiego
{
    Insuficiente,
    Correcto,
    Exceso
}

public class Regadera : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Raíz del minijuego")]
    [Tooltip("Objeto que contiene todo el minijuego (fondo, planta, regadera...). Si se deja vacío se usa este mismo objeto.")]
    [SerializeField] private GameObject raizMinijuego;

    [Header("Referencias UI")]
    public GameObject prefabGota;
    public RectTransform contenedorPanel;
    public RectTransform puntoSalidaGota;
    public RectTransform zonaColisionMaceta;
    public Image imagenBasePlanta;
    public Image imagenLineaMeta;

    [Header("Configuración Inclinación")]
    public RectTransform transformRegadera;
    public float anguloInclinación = -30f;
    public float velocidadInclinación = 10f;
    private Quaternion rotacionOriginal;

    [Header("Configuracion de Gotas")]
    public float frecuenciaGotas = 0.10f;
    public float velocidadGotas = 200f;
    [Tooltip("Tiempo máximo (segundos) que se espera a que las gotas en el aire caigan antes de evaluar el riego.")]
    public float tiempoMaxEsperaGotas = 2f;
    private float temporizadorGotas = 0f;

    [Header("Sprites de las plantas (estado en la que se encuentre)")]
    [Tooltip("Solo se usa si la imagen de la planta no recibe el sprite de la planta seleccionada.")]
    public Sprite spriteBase;
    [Tooltip("Opcional. Si está vacío, la planta se tiñe con 'colorHumedo'.")]
    public Sprite spriteHumedo;
    [Tooltip("Opcional. Si está vacío, la planta se tiñe con 'colorMuerta'.")]
    public Sprite spriteMuerta;
    public Color colorHumedo = new Color(0.75f, 0.85f, 1f, 1f);
    public Color colorMuerta = new Color(0.45f, 0.45f, 0.45f, 1f);

    [Header("Colores de la línea de meta")]
    public Color colorlineaPendiente = Color.white;
    public Color colorlinraAlcanzada = Color.green;

    [Header("Progreso del riego")]
    public float progresoAgua = 0f;
    public float nivelMinimo = 60f;
    public float nivelMaximo = 100f;
    public float incrementoPorGota = 4f;

    public event Action Terminado;

    public GameObject Raiz => raizMinijuego != null ? raizMinijuego : gameObject;

    private enum EstadoVisual { Base, Humedo, Muerta }

    private readonly List<GameObject> gotasActivas = new List<GameObject>();
    private Action<ResultadoRiego> alTerminar;
    private Coroutine rutinaEvaluacion;
    private Sprite spritePlantaOriginal;

    private bool estaRegando = false;
    private bool esperandoGotas = false;
    private bool miniJuegoCompletado = true;

    private void Awake()
    {
        if (transformRegadera == null)
            transformRegadera = GetComponent<RectTransform>();

        rotacionOriginal = transformRegadera.localRotation;
    }

    private void OnDisable()
    {
        estaRegando = false;
        esperandoGotas = false;
        LimpiarGotas();
        AplicarVisualPlanta(EstadoVisual.Base);
    }

    public void Iniciar(Action<ResultadoRiego> alTerminar)
    {
        StopAllCoroutines();
        rutinaEvaluacion = null;
        LimpiarGotas();

        this.alTerminar = alTerminar;

        progresoAgua = 0f;
        temporizadorGotas = 0f;
        estaRegando = false;
        esperandoGotas = false;
        miniJuegoCompletado = false;

        transformRegadera.localRotation = rotacionOriginal;

        // El gestor principal ya aplicó el sprite de la planta seleccionada antes de llamar a Iniciar.
        if (imagenBasePlanta != null && imagenBasePlanta.sprite != null)
            spritePlantaOriginal = imagenBasePlanta.sprite;
        else
            spritePlantaOriginal = spriteBase;

        AplicarVisualPlanta(EstadoVisual.Base);

        if (imagenLineaMeta != null)
            imagenLineaMeta.color = colorlineaPendiente;
    }

    private void Update()
    {
        Quaternion rotacionObjetivo = estaRegando
            ? Quaternion.Euler(0, 0, anguloInclinación)
            : rotacionOriginal;

        transformRegadera.localRotation = Quaternion.Lerp(transformRegadera.localRotation, rotacionObjetivo, Time.deltaTime * velocidadInclinación);

        if (miniJuegoCompletado || !estaRegando) return;

        temporizadorGotas += Time.deltaTime;
        if (temporizadorGotas >= frecuenciaGotas)
        {
            temporizadorGotas = 0f;
            CrearGota();
        }
    }

    private void CrearGota()
    {
        if (prefabGota == null || contenedorPanel == null || puntoSalidaGota == null) return;

        GameObject nuevaGota = Instantiate(prefabGota, contenedorPanel);
        RectTransform rectGota = nuevaGota.GetComponent<RectTransform>();

        rectGota.anchoredPosition = puntoSalidaGota.anchoredPosition;

        // Las gotas no deben interceptar los toques (nacen sobre la regadera).
        Graphic grafico = nuevaGota.GetComponent<Graphic>();
        if (grafico != null) grafico.raycastTarget = false;

        GotaAgua scriptGota = nuevaGota.AddComponent<GotaAgua>();
        scriptGota.Inicializar(this, velocidadGotas, zonaColisionMaceta);

        gotasActivas.Add(nuevaGota);
    }

    // La llama GotaAgua cuando una gota toca la maceta.
    public void RegistrarImpactoGota()
    {
        if (miniJuegoCompletado) return;

        progresoAgua = Mathf.Clamp(progresoAgua + incrementoPorGota, 0f, nivelMaximo);

        ActualizarEstadoJuego();

        if (progresoAgua >= nivelMaximo)
        {
            Terminar(ResultadoRiego.Exceso);
        }
    }

    private void ActualizarEstadoJuego()
    {
        bool alcanzado = progresoAgua >= nivelMinimo;

        AplicarVisualPlanta(alcanzado ? EstadoVisual.Humedo : EstadoVisual.Base);

        if (imagenLineaMeta != null)
            imagenLineaMeta.color = alcanzado ? colorlinraAlcanzada : colorlineaPendiente;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (miniJuegoCompletado || esperandoGotas) return;

        estaRegando = true;
        temporizadorGotas = 0f;
        CrearGota();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (miniJuegoCompletado || !estaRegando) return;

        estaRegando = false;
        rutinaEvaluacion = StartCoroutine(EsperarGotasYEvaluar());
    }

    // Al soltar todavía puede haber gotas en el aire: se espera a que caigan para no evaluar con un progreso incompleto.
    private IEnumerator EsperarGotasYEvaluar()
    {
        esperandoGotas = true;

        float limite = Time.time + tiempoMaxEsperaGotas;
        while (HayGotasEnCaida() && Time.time < limite)
        {
            yield return null;
        }

        esperandoGotas = false;
        rutinaEvaluacion = null;

        if (miniJuegoCompletado) yield break;

        Terminar(progresoAgua >= nivelMinimo ? ResultadoRiego.Correcto : ResultadoRiego.Insuficiente);
    }

    private bool HayGotasEnCaida()
    {
        for (int i = gotasActivas.Count - 1; i >= 0; i--)
        {
            GameObject gota = gotasActivas[i];
            if (gota == null || !gota.activeInHierarchy)
            {
                gotasActivas.RemoveAt(i);
            }
        }

        return gotasActivas.Count > 0;
    }

    private void Terminar(ResultadoRiego resultado)
    {
        if (miniJuegoCompletado) return;

        miniJuegoCompletado = true;
        estaRegando = false;
        esperandoGotas = false;

        if (rutinaEvaluacion != null)
        {
            StopCoroutine(rutinaEvaluacion);
            rutinaEvaluacion = null;
        }

        LimpiarGotas();

        switch (resultado)
        {
            case ResultadoRiego.Correcto:
                Debug.Log("Regaste bien la planta");
                break;
            case ResultadoRiego.Insuficiente:
                Debug.Log("Le echaste poca agua a la planta");
                break;
            case ResultadoRiego.Exceso:
                AplicarVisualPlanta(EstadoVisual.Muerta);
                Debug.Log("Te excediste con el agua de la planta");
                break;
        }

        Action<ResultadoRiego> cb = alTerminar;
        alTerminar = null;

        cb?.Invoke(resultado);
        Terminado?.Invoke();
    }

    private void AplicarVisualPlanta(EstadoVisual estado)
    {
        if (imagenBasePlanta == null) return;

        Sprite sprite = spritePlantaOriginal;
        Color color = Color.white;

        if (estado == EstadoVisual.Humedo)
        {
            if (spriteHumedo != null) sprite = spriteHumedo;
            else color = colorHumedo;
        }
        else if (estado == EstadoVisual.Muerta)
        {
            if (spriteMuerta != null) sprite = spriteMuerta;
            else color = colorMuerta;
        }

        if (sprite != null) imagenBasePlanta.sprite = sprite;
        imagenBasePlanta.color = color;
    }

    private void LimpiarGotas()
    {
        foreach (GameObject gota in gotasActivas)
        {
            if (gota != null) Destroy(gota);
        }

        gotasActivas.Clear();
    }
}
