using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ControlFertilizante : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Referencias de Gotas y UI")]
    public GameObject prefabFertilizante;
    public RectTransform contenedorPanel;
    public RectTransform puntoSalidaFertilizante;
    public RectTransform zonaColisionPlanta;
    public TextMeshProUGUI texProgresoFertilizante;

    [Header("Configuración de la Barra y del Indicador")]
    public RectTransform indicadorBarra;          // El indicador controlado por el jugador
    public float velocidadSubida = 400f;
    public float velocidadBajada = 350f;
    public float minY = -180f;
    public float maxY = 180f;

    [Header("Configuración de la Zona Objetivo Móvil")]
    public RectTransform zonaObjetivo;            // La zona amarilla que se moverá sola
    public float velocidadZona = 150f;            // Qué tan rápido se desplaza la meta
    public float minZonaY = -150f;                // Límite inferior de movimiento de la meta
    public float maxZonaY = 150f;                 // Límite superior de movimiento de la meta
    private int direccionZona = 1;                // Dirección actual (1 sube, -1 baja)

    [Header("Configuración de Gotas")]
    public float frecuenciaGotas = 0.15f;
    public float velocidadGotas = 250f;
    private float temporizadorGotas = 0f;

    [Header("Progreso del Minijuego")]
    public float progresoFertilizante = 0f;
    public float metaRequerida = 100f;
    public float incrementoPorGota = 10f;

    private bool estaPresionando = false;
    private bool minijuegoTerminado = false;

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

                // Valida estrictamente si el indicador está en la zona meta móvil
                if (VerificarSiEstaEnMeta())
                {
                    CrearGotaFertilizante();
                }
                else
                {
                    Debug.Log("Fuera de la zona objetivo: No se echa fertilizante.");
                }
            }
        }
    }

    void CrearGotaFertilizante()
    {
        if (prefabFertilizante == null || contenedorPanel == null || puntoSalidaFertilizante == null) return;

        GameObject nuevaGota = Instantiate(prefabFertilizante, contenedorPanel);
        RectTransform rectGota = nuevaGota.GetComponent<RectTransform>();
        rectGota.anchoredPosition = puntoSalidaFertilizante.anchoredPosition;

        GotaFertilizante scriptGota = nuevaGota.AddComponent<GotaFertilizante>();
        scriptGota.Inicializar(this, velocidadGotas, zonaColisionPlanta, true);
    }

    // Comprobación exacta por rango Y local para saber si hay solapamiento
    bool VerificarSiEstaEnMeta()
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

    public void RegistrarImpactoExitoso()
    {
        if (minijuegoTerminado) return;
        progresoFertilizante += incrementoPorGota;
        progresoFertilizante = Mathf.Clamp(progresoFertilizante, 0f, metaRequerida);

        ActualizarTextoUI();
        Debug.Log("¡Fertilizante aplicado con éxito! Progreso: " + progresoFertilizante);

        if (progresoFertilizante >= metaRequerida)
        {
            CompletarMinijuego();
        }
    }

    void ActualizarTextoUI()
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
        if (minijuegoTerminado) return;
        estaPresionando = false;
    }

    void CompletarMinijuego()
    {
        minijuegoTerminado = true;
        estaPresionando = false;
        if (texProgresoFertilizante != null)
        {
            texProgresoFertilizante.text = "¡Planta Abonada!";
        }
        Debug.Log("¡Minijuego de fertilizante completado con éxito!");
    }
}