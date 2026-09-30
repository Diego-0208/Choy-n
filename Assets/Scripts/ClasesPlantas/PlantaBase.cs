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

    [Header("Configuración General")]
    [SerializeField] protected string nombrePlanta = "Planta";
    [SerializeField] protected TipoPlanta tipoDePlanta = TipoPlanta.Comun;
    [SerializeField] protected float tiempoDeVidaMaximo = 60f;

    [Header("Sprites por Etapas de Crecimiento")]
    [Tooltip("Element 0 = Brote, Element 1 = Crecimiento, Element 2 = Adulta")]
    [SerializeField] protected Sprite[] spritesEtapas;

    [Header("Sprites por Afecciones/Estado de Salud")]
    [SerializeField] protected Sprite spriteMuySeca;
    [SerializeField] protected Sprite spriteConHongos;
    [SerializeField] protected Sprite spriteMuyMojada;

    [Header("Visual Muerte")]
    [SerializeField] protected Sprite spritePlantaMuerta;

    [Header("Configuración de Crecimiento")]
    [SerializeField] protected DireccionCrecimiento direccionCrecimiento = DireccionCrecimiento.HaciaArriba;
    [SerializeField] protected float tiempoParaCrecer = 15f; 
    [SerializeField] protected int etapaMaxima = 2; 
    [SerializeField] protected Vector2 incrementoTamanoPorEtapa = new Vector2(0f, 40f); 

    [Header("Estado Actual")]
    [SerializeField] protected float tiempoDeVidaActual;
    [SerializeField] protected int etapaActual = 0;
    [SerializeField] protected float temporizadorCrecimiento = 0f;
    [SerializeField] protected EstadoAfeccion afeccionActual = EstadoAfeccion.Ninguna;
    protected bool estaViva = true;

    private RectTransform rectTransform;
    private Image imagenPlanta;

    public TipoPlanta Tipo => tipoDePlanta;
    public string Nombre => nombrePlanta;
    public bool EstaViva => estaViva;
    public bool EsAdulta => etapaActual >= etapaMaxima;

    public float TiempoDeVidaActual => tiempoDeVidaActual;
    public int EtapaActual => etapaActual;
    public float TemporizadorCrecimiento => temporizadorCrecimiento;
    public EstadoAfeccion AfeccionActual => afeccionActual;

    protected virtual void Awake()
    {
        imagenPlanta = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
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
        if (!estaViva) return;

        ProcesarCrecimiento();
    }

    public virtual void ActualizarSpriteVisual()
    {
        if (imagenPlanta == null) imagenPlanta = GetComponent<Image>();
        if (imagenPlanta == null) return;

        if (!estaViva)
        {
            if (spritePlantaMuerta != null) imagenPlanta.sprite = spritePlantaMuerta;
            return;
        }

        switch (afeccionActual)
        {
            case EstadoAfeccion.MuySeca:
                if (spriteMuySeca != null) { imagenPlanta.sprite = spriteMuySeca; return; }
                break;
            case EstadoAfeccion.ConHongos:
                if (spriteConHongos != null) { imagenPlanta.sprite = spriteConHongos; return; }
                break;
            case EstadoAfeccion.MuyMojada:
                if (spriteMuyMojada != null) { imagenPlanta.sprite = spriteMuyMojada; return; }
                break;
        }

        if (spritesEtapas != null && spritesEtapas.Length > 0)
        {
            int indiceSprite = Mathf.Clamp(etapaActual, 0, spritesEtapas.Length - 1);
            if (spritesEtapas[indiceSprite] != null)
            {
                imagenPlanta.sprite = spritesEtapas[indiceSprite];
            }
        }
    }

    public void CambiarAfeccion(EstadoAfeccion nuevaAfeccion)
    {
        afeccionActual = nuevaAfeccion;
        ActualizarSpriteVisual();
    }

    public void RestaurarEstado(float vida, int etapa, float tempCrecimiento, bool viva)
    {
        tiempoDeVidaActual = vida;
        etapaActual = etapa;
        temporizadorCrecimiento = tempCrecimiento;
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

    private void ProcesarCrecimiento()
    {
        if (EsAdulta) return;

        temporizadorCrecimiento += Time.deltaTime;

        if (temporizadorCrecimiento >= tiempoParaCrecer)
        {
            AvanzarEtapa();
        }
    }

    private void AvanzarEtapa()
    {
        etapaActual++;
        temporizadorCrecimiento = 0f;

        if (rectTransform != null)
        {
            rectTransform.sizeDelta += incrementoTamanoPorEtapa;
        }

        ActualizarSpriteVisual();
        Debug.Log($"{nombrePlanta} ha crecido a la etapa {etapaActual}.");
    }

    public void AcelerarCrecimiento(float segundosReducidos)
    {
        if (EsAdulta || !estaViva) return;

        temporizadorCrecimiento += segundosReducidos;

        if (temporizadorCrecimiento >= tiempoParaCrecer)
        {
            AvanzarEtapa();
        }
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

        GestorSaludPlanta gestorSalud = GetComponent<GestorSaludPlanta>();
        if (gestorSalud != null)
        {
            gestorSalud.enabled = false;
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