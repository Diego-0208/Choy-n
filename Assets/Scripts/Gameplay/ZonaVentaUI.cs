using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ZonaVentaUI : MonoBehaviour, IDropHandler
{
    [Header("Configuración de Porcentajes/Multiplicadores")]
    [Tooltip("Porcentaje que vale si la planta NO es adulta (0.6 = 60%)")]
    [SerializeField] private float factorJoven = 0.6f; 

    [Tooltip("Multiplicador si la planta SÍ es adulta (2.0 = x2)")]
    [SerializeField] private float factorAdulto = 2.0f; 

    [Header("Costos Originales de Compra (Referencia)")]
    public int costoFlor1 = 200;
    public int costoFlor2 = 400;
    public int costoFlor3 = 150;
    public int costoFlor4 = 700;

    [Header("Precios Manuales Personalizados (Opcional)")]
    [Tooltip("Si se activa, usará los precios manuales de abajo en lugar de calcular el 60% y el x2 automáticamente.")]
    [SerializeField] private bool usarPreciosManuales = false;

    [Header("Sobrescribir Precios (Si Usar Precios Manuales es TRUE)")]
    public int precioManualJovenFlor1 = 120; 
    public int precioManualAdultaFlor1 = 400; 

    public int precioManualJovenFlor2 = 240; 
    public int precioManualAdultaFlor2 = 800; 

    public int precioManualJovenFlor3 = 90;  
    public int precioManualAdultaFlor3 = 300; 

    public int precioManualJovenFlor4 = 420; 
    public int precioManualAdultaFlor4 = 1400; 

    private Compra_01 inventario;

    private void Awake()
    {
        inventario = FindFirstObjectByType<Compra_01>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject objetoArrastrado = eventData.pointerDrag;

        if (objetoArrastrado != null)
        {
            FlorItem_01 itemFlor = objetoArrastrado.GetComponent<FlorItem_01>();
            PlantaBase planta = objetoArrastrado.GetComponent<PlantaBase>();

            if (itemFlor != null && planta != null)
            {
                ProcesarVenta(objetoArrastrado, itemFlor.tipoFlor, planta.EsAdulta);
            }
        }
    }

    private void ProcesarVenta(GameObject objetoPlanta, int tipoFlor, bool esAdulta)
    {
        int precioFinal = CalcularPrecioVenta(tipoFlor, esAdulta);

        CasillaUI casilla = objetoPlanta.GetComponentInParent<CasillaUI>();
        if (casilla != null)
        {
            casilla.VaciarCasilla();
        }

        if (inventario != null)
        {
            inventario.Moneda += precioFinal;
            PlayerPrefs.SetInt("Moneda", inventario.Moneda);
            PlayerPrefs.Save();
            inventario.ActualizarTextos();

            Debug.Log($"¡Planta Tipo {tipoFlor} vendida! Adulta: {esAdulta} | Monedas ganadas: +${precioFinal}");
        }

        Destroy(objetoPlanta);
    }

    public int CalcularPrecioVenta(int tipoFlor, bool esAdulta)
    {
        if (usarPreciosManuales)
        {
            switch (tipoFlor)
            {
                case 1: return esAdulta ? precioManualAdultaFlor1 : precioManualJovenFlor1;
                case 2: return esAdulta ? precioManualAdultaFlor2 : precioManualJovenFlor2;
                case 3: return esAdulta ? precioManualAdultaFlor3 : precioManualJovenFlor3;
                case 4: return esAdulta ? precioManualAdultaFlor4 : precioManualJovenFlor4;
                default: return 0;
            }
        }

        int costoOriginal = ObtenerCostoOriginal(tipoFlor);

        if (esAdulta)
        {
            return Mathf.RoundToInt(costoOriginal * factorAdulto); 
        }
        else
        {
            return Mathf.RoundToInt(costoOriginal * factorJoven);  
        }
    }

    private int ObtenerCostoOriginal(int tipoFlor)
    {
        switch (tipoFlor)
        {
            case 1: return costoFlor1; 
            case 2: return costoFlor2; 
            case 3: return costoFlor3; 
            case 4: return costoFlor4; 
            default: return 0;
        }
    }
}