using System.Collections.Generic;
using UnityEngine;

public enum TipoAfeccion
{
    Ninguna,
    Hongos,          
    PlagaMoscas,     
    ExcesoDeAgua,    
    Sequia,          
    HojasQuemadas,   
    Frio             
}

public class GestorSaludPlanta : MonoBehaviour
{
    [Header("Niveles de Agua")]
    [SerializeField] private float nivelAgua = 50f;
    [SerializeField] private float consumoAguaPorSegundo = 1f;
    [SerializeField] private float umbralSequia = 15f;
    [SerializeField] private float umbralExcesoAgua = 85f;

    [Header("Control de Temperatura")]
    [SerializeField] private float temperaturaAmbiente = 20f;
    [SerializeField] private float umbralFrio = 5f;

    [Header("Afecciones Activas")]
    [SerializeField] private List<TipoAfeccion> afeccionesActivas = new List<TipoAfeccion>();

    private PlantaBase plantaBase;

    public float NivelAgua => nivelAgua;
    public float TemperaturaAmbiente => temperaturaAmbiente;
    public bool TieneAfecciones => afeccionesActivas.Count > 0;

    private void Awake()
    {
        plantaBase = GetComponent<PlantaBase>();
    }

    private void Update()
    {
        if (plantaBase == null || !plantaBase.EstaViva) return;

        ProcesarAguaYHumdedad();
        ProcesarTemperatura();
        ProcesarEfectosAfecciones();
    }

    private void ProcesarAguaYHumdedad()
    {
        nivelAgua -= consumoAguaPorSegundo * Time.deltaTime;

        if (nivelAgua <= umbralSequia && !TieneAfeccion(TipoAfeccion.Sequia))
        {
            AgregarAfeccion(TipoAfeccion.Sequia);
        }
        else if (nivelAgua > umbralSequia && TieneAfeccion(TipoAfeccion.Sequia))
        {
            RemoverAfeccion(TipoAfeccion.Sequia);
        }

        if (nivelAgua >= umbralExcesoAgua && !TieneAfeccion(TipoAfeccion.ExcesoDeAgua))
        {
            AgregarAfeccion(TipoAfeccion.ExcesoDeAgua);
        }
        else if (nivelAgua < umbralExcesoAgua && TieneAfeccion(TipoAfeccion.ExcesoDeAgua))
        {
            RemoverAfeccion(TipoAfeccion.ExcesoDeAgua);
        }
    }
    public void ExtraerAgua(float cantidad)
    {
        nivelAgua -= cantidad;
        if (nivelAgua < 0f) nivelAgua = 0f;

        Debug.Log($"{gameObject.name} se le retiró agua. Agua actual: {nivelAgua}");
    }

    private void ProcesarTemperatura()
    {
        if (temperaturaAmbiente <= umbralFrio && !TieneAfeccion(TipoAfeccion.Frio))
        {
            AgregarAfeccion(TipoAfeccion.Frio);
        }
        else if (temperaturaAmbiente > umbralFrio && TieneAfeccion(TipoAfeccion.Frio))
        {
            RemoverAfeccion(TipoAfeccion.Frio);
        }
    }

    private void ProcesarEfectosAfecciones()
    {
        if (!TieneAfecciones) return; 

        float danoTotal = 0f;

        foreach (var afeccion in afeccionesActivas)
        {
            switch (afeccion)
            {
                case TipoAfeccion.Hongos:
                    danoTotal += 2f * Time.deltaTime;
                    break;
                case TipoAfeccion.PlagaMoscas:
                    danoTotal += 1.5f * Time.deltaTime;
                    break;
                case TipoAfeccion.Sequia:
                    danoTotal += 3f * Time.deltaTime;
                    break;
                case TipoAfeccion.ExcesoDeAgua:
                    danoTotal += 2.5f * Time.deltaTime;
                    break;
                case TipoAfeccion.Frio:
                    danoTotal += 2f * Time.deltaTime;
                    break;
            }
        }

        if (danoTotal > 0)
        {
            plantaBase.AplicarDano(danoTotal);
        }
    }

    public void ActualizarTemperatura(float nuevaTemperatura)
    {
        temperaturaAmbiente = nuevaTemperatura;
    }

    public void Regar(float cantidad)
    {
        nivelAgua += cantidad;
        if (nivelAgua < 0f) nivelAgua = 0f;
    }

    public void AgregarAfeccion(TipoAfeccion nuevaAfeccion)
    {
        if (!afeccionesActivas.Contains(nuevaAfeccion))
        {
            afeccionesActivas.Add(nuevaAfeccion);
            Debug.Log($"{gameObject.name} adquirió la afección: {nuevaAfeccion}");
        }
    }

    public void RemoverAfeccion(TipoAfeccion afeccionACurar)
    {
        if (afeccionesActivas.Contains(afeccionACurar))
        {
            afeccionesActivas.Remove(afeccionACurar);
            Debug.Log($"{gameObject.name} se curó de: {afeccionACurar}");

            if (!TieneAfecciones && plantaBase != null)
            {
                plantaBase.RestablecerSalud();
            }
        }
    }

    public bool TieneAfeccion(TipoAfeccion afeccion) => afeccionesActivas.Contains(afeccion);
}