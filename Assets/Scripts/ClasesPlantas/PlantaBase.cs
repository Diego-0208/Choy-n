using UnityEngine;

public class PlantaBase : MonoBehaviour
{
    public enum DireccionCrecimiento
    {
        HaciaArriba,
        HaciaAbajo
    }

    [Header("Configuración General")]
    [SerializeField] protected string nombrePlanta = "Planta";
    [SerializeField] protected TipoPlanta tipoDePlanta = TipoPlanta.Comun;
    [SerializeField] protected float tiempoDeVidaMaximo = 60f;

    [Header("Configuración de Crecimiento")]
    [SerializeField] protected DireccionCrecimiento direccionCrecimiento = DireccionCrecimiento.HaciaArriba;
    [SerializeField] protected float tiempoParaCrecer = 15f; 
    [SerializeField] protected int etapaMaxima = 2; 
    [SerializeField] protected Vector2 incrementoTamanoPorEtapa = new Vector2(0f, 40f); 
    [Header("Estado Actual")]
    [SerializeField] protected float tiempoDeVidaActual;
    [SerializeField] protected int etapaActual = 0;
    [SerializeField] protected float temporizadorCrecimiento = 0f;
    protected bool estaViva = true;

    private RectTransform rectTransform;

    public TipoPlanta Tipo => tipoDePlanta;
    public string Nombre => nombrePlanta;
    public bool EstaViva => estaViva;
    public bool EsAdulta => etapaActual >= etapaMaxima;

    protected virtual void Start()
    {
        tiempoDeVidaActual = tiempoDeVidaMaximo;
        rectTransform = GetComponent<RectTransform>();
        AjustarPivoteSegunDireccion();
    }

    protected virtual void Update()
    {
        if (!estaViva) return;

        ProcesarCrecimiento();
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

        Debug.Log($"{nombrePlanta} ha crecido a la etapa {etapaActual}.");
    }

    public void AcelerarCrecimiento(float segundosReducidos)
    {
        if (EsAdulta || !estaViva) return;

        temporizadorCrecimiento += segundosReducidos;
        Debug.Log($"¡Fertilizante aplicado a {nombrePlanta}! Tiempo reducido en {segundosReducidos}s.");

        
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
        Debug.Log($"{nombrePlanta} ha sido curada completamente.");
    }

    protected virtual void Morir()
    {
        estaViva = false;
        Debug.Log($"{nombrePlanta} ha muerto.");
        Destroy(gameObject);
    }

    public enum TipoPlanta
    {
        Comun,
        Vegetal,
        Medicinal,
        Ornamental,
        Frutal,
        Toxica
    }
}