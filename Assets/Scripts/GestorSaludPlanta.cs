using System.Collections.Generic;
using UnityEngine;

public class GestorSaludPlanta : MonoBehaviour
{
    [Header("Niveles de Agua")]
    [Range(0f, 100f)] [SerializeField] private float nivelAgua = 50f;
    [SerializeField] private float consumoAguaPorSegundo = 1f;
    [SerializeField] private float umbralSequia = 15f;
    [SerializeField] private float umbralExcesoAgua = 85f;

    [Header("Afecciones Activas")]
    [SerializeField] private List<TipoAfeccion> afeccionesActivas = new List<TipoAfeccion>();

    private PlantaBase plantaBase;

    public float NivelAgua => nivelAgua;

    private void Awake()
    {
        plantaBase = GetComponent<PlantaBase>();
    }

    private void Update()
    {
        if (plantaBase == null || !plantaBase.EstaViva) return;

        ProcesarAguaYHumdedad();
        ProcesarEfectosAfecciones();
    }

    private void ProcesarAguaYHumdedad()
    {
        // La planta consume agua constantemente
        nivelAgua -= consumoAguaPorSegundo * Time.deltaTime;
        nivelAgua = Mathf.Clamp(nivelAgua, 0f, 100f);

        // Control automático de Sequía
        if (nivelAgua <= umbralSequia && !TieneAfeccion(TipoAfeccion.Sequia))
        {
            AgregarAfeccion(TipoAfeccion.Sequia);
        }
        else if (nivelAgua > umbralSequia && TieneAfeccion(TipoAfeccion.Sequia))
        {
            RemoverAfeccion(TipoAfeccion.Sequia);
        }

        // Control automático de Exceso de Agua
        if (nivelAgua >= umbralExcesoAgua && !TieneAfeccion(TipoAfeccion.ExcesoDeAgua))
        {
            AgregarAfeccion(TipoAfeccion.ExcesoDeAgua);
        }
        else if (nivelAgua < umbralExcesoAgua && TieneAfeccion(TipoAfeccion.ExcesoDeAgua))
        {
            RemoverAfeccion(TipoAfeccion.ExcesoDeAgua);
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
                    danoTotal += 3f * Time.deltaTime; // Se seca y quema
                    break;
                case TipoAfeccion.ExcesoDeAgua:
                    danoTotal += 2.5f * Time.deltaTime; // Se pudre
                    break;
            }
        }

        // Acelera la muerte o resta vida en PlantaBase
        if (danoTotal > 0)
        {
            plantaBase.AplicarDanoOEnvejecimientoForzado(danoTotal);
        }
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


      public enum TipoAfeccion
    {
        Ninguna,
        Hongos,          // Requiere fungicida
        PlagaMoscas,     // Requiere insecticida
        ExcesoDeAgua,    // Raices podridas por regar demasiado
        Sequia,          // Poca agua / planta seca
        HojasQuemadas    // Dano por sol o calor
    }
}