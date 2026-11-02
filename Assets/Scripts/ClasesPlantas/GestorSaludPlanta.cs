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

    [Header("Moho y Plagas")]
    [SerializeField] private int cantidadMoho = 0;
    [SerializeField] private int cantidadPlagas = 0;
    [SerializeField] private int maxMoho = 8;
    [SerializeField] private int maxPlagas = 10;
    [SerializeField] private float danoPorMohoPorSegundo = 0.5f;
    [SerializeField] private float danoPorPlagaPorSegundo = 0.3f;

    [Header("Aparición de Moho y Plagas")]
    [SerializeField] private float segundosEntreChequeos = 20f;
    [SerializeField, Range(0f, 1f)] private float probabilidadMoho = 0.15f;
    [SerializeField, Range(0f, 1f)] private float probabilidadMohoConExcesoAgua = 0.5f;
    [SerializeField, Range(0f, 1f)] private float probabilidadPlagas = 0.15f;
    [SerializeField] private int maxPorBrote = 2;

    [Header("Afecciones Activas")]
    [SerializeField] private List<TipoAfeccion> afeccionesActivas = new List<TipoAfeccion>();

    private PlantaBase plantaBase;
    private float temporizadorInfestacion;

    public float NivelAgua => nivelAgua;
    public float TemperaturaAmbiente => temperaturaAmbiente;
    public bool TieneAfecciones => afeccionesActivas.Count > 0;
    public int CantidadMoho => cantidadMoho;
    public int CantidadPlagas => cantidadPlagas;

    private void Awake()
    {
        plantaBase = GetComponent<PlantaBase>();
    }

    private void Update()
    {
        if (plantaBase == null || !plantaBase.EstaViva) return;

        ProcesarAguaYHumdedad();
        ProcesarTemperatura();
        ProcesarInfestaciones();
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

    private void ProcesarInfestaciones()
    {
        temporizadorInfestacion += Time.deltaTime;
        if (temporizadorInfestacion < segundosEntreChequeos) return;

        temporizadorInfestacion = 0f;

        float chanceMoho = TieneAfeccion(TipoAfeccion.ExcesoDeAgua) ? probabilidadMohoConExcesoAgua : probabilidadMoho;

        if (Random.value < chanceMoho)
        {
            AgregarMoho(Random.Range(1, maxPorBrote + 1));
        }

        if (Random.value < probabilidadPlagas)
        {
            AgregarPlagas(Random.Range(1, maxPorBrote + 1));
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
                    danoTotal += danoPorMohoPorSegundo * Mathf.Max(1, cantidadMoho) * Time.deltaTime;
                    break;
                case TipoAfeccion.PlagaMoscas:
                    danoTotal += danoPorPlagaPorSegundo * Mathf.Max(1, cantidadPlagas) * Time.deltaTime;
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

    public void AgregarMoho(int cantidad)
    {
        cantidadMoho = Mathf.Min(cantidadMoho + cantidad, maxMoho);
        ActualizarAfeccionesInfestacion();
    }

    public void QuitarMoho(int cantidad)
    {
        cantidadMoho = Mathf.Max(0, cantidadMoho - cantidad);
        ActualizarAfeccionesInfestacion();
    }

    public void AgregarPlagas(int cantidad)
    {
        cantidadPlagas = Mathf.Min(cantidadPlagas + cantidad, maxPlagas);
        ActualizarAfeccionesInfestacion();
    }

    public void QuitarPlagas(int cantidad)
    {
        cantidadPlagas = Mathf.Max(0, cantidadPlagas - cantidad);
        ActualizarAfeccionesInfestacion();
    }

    private void ActualizarAfeccionesInfestacion()
    {
        if (cantidadMoho > 0) AgregarAfeccion(TipoAfeccion.Hongos);
        else RemoverAfeccion(TipoAfeccion.Hongos);

        if (cantidadPlagas > 0) AgregarAfeccion(TipoAfeccion.PlagaMoscas);
        else RemoverAfeccion(TipoAfeccion.PlagaMoscas);
    }

    public void AgregarAfeccion(TipoAfeccion nuevaAfeccion)
    {
        if (!afeccionesActivas.Contains(nuevaAfeccion))
        {
            afeccionesActivas.Add(nuevaAfeccion);
            Debug.Log($"{gameObject.name} adquirió la afección: {nuevaAfeccion}");
            SincronizarVisual();
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

            SincronizarVisual();
        }
    }

    private void SincronizarVisual()
    {
        if (plantaBase == null) return;

        PlantaBase.EstadoAfeccion estado = PlantaBase.EstadoAfeccion.Ninguna;

        if (TieneAfeccion(TipoAfeccion.Hongos)) estado = PlantaBase.EstadoAfeccion.ConHongos;
        else if (TieneAfeccion(TipoAfeccion.Sequia)) estado = PlantaBase.EstadoAfeccion.MuySeca;
        else if (TieneAfeccion(TipoAfeccion.ExcesoDeAgua)) estado = PlantaBase.EstadoAfeccion.MuyMojada;

        if (plantaBase.AfeccionActual != estado)
        {
            plantaBase.CambiarAfeccion(estado);
        }
    }

    public bool TieneAfeccion(TipoAfeccion afeccion) => afeccionesActivas.Contains(afeccion);

    public void RestaurarEstado(float agua, float temp, int moho, int plagas)
    {
        nivelAgua = agua;
        temperaturaAmbiente = temp;
        cantidadMoho = Mathf.Clamp(moho, 0, maxMoho);
        cantidadPlagas = Mathf.Clamp(plagas, 0, maxPlagas);
        ActualizarAfeccionesInfestacion();
    }

    [ContextMenu("Probar: agregar 3 moho")]
    private void ProbarAgregarMoho()
    {
        AgregarMoho(3);
    }

    [ContextMenu("Probar: agregar 3 plagas")]
    private void ProbarAgregarPlagas()
    {
        AgregarPlagas(3);
    }
}