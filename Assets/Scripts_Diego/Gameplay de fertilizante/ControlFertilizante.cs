using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ControlFertilizante : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Raíz del minijuego")]
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

    [Header("Animación de la Bolsa")]
    public RectTransform bolsaFertilizante;
    public float anguloAgitado = 8f;
    public float duracionAgitado = 0.12f;
    public float fuerzaSaltoGota = 0.06f;
    public float duracionRegreso = 0.2f;

    public ParticleSystem particulasImpactoPlanta;

    public event Action Terminado;

    public GameObject Raiz => raizMinijuego != null ? raizMinijuego : gameObject;

    private readonly List<GameObject> gotasActivas = new List<GameObject>();
    private Action alTerminar;

    private bool estaPresionando = false;
    private bool minijuegoTerminado = true;

    private Tween tweenAgitado;
    private Vector3 rotacionOriginalBolsa;
    private Vector3 escalaOriginalBolsa;
    private bool bolsaInicializada;

    private void OnDisable()
    {
        estaPresionando = false;
        LimpiarGotas();
        DetenerAgitado(true);
    }

    public void AsignarPlantaObjetivo(GestorSaludPlanta planta)
    {
        Debug.LogWarning("[ControlFertilizante] AsignarPlantaObjetivo ya no se usa. Abre el minijuego con GestorMinijuegosPrincipal.AbrirFertilizante().");
    }

    public void Iniciar(Action alTerminar)
    {
        this.alTerminar = alTerminar;

        LimpiarGotas();
        InicializarBolsa();
        DetenerAgitado(true);

        progresoFertilizante = 0f;
        temporizadorGotas = 0f;
        estaPresionando = false;
        minijuegoTerminado = false;

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

        if (zonaObjetivo != null)
        {
            Vector2 posZona = zonaObjetivo.anchoredPosition;
            posZona.y += velocidadZona * direccionZona * Time.deltaTime;

            if (posZona.y >= maxZonaY)
            {
                posZona.y = maxZonaY;
                direccionZona = -1;
            }
            else if (posZona.y <= minZonaY)
            {
                posZona.y = minZonaY;
                direccionZona = 1;
            }
            zonaObjetivo.anchoredPosition = posZona;
        }

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

    private void InicializarBolsa()
    {
        if (bolsaFertilizante == null || bolsaInicializada) return;
        rotacionOriginalBolsa = bolsaFertilizante.localEulerAngles;
        escalaOriginalBolsa = bolsaFertilizante.localScale;
        bolsaInicializada = true;
    }

    private void IniciarAgitado()
    {
        if (bolsaFertilizante == null) return;
        InicializarBolsa();

        tweenAgitado?.Kill();
        bolsaFertilizante.localEulerAngles = rotacionOriginalBolsa + new Vector3(0f, 0f, -anguloAgitado);
        tweenAgitado = bolsaFertilizante
            .DOLocalRotate(rotacionOriginalBolsa + new Vector3(0f, 0f, anguloAgitado), duracionAgitado)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void DetenerAgitado(bool inmediato)
    {
        if (bolsaFertilizante == null || !bolsaInicializada) return;

        tweenAgitado?.Kill();
        tweenAgitado = null;
        bolsaFertilizante.DOKill();

        if (inmediato)
        {
            bolsaFertilizante.localEulerAngles = rotacionOriginalBolsa;
            bolsaFertilizante.localScale = escalaOriginalBolsa;
        }
        else
        {
            bolsaFertilizante.DOLocalRotate(rotacionOriginalBolsa, duracionRegreso).SetEase(Ease.OutBack);
            bolsaFertilizante.DOScale(escalaOriginalBolsa, duracionRegreso);
        }
    }

    private void SaltoBolsa()
    {
        if (bolsaFertilizante == null || !bolsaInicializada) return;

        bolsaFertilizante.localScale = escalaOriginalBolsa;
        bolsaFertilizante.DOPunchScale(escalaOriginalBolsa * fuerzaSaltoGota, frecuenciaGotas * 0.9f, 6, 0.5f);
    }

    private void CrearGotaFertilizante()
    {
        if (prefabFertilizante == null || contenedorPanel == null || puntoSalidaFertilizante == null) return;

        GameObject nuevaGota = Instantiate(prefabFertilizante, contenedorPanel);
        RectTransform rectGota = nuevaGota.GetComponent<RectTransform>();
        rectGota.anchoredPosition = puntoSalidaFertilizante.anchoredPosition;

        Graphic grafico = nuevaGota.GetComponent<Graphic>();
        if (grafico != null) grafico.raycastTarget = false;

        GotaFertilizante scriptGota = nuevaGota.AddComponent<GotaFertilizante>();
        scriptGota.Inicializar(this, velocidadGotas, zonaColisionPlanta, true);

        gotasActivas.RemoveAll(g => g == null);
        gotasActivas.Add(nuevaGota);

        SaltoBolsa();
    }

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

    public void RegistrarImpactoExitoso(Vector2 posicionImpacto = default)
    {
        if (minijuegoTerminado) return;

        progresoFertilizante += incrementoPorGota;
        progresoFertilizante = Mathf.Clamp(progresoFertilizante, 0f, metaRequerida);

        ActualizarTextoUI(); 

        if (particulasImpactoPlanta != null)
        {
            if (posicionImpacto != default && contenedorPanel != null)
            {
                particulasImpactoPlanta.GetComponent<RectTransform>().anchoredPosition = posicionImpacto;
            } 
            particulasImpactoPlanta.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particulasImpactoPlanta.Play();
        }   

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
        IniciarAgitado();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        estaPresionando = false;
        DetenerAgitado(false);
    }

    private void CompletarMinijuego()
    {
        minijuegoTerminado = true;
        estaPresionando = false;

        LimpiarGotas();
        DetenerAgitado(false);

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