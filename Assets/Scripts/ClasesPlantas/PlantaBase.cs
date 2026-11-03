using System;
using UnityEngine;
using UnityEngine.UI;

public class PlantaBase : MonoBehaviour
{
    public enum DireccionCrecimiento
    {
        HaciaArriba,
        HaciaAbajo
    }

    public enum EstadoAfeccion
    {
        Ninguna,
        MuySeca,
        ConHongos,
        MuyMojada
    }

    [Flags]
    public enum Minijuego
    {
        Ninguno = 0,
        Riego = 1 << 0,
        Fertilizante = 1 << 1,
        Cosecha = 1 << 2,
        Trasplante = 1 << 3,
        Moho = 1 << 4,
        Plagas = 1 << 5
    }

    [Header("Configuración General")]
    [SerializeField] protected string nombrePlanta = "Planta";
    [SerializeField] protected TipoPlanta tipoDePlanta = TipoPlanta.Comun;
    [Tooltip("Puntos de vida. El daño de GestorSaludPlanta se resta de aquí.")]
    [SerializeField] protected float tiempoDeVidaMaximo = 100f;
    [SerializeField] protected string[] nombresEtapas;

    [Header("Sprites por Etapas de Crecimiento")]
    [Tooltip("Element 0 = Brote, Element 1 = Crecimiento, Element 2 = Adulta")]
    [SerializeField] protected Sprite[] spritesEtapas;

    [Header("Acercamiento (imagen grande en los minijuegos)")]
    [Tooltip("Versión en alta calidad de cada etapa, mismo orden que Sprites Etapas. Si falta, se usa el sprite normal.")]
    [SerializeField] protected Sprite[] spritesAcercamiento;
    [Tooltip("Opcional: acercamiento con afección por etapa (Element 0 = brote...). Si falta, se tiñe el de arriba.")]
    [SerializeField] protected AcercamientoAfeccion[] acercamientoAfecciones;

    [Serializable]
    public class AcercamientoAfeccion
    {
        public Sprite seca;
        public Sprite hongos;
        public Sprite mojada;
        public Sprite muerta;
    }

    [Header("Afecciones: sprites de la etapa ADULTA")]
    [Tooltip("Solo se usan cuando la planta es adulta. Para las otras etapas usa los arreglos de abajo o el tinte.")]
    [SerializeField] protected Sprite spriteMuySeca;
    [SerializeField] protected Sprite spriteConHongos;
    [SerializeField] protected Sprite spriteMuyMojada;
    [SerializeField] protected Sprite spritePlantaMuerta;

    [Header("Afecciones por etapa (opcional, mismo orden que Sprites Etapas)")]
    [SerializeField] protected Sprite[] spritesSecaPorEtapa;
    [SerializeField] protected Sprite[] spritesHongosPorEtapa;
    [SerializeField] protected Sprite[] spritesMojadaPorEtapa;
    [SerializeField] protected Sprite[] spritesMuertaPorEtapa;

    [Header("Tinte cuando no hay sprite de afección para la etapa")]
    [SerializeField] protected Color tinteSeca = new Color(0.85f, 0.72f, 0.45f);
    [SerializeField] protected Color tinteHongos = new Color(0.78f, 0.82f, 0.78f);
    [SerializeField] protected Color tinteMojada = new Color(0.70f, 0.80f, 1.00f);
    [SerializeField] protected Color tinteMuerta = new Color(0.45f, 0.38f, 0.30f);

    [Header("Configuración de Crecimiento (en HORAS de juego)")]
    [SerializeField] protected DireccionCrecimiento direccionCrecimiento = DireccionCrecimiento.HaciaArriba;
    [Tooltip("Horas de juego que dura cada etapa antes de pasar a la siguiente.")]
    [SerializeField] protected float tiempoParaCrecer = 12f;
    [SerializeField] protected int etapaMaxima = 2;
    [SerializeField] protected Vector2 incrementoTamanoPorEtapa = new Vector2(0f, 40f);

    [Header("Minijuegos")]
    [SerializeField]
    protected Minijuego minijuegosPermitidos =
        Minijuego.Riego | Minijuego.Fertilizante | Minijuego.Cosecha |
        Minijuego.Trasplante | Minijuego.Moho | Minijuego.Plagas;
    [Tooltip("Etapa en la que aparece el botón Trasplantar (0 = brote).")]
    [SerializeField] protected int etapaTrasplante = 0;
    [Tooltip("Si está activo, la planta no pasa de la etapa de trasplante hasta que la trasplanten.")]
    [SerializeField] protected bool esperarTrasplanteParaCrecer = true;

    [Header("Cosecha (en HORAS de juego)")]
    [Tooltip("Horas de juego que debe esperar una planta adulta para cosecharse de nuevo (0 = sin espera).")]
    [SerializeField] protected float tiempoEntreCosechas = 24f;
    [SerializeField] protected float temporizadorCosecha = 0f;

    [Header("Estado Actual")]
    [SerializeField] protected float tiempoDeVidaActual;
    [SerializeField] protected int etapaActual = 0;
    [SerializeField] protected float temporizadorCrecimiento = 0f;
    [SerializeField] protected EstadoAfeccion afeccionActual = EstadoAfeccion.Ninguna;
    protected bool estaViva = true;

    private RectTransform rectTransform;
    private Image imagenPlanta;
    private GestorSaludPlanta gestorSalud;

    private const float PASO_SIMULACION_HORAS = 0.25f;

    public TipoPlanta Tipo => tipoDePlanta;
    public string Nombre => nombrePlanta;
    public bool EstaViva => estaViva;
    public bool EsAdulta => etapaActual >= etapaMaxima;
    public int EtapaMaxima => etapaMaxima;
    public float TiempoParaCrecer => tiempoParaCrecer;
    public float VidaMaxima => tiempoDeVidaMaximo;
    public bool EsperandoTrasplante => CrecimientoBloqueado;

    public string NombreEtapa(string[] porDefecto)
    {
        string[] lista = nombresEtapas != null && nombresEtapas.Length > 0 ? nombresEtapas : porDefecto;
        if (lista == null || lista.Length == 0) return "Etapa " + etapaActual;
        return lista[Mathf.Clamp(etapaActual, 0, lista.Length - 1)];
    }
    public float TiempoDeVidaActual => tiempoDeVidaActual;
    public int EtapaActual => etapaActual;
    public float TemporizadorCrecimiento => temporizadorCrecimiento;
    public float TemporizadorCosecha => temporizadorCosecha;
    public bool PuedeCosechar => estaViva && EsAdulta && temporizadorCosecha <= 0f;
    public EstadoAfeccion AfeccionActual => afeccionActual;
    public bool YaTrasplantada => MacetaActual != null && !MacetaActual.EsPequena;

    private MacetaUI macetaCache;
    public MacetaUI MacetaActual
    {
        get
        {
            Transform padre = transform.parent;
            if (macetaCache == null || macetaCache.transform != padre)
                macetaCache = padre != null ? padre.GetComponent<MacetaUI>() : null;
            return macetaCache;
        }
    }

    private bool EnEnvasePequeno => MacetaActual != null && MacetaActual.EsPequena;

    public bool Permite(Minijuego minijuego) => (minijuegosPermitidos & minijuego) != 0;

    public bool NecesitaTrasplante =>
    estaViva && Permite(Minijuego.Trasplante) && EnEnvasePequeno && etapaActual >= etapaTrasplante;

    public Sprite SpriteAcercamiento
    {
        get
        {
            ObtenerVisual(true, out Sprite sprite, out _);
            return sprite;
        }
    }


    public void ObtenerVisual(bool acercamiento, out Sprite sprite, out Color color)
    {
        if (imagenPlanta == null) imagenPlanta = GetComponent<Image>();
        sprite = imagenPlanta != null ? imagenPlanta.sprite : null;
        color = imagenPlanta != null ? imagenPlanta.color : Color.white;

        if (!acercamiento) return;

        Sprite especifico = AcercamientoDeEstado();
        if (especifico != null)
        {
            sprite = especifico;
            color = Color.white;
            return;
        }

        Sprite hqEtapa = SpritePorEtapa(spritesAcercamiento);
        if (hqEtapa != null)
        {
            sprite = hqEtapa;
            color = TinteEstado();
        }
    }

