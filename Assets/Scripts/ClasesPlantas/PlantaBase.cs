using UnityEngine;

public class PlantaBase : MonoBehaviour
{
    [Header("Configuración General")]
    [SerializeField] protected string nombrePlanta = "Planta";
    [SerializeField] protected TipoPlanta tipoDePlanta = TipoPlanta.Comun;
    [SerializeField] protected float tiempoDeVidaMaximo = 60f; 

    [Header("Estado Actual")]
    [SerializeField] protected float tiempoDeVidaActual;
    protected bool estaViva = true;

    public TipoPlanta Tipo => tipoDePlanta;
    public string Nombre => nombrePlanta;
    public bool EstaViva => estaViva;

    protected virtual void Start()
    {
        tiempoDeVidaActual = tiempoDeVidaMaximo;
    }

    protected virtual void Update()
    {
        if (!estaViva) return;
        Envejecer();
    }

    protected virtual void Envejecer()
    {
        tiempoDeVidaActual -= Time.deltaTime;

        if (tiempoDeVidaActual <= 0)
        {
            Morir();
        }
    }

    protected virtual void Morir()
    {
        estaViva = false;
        Debug.Log($"{nombrePlanta} ha muerto.");
        Destroy(gameObject);
    }

    public void AplicarDanoOEnvejecimientoForzado(float cantidad)
    {
        tiempoDeVidaActual -= cantidad;

        if (tiempoDeVidaActual <= 0)
        {
            Morir();
        }
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