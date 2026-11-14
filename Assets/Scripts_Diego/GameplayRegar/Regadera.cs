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
    [SerializeField] private GameObject raizMinijuego;

    [Header("Referencias UI")]
    public GameObject prefabGota;
    public RectTransform contenedorPanel;   
    public RectTransform puntoSalidaA;
    public RectTransform puntoSalidaB;
    public RectTransform puntoSalidaC;
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
    public float tiempoMaxEsperaGotas = 2f;
    private float temporizadorGotas = 0f;

    [Header("Tinte de la planta")]
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

    [Header("Barra de nivel de agua")]
    public Image barraAgua;
    public TMPro.TextMeshProUGUI textoNivelAgua;
    public bool colocarLineaMetaEnBarra = true;
    [Range(0f, 1f)] public float fraccionPeligro = 0.85f;
    public float velocidadBarra = 6f;
    public Color colorBarraBajo = new Color(0.55f, 0.75f, 1f, 1f);
    public Color colorBarraCorrecto = new Color(0.35f, 0.85f, 0.45f, 1f);
    public Color colorBarraPeligro = new Color(1f, 0.45f, 0.35f, 1f);
    private float rellenoMostrado;

    public event Action Terminado;

    public GameObject Raiz => raizMinijuego != null ? raizMinijuego : gameObject;

    private enum EstadoVisual { Base, Humedo, Muerta }

    private readonly List<GameObject> gotasActivas = new List<GameObject>();
    private Action<ResultadoRiego> alTerminar;
    private Coroutine rutinaEvaluacion;
    private Sprite spritePlantaOriginal;
    private Color colorPlantaOriginal = Color.white;

    private bool estaRegando = false;
    private bool esperandoGotas = false;
    private bool miniJuegoCompletado = true;

    public ParticleSystem particulasImpactoMaceta;

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

        if (imagenBasePlanta != null)
        {
            spritePlantaOriginal = imagenBasePlanta.sprite;
            colorPlantaOriginal = imagenBasePlanta.color;
        }

        AplicarVisualPlanta(EstadoVisual.Base);

        if (imagenLineaMeta != null)
            imagenLineaMeta.color = colorlineaPendiente;

        rellenoMostrado = 0f;
        ColocarLineaMeta();
        ActualizarBarra(true);
    }

    private void ColocarLineaMeta()
    {
        if (!colocarLineaMetaEnBarra || imagenLineaMeta == null || barraAgua == null || nivelMaximo <= 0f) return;

        RectTransform linea = imagenLineaMeta.rectTransform;
        if (linea.parent != barraAgua.rectTransform.parent && linea.parent != barraAgua.rectTransform) return;

        float y = Mathf.Clamp01(nivelMinimo / nivelMaximo);
        linea.anchorMin = new Vector2(linea.anchorMin.x, y);
        linea.anchorMax = new Vector2(linea.anchorMax.x, y);
        linea.anchoredPosition = new Vector2(linea.anchoredPosition.x, 0f);
    }

    private void ActualizarBarra(bool inmediato)
    {
        if (barraAgua == null || nivelMaximo <= 0f) return;

        float objetivo = Mathf.Clamp01(progresoAgua / nivelMaximo);
        rellenoMostrado = inmediato
            ? objetivo
            : Mathf.MoveTowards(rellenoMostrado, objetivo, velocidadBarra * Time.deltaTime);

        barraAgua.fillAmount = rellenoMostrado;

        if (objetivo >= fraccionPeligro) barraAgua.color = colorBarraPeligro;
        else if (progresoAgua >= nivelMinimo) barraAgua.color = colorBarraCorrecto;
        else barraAgua.color = colorBarraBajo;

        if (textoNivelAgua != null)
            textoNivelAgua.text = Mathf.RoundToInt(objetivo * 100f) + "%";
    }

    private void Update()
    {
        Quaternion rotacionObjetivo = estaRegando
            ? Quaternion.Euler(0, 0, anguloInclinación)
            : rotacionOriginal;

        transformRegadera.localRotation = Quaternion.Lerp(transformRegadera.localRotation, rotacionObjetivo, Time.deltaTime * velocidadInclinación);

        ActualizarBarra(false);

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
        if (prefabGota == null || contenedorPanel == null) return;

        
        List<RectTransform> puntosDisponibles = new List<RectTransform>();
        if (puntoSalidaA != null) puntosDisponibles.Add(puntoSalidaA);
        if (puntoSalidaB != null) puntosDisponibles.Add(puntoSalidaB);
        if (puntoSalidaC != null) puntosDisponibles.Add(puntoSalidaC);

        
        if (puntosDisponibles.Count == 0) return;

       
        RectTransform puntoElegido = puntosDisponibles[UnityEngine.Random.Range(0, puntosDisponibles.Count)];

        GameObject nuevaGota = Instantiate(prefabGota, contenedorPanel);
        RectTransform rectGota = nuevaGota.GetComponent<RectTransform>();

        rectGota.anchoredPosition = puntoElegido.anchoredPosition;

        Graphic grafico = nuevaGota.GetComponent<Graphic>();
        if (grafico != null) grafico.raycastTarget = false;

        GotaAgua scriptGota = nuevaGota.AddComponent<GotaAgua>();
        scriptGota.Inicializar(this, velocidadGotas, zonaColisionMaceta);

        gotasActivas.Add(nuevaGota);
    }

    public void RegistrarImpactoGota(Vector2 posicionImpacto = default)
    {
        if (miniJuegoCompletado) return;

        progresoAgua = Mathf.Clamp(progresoAgua + incrementoPorGota, 0f, nivelMaximo);
        GestorSonidos.Reproducir(Sonido.GotaAgua);

        

        if (particulasImpactoMaceta != null)
        {
            if (posicionImpacto != default && contenedorPanel != null)
            {
                
                Vector2 posicionAjustada = posicionImpacto + new Vector2(0f, 420f);
                particulasImpactoMaceta.GetComponent<RectTransform>().anchoredPosition = posicionAjustada;
            }
            particulasImpactoMaceta.Emit(8);
        } 

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

        Color color = colorPlantaOriginal;
        if (estado == EstadoVisual.Humedo) color = colorHumedo;
        else if (estado == EstadoVisual.Muerta) color = colorMuerta;

        if (spritePlantaOriginal != null) imagenBasePlanta.sprite = spritePlantaOriginal;
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