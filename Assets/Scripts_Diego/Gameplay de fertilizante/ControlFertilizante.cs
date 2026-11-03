using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ControlFertilizante : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Raíz del minijuego")]
    [Tooltip("Objeto que contiene todo el minijuego. Si se deja vacío se usa este mismo objeto.")]
    [SerializeField] private GameObject raizMinijuego;

    [Header("Referencias de Gotas y UI")]
    public GameObject prefabFertilizante;
    public RectTransform contenedorPanel;
    public RectTransform puntoSalidaFertilizante;
    public RectTransform zonaColisionPlanta;
    public TextMeshProUGUI texProgresoFertilizante;

    [Header("Configuración de la Barra y del Indicador")]
    public RectTransform indicadorBarra;        
    public float velocidadSubida = 400f;
    public float velocidadBajada = 350f;
    public float minY = -180f;
    public float maxY = 180f;

    [Header("Configuración de la Zona Objetivo Móvil")]
    public RectTransform zonaObjetivo;         
    public float velocidadZona = 150f;         
    public float minZonaY = -150f;              
    public float maxZonaY = 150f;           
    private int direccionZona = 1;               

    [Header("Configuración de Gotas")]
    public float frecuenciaGotas = 0.15f;
    public float velocidadGotas = 250f;
    private float temporizadorGotas = 0f;

    [Header("Progreso del Minijuego")]
    public float progresoFertilizante = 0f;
    public float metaRequerida = 100f;
    public float incrementoPorGota = 10f;

    public event Action Terminado;

    public GameObject Raiz => raizMinijuego != null ? raizMinijuego : gameObject;

    private readonly List<GameObject> gotasActivas = new List<GameObject>();
    private Action alTerminar;

    private bool estaPresionando = false;
    private bool minijuegoTerminado = true;

    private void OnDisable()
    {
        estaPresionando = false;
        LimpiarGotas();
    }

    // Compatibilidad con GestorMenuAcciones (flujo antiguo, Scripts_Diego).
    // Con GestorMinijuegosPrincipal ya no hace falta: la planta se pasa al abrir el minijuego
    // y el efecto lo aplica el gestor principal.
    public void AsignarPlantaObjetivo(GestorSaludPlanta planta)
    {
        Debug.LogWarning("[ControlFertilizante] AsignarPlantaObjetivo ya no se usa. Abre el minijuego con GestorMinijuegosPrincipal.AbrirFertilizante().");
    }

    public void Iniciar(Action alTerminar)
    {
        this.alTerminar = alTerminar;

        LimpiarGotas();

        progresoFertilizante = 0f;
        temporizadorGotas = 0f;
        estaPresionando = false;
        minijuegoTerminado = false;

        // La meta empieza en un punto y una dirección aleatorios para que cada partida sea distinta
        direccionZona = UnityEngine.Random.value < 0.5f ? 1 : -1;

        if (zonaObjetivo != null)
        {
            Vector2 posZona = zonaObjetivo.anchoredPosition;
            posZona.y = UnityEngine.Random.Range(minZonaY, maxZonaY);
            zonaObjetivo.anchoredPosition = posZona;
        }

        if (indicadorBarra != null)
        {
            Vector2 posIndicador = indicadorBarra.anchoredPosition;
            posIndicador.y = minY;
            indicadorBarra.anchoredPosition = posIndicador;
        }

        ActualizarTextoUI();
    }

    private void Update()
    {
        if (minijuegoTerminado) return;

        // 1. Movimiento automático de la Zona Objetivo (Meta Móvil)
        if (zonaObjetivo != null)
        {
            Vector2 posZona = zonaObjetivo.anchoredPosition;
            posZona.y += velocidadZona * direccionZona * Time.deltaTime;

            // Cambiar de dirección al llegar a los límites
            if (posZona.y >= maxZonaY)
            {
                posZona.y = maxZonaY;
                direccionZona = -1; // Invierte hacia abajo
            }
            else if (posZona.y <= minZonaY)
            {
                posZona.y = minZonaY;
                direccionZona = 1;  // Invierte hacia arriba
            }
            zonaObjetivo.anchoredPosition = posZona;
        }

        // 2. Movimiento del indicador del jugador (Sube al presionar, baja al soltar)
        if (indicadorBarra != null)
        {
            Vector2 posIndicador = indicadorBarra.anchoredPosition;
            if (estaPresionando)
            {
                posIndicador.y += velocidadSubida * Time.deltaTime;
            }
            else
            {
                posIndicador.y -= velocidadBajada * Time.deltaTime;
            }
            posIndicador.y = Mathf.Clamp(posIndicador.y, minY, maxY);
            indicadorBarra.anchoredPosition = posIndicador;
        }

        // 3. Generación de gotas SOLO si se presiona Y está dentro de la zona objetivo
        if (estaPresionando)
        {
            temporizadorGotas += Time.deltaTime;
            if (temporizadorGotas >= frecuenciaGotas)
            {
                temporizadorGotas = 0f;

                if (VerificarSiEstaEnMeta())
                {
                    CrearGotaFertilizante();
                }
            }
        }
    }

    private void CrearGotaFertilizante()
    {
        if (prefabFertilizante == null || contenedorPanel == null || puntoSalidaFertilizante == null) return;

        GameObject nuevaGota = Instantiate(prefabFertilizante, contenedorPanel);
        RectTransform rectGota = nuevaGota.GetComponent<RectTransform>();
        rectGota.anchoredPosition = puntoSalidaFertilizante.anchoredPosition;

        // Las gotas no deben interceptar los toques sobre el área de presión.
        Graphic grafico = nuevaGota.GetComponent<Graphic>();
        if (grafico != null) grafico.raycastTarget = false;

        GotaFertilizante scriptGota = nuevaGota.AddComponent<GotaFertilizante>();
        scriptGota.Inicializar(this, velocidadGotas, zonaColisionPlanta, true);

        gotasActivas.RemoveAll(g => g == null);
        gotasActivas.Add(nuevaGota);
    }

    // Comprobación exacta por rango Y local para saber si hay solapamiento
    private bool VerificarSiEstaEnMeta()
    {
        if (indicadorBarra == null || zonaObjetivo == null) return false;

        float indicadorY = indicadorBarra.anchoredPosition.y;
        float indicadorAltura = indicadorBarra.rect.height;

        float zonaY = zonaObjetivo.anchoredPosition.y;
        float zonaAltura = zonaObjetivo.rect.height;

        float indicadorMin = indicadorY - (indicadorAltura / 2f);
        float indicadorMax = indicadorY + (indicadorAltura / 2f);

        float zonaMin = zonaY - (zonaAltura / 2f);
        float zonaMax = zonaY + (zonaAltura / 2f);

        return indicadorMax >= zonaMin && indicadorMin <= zonaMax;
    }

    // La llama GotaFertilizante cuando una gota llega a la planta.
    public void RegistrarImpactoExitoso()
    {
        if (minijuegoTerminado) return;

        progresoFertilizante += incrementoPorGota;
        progresoFertilizante = Mathf.Clamp(progresoFertilizante, 0f, metaRequerida);

        ActualizarTextoUI();

        if (progresoFertilizante >= metaRequerida)
        {
            CompletarMinijuego();
        }
    }

    private void ActualizarTextoUI()
    {
        if (texProgresoFertilizante != null)
        {
            texProgresoFertilizante.text = "Abono: " + Mathf.FloorToInt(progresoFertilizante) + "%";
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (minijuegoTerminado) return;
        estaPresionando = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        estaPresionando = false;
    }

    private void CompletarMinijuego()
    {
        minijuegoTerminado = true;
        estaPresionando = false;

        LimpiarGotas();

        if (texProgresoFertilizante != null)
        {
            texProgresoFertilizante.text = "¡Planta Abonada!";
        }

        Action cb = alTerminar;
        alTerminar = null;

        cb?.Invoke();
        Terminado?.Invoke();
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