    private Color TinteEstado()
    {
        if (!estaViva) return tinteMuerta;
        switch (afeccionActual)
        {
            case EstadoAfeccion.MuySeca: return tinteSeca;
            case EstadoAfeccion.ConHongos: return tinteHongos;
            case EstadoAfeccion.MuyMojada: return tinteMojada;
            default: return Color.white;
        }
    }

    private Sprite AcercamientoDeEstado()
    {
        if (acercamientoAfecciones == null || etapaActual >= acercamientoAfecciones.Length) return null;
        AcercamientoAfeccion a = acercamientoAfecciones[etapaActual];
        if (a == null) return null;

        if (!estaViva) return a.muerta;
        switch (afeccionActual)
        {
            case EstadoAfeccion.MuySeca: return a.seca;
            case EstadoAfeccion.ConHongos: return a.hongos;
            case EstadoAfeccion.MuyMojada: return a.mojada;
            default: return null;
        }
    }

    public float HorasHastaCosecha
    {
        get
        {
            if (!estaViva || !Permite(Minijuego.Cosecha)) return -1f;
            float horas = Mathf.Max(0f, temporizadorCosecha);
            if (!EsAdulta) horas += (etapaMaxima - etapaActual) * tiempoParaCrecer - temporizadorCrecimiento;
            return Mathf.Max(0f, horas);
        }
    }

    public float HorasHastaTrasplante
    {
        get
        {
            if (!estaViva || !Permite(Minijuego.Trasplante) || !EnEnvasePequeno) return -1f;
            if (etapaActual >= etapaTrasplante) return 0f;
            return (etapaTrasplante - etapaActual) * tiempoParaCrecer - temporizadorCrecimiento;
        }
    }

    protected virtual void Awake()
    {
        imagenPlanta = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        gestorSalud = GetComponent<GestorSaludPlanta>();
    }

    protected virtual void Start()
    {
        if (tiempoDeVidaActual <= 0 && estaViva)
        {
            tiempoDeVidaActual = tiempoDeVidaMaximo;
        }

        AjustarPivoteSegunDireccion();
        ActualizarSpriteVisual();
    }

    protected virtual void Update()
    {
        if (!estaViva || TimeManager.Instance == null) return;

        Avanzar(TimeManager.Instance.DeltaHoras);
    }

    public virtual void Avanzar(float horas)
    {
        if (!estaViva || horas <= 0f) return;

        ProcesarCrecimiento(horas);
        ProcesarTemporizadorCosecha(horas);
    }


    public void SimularTiempo(float horasTotales)
    {
        if (gestorSalud == null) gestorSalud = GetComponent<GestorSaludPlanta>();

        while (horasTotales > 0f && estaViva)
        {
            float paso = Mathf.Min(PASO_SIMULACION_HORAS, horasTotales);
            Avanzar(paso);
            if (gestorSalud != null && gestorSalud.enabled) gestorSalud.Avanzar(paso);
            horasTotales -= paso;
        }
    }


    public virtual void ActualizarSpriteVisual()
    {
        if (imagenPlanta == null) imagenPlanta = GetComponent<Image>();
        if (imagenPlanta == null) return;

        Sprite spriteEtapa = SpritePorEtapa(spritesEtapas);
        Sprite spriteAfeccion = null;
        Color tinte = Color.white;

        if (!estaViva)
        {
            spriteAfeccion = SpriteAfeccion(spritesMuertaPorEtapa, spritePlantaMuerta);
            tinte = tinteMuerta;
        }
        else
        {
            switch (afeccionActual)
            {
                case EstadoAfeccion.MuySeca:
                    spriteAfeccion = SpriteAfeccion(spritesSecaPorEtapa, spriteMuySeca);
                    tinte = tinteSeca;
                    break;
                case EstadoAfeccion.ConHongos:
                    spriteAfeccion = SpriteAfeccion(spritesHongosPorEtapa, spriteConHongos);
                    tinte = tinteHongos;
                    break;
                case EstadoAfeccion.MuyMojada:
                    spriteAfeccion = SpriteAfeccion(spritesMojadaPorEtapa, spriteMuyMojada);
                    tinte = tinteMojada;
                    break;
            }
        }

        if (spriteAfeccion != null)
        {
            imagenPlanta.sprite = spriteAfeccion;
            imagenPlanta.color = Color.white;
        }
        else
        {
            if (spriteEtapa != null) imagenPlanta.sprite = spriteEtapa;
            imagenPlanta.color = tinte;
        }
    }

    private Sprite SpritePorEtapa(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) return null;
        int i = Mathf.Clamp(etapaActual, 0, sprites.Length - 1);
        return sprites[i];
    }

    private Sprite SpriteAfeccion(Sprite[] porEtapa, Sprite adulta)
    {
        if (porEtapa != null && etapaActual < porEtapa.Length && porEtapa[etapaActual] != null)
            return porEtapa[etapaActual];

        return EsAdulta ? adulta : null;
    }

    public void CambiarAfeccion(EstadoAfeccion nuevaAfeccion)
    {
        afeccionActual = nuevaAfeccion;
        ActualizarSpriteVisual();
    }

    public void RestaurarEstado(float vida, int etapa, float tempCrecimiento, bool viva, float tempCosecha = 0f, bool trasplantada = false)
    {
        tiempoDeVidaActual = vida;
        etapaActual = etapa;
        temporizadorCrecimiento = tempCrecimiento;
        temporizadorCosecha = Mathf.Max(0f, tempCosecha);
        estaViva = viva;


        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null && etapaActual > 0)
        {
            rectTransform.sizeDelta += incrementoTamanoPorEtapa * etapaActual;
        }

        if (!estaViva)
        {
            Morir();
        }
        else
        {
            ActualizarSpriteVisual();
        }
    }

    public void MarcarTrasplantada() { }

    private void AjustarPivoteSegunDireccion()
    {
        if (rectTransform == null) return;

        if (direccionCrecimiento == DireccionCrecimiento.HaciaArriba)
        {
            rectTransform.pivot = new Vector2(0.5f, 0f);
        }
        else
        {
            rectTransform.pivot = new Vector2(0.5f, 1f);
        }
    }

    private bool CrecimientoBloqueado =>
        esperarTrasplanteParaCrecer && NecesitaTrasplante;

    private void ProcesarCrecimiento(float horas)
    {
        if (EsAdulta || CrecimientoBloqueado) return;

        temporizadorCrecimiento += horas;

        while (!EsAdulta && !CrecimientoBloqueado && temporizadorCrecimiento >= tiempoParaCrecer)
        {
            temporizadorCrecimiento -= tiempoParaCrecer;
            AvanzarEtapa();
        }

        if (EsAdulta || CrecimientoBloqueado) temporizadorCrecimiento = Mathf.Min(temporizadorCrecimiento, tiempoParaCrecer);
    }

    private void AvanzarEtapa()
    {
        etapaActual++;

        if (rectTransform != null)
        {
            rectTransform.sizeDelta += incrementoTamanoPorEtapa;
        }

        ActualizarSpriteVisual();
        Debug.Log($"{nombrePlanta} ha crecido a la etapa {etapaActual}.");
    }

    public void AcelerarCrecimiento(float horasReducidas)
    {
        if (EsAdulta || !estaViva) return;
        ProcesarCrecimiento(horasReducidas);
    }

    private void ProcesarTemporizadorCosecha(float horas)
    {
        if (temporizadorCosecha > 0f)
        {
            temporizadorCosecha = Mathf.Max(0f, temporizadorCosecha - horas);
        }
    }

    public void RegistrarCosecha()
    {
        temporizadorCosecha = tiempoEntreCosechas;
    }

    public void AplicarDano(float cantidad)
    {
        if (!estaViva) return;

        tiempoDeVidaActual -= cantidad;

        if (tiempoDeVidaActual <= 0)
        {
            tiempoDeVidaActual = 0;
            Morir();
        }
    }

    public void RestablecerSalud()
    {
        tiempoDeVidaActual = tiempoDeVidaMaximo;
        afeccionActual = EstadoAfeccion.Ninguna;
        ActualizarSpriteVisual();
    }

    protected virtual void Morir()
    {
        estaViva = false;

        ActualizarSpriteVisual();

        GestorSaludPlanta salud = GetComponent<GestorSaludPlanta>();
        if (salud != null)
        {
            salud.enabled = false;
        }

        Debug.Log($"{nombrePlanta} ha muerto. Permanecerá en la maceta hasta ser desechada.");
    }

    public enum TipoPlanta
    {
        Comun,
        Vegetal,
        Medicinal,
        Ornamental,
        Frutal
    }
}