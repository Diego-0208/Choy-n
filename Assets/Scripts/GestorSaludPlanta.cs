using System.Collections.Generic;
using UnityEngine;

public enum TipoAfeccion
{
    Ninguna,
    Hongos,          // Requiere fungicida
    PlagaMoscas,     // Requiere insecticida
    ExcesoDeAgua,    // Raíces podridas por regar demasiado
    Sequia,          // Poca agua / planta seca
    HojasQuemadas,   // Daño por sol o calor
    Frio             // Daño por bajas temperaturas / heladas
}

public class GestorSaludPlanta : MonoBehaviour
{
    [Header("Niveles de Agua")]
    [Range(0f, 100f)] [SerializeField] private float nivelAgua = 50f;
    [SerializeField] private float consumoAguaPorSegundo = 1f;
    [SerializeField] private float umbralSequia = 15f;
    [SerializeField] private float umbralExcesoAgua = 85f;

    [Header("Control de Temperatura")]
    [SerializeField] private float temperaturaAmbiente = 20f; // En grados Celsius
    [SerializeField] private float umbralFrio = 5f;             // Por debajo de esto la planta sufre frío

    [Header("Afecciones Activas")]
    [SerializeField] private List<TipoAfeccion> afeccionesActivas = new List<TipoAfeccion>();

    private PlantaBase plantaBase;

    public float NivelAgua => nivelAgua;
    public float TemperaturaAmbiente => temperaturaAmbiente;

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
        nivelAgua = Mathf.Clamp(nivelAgua, 0f, 100f);

        // Control de Sequía
        if (nivelAgua <= umbralSequia && !TieneAfeccion(TipoAfeccion.Sequia))
        {
            AgregarAfeccion(TipoAfeccion.Sequia);
        }
        else if (nivelAgua > umbralSequia && TieneAfeccion(TipoAfeccion.Sequia))
        {
            RemoverAfeccion(TipoAfeccion.Sequia);
        }

        // Control de Exceso de Agua
        if (nivelAgua >= umbralExcesoAgua && !TieneAfeccion(TipoAfeccion.ExcesoDeAgua))
        {
            AgregarAfeccion(TipoAfeccion.ExcesoDeAgua);
        }
        else if (nivelAgua < umbralExcesoAgua && TieneAfeccion(TipoAfeccion.ExcesoDeAgua))
        {
            RemoverAfeccion(TipoAfeccion.ExcesoDeAgua);
        }
    }

    private void ProcesarTemperatura()
    {
        // Si la temperatura cae por debajo del umbral, se congela/enfría
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
                    danoTotal += 2f * Time.deltaTime; // Se hiela y pierde salud progresivamente
                    break;
            }
        }

        if (danoTotal > 0)
        {
            plantaBase.AplicarDanoOEnvejecimientoForzado(danoTotal);
        }
    }

    public void ActualizarTemperatura(float nuevaTemperatura)
    {
        temperaturaAmbiente = nuevaTemperatura;
    }

    public void Regar(float cantidad)
    {
        nivelAgua += cantidad;
    }

    public void AgregarAfeccion(TipoAfeccion nuevaAfeccion)
    {
        if (!afeccionesActivas.Contains(nuevaAfeccion))
        {
            afeccionesActivas.Add(nuevaAfeccion);
            Debug.Log($"{gameObject.name} ahora tiene: {nuevaAfeccion}");
        }
    }

    public void RemoverAfeccion(TipoAfeccion afeccionACurar)
    {
        if (afeccionesActivas.Contains(afeccionACurar))
        {
            afeccionesActivas.Remove(afeccionACurar);
            Debug.Log($"{gameObject.name} se curó de: {afeccionACurar}");
        }
    }

    public bool TieneAfeccion(TipoAfeccion afeccion) => afeccionesActivas.Contains(afeccion);
}