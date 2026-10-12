using UnityEngine;
using UnityEngine.EventSystems;

public class ZonaVentaUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Configuración de Porcentajes/Multiplicadores")]
    [SerializeField] private float factorJoven = 0.6f;
    [SerializeField] private float factorAdulto = 2.0f;

    [Header("Costos Originales de Compra")]
    public int costoFlor1 = 200;
    public int costoFlor2 = 400;
    public int costoFlor3 = 150;
    public int costoFlor4 = 700;

    [Header("Precios Manuales Personalizados")]
    [SerializeField] private bool usarPreciosManuales = false;
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

    public void OnPointerClick(PointerEventData eventData)
    {
        EjecutarVenta();
    }

    public void EjecutarVenta()
    {
        CasillaUI casilla = MenuCuidadosUI.Instance?.CasillaSeleccionada;

        if (casilla != null)
        {
            FlorItem_01 itemFlor = casilla.GetComponentInChildren<FlorItem_01>();
            PlantaBase planta = casilla.GetComponentInChildren<PlantaBase>();

            if (itemFlor != null && planta != null)
            {
                ProcesarVenta(planta.gameObject, itemFlor.tipoFlor, planta.EsAdulta, casilla);
            }
            else
            {
                Debug.Log("No hay una planta válida para vender en esta casilla.");
            }
        }

        MenuCuidadosUI.Instance?.OcultarPanel();
    }

    private void ProcesarVenta(GameObject objetoPlanta, int tipoFlor, bool esAdulta, CasillaUI casilla)
    {
        int precioFinal = CalcularPrecioVenta(tipoFlor, esAdulta);

        // Notificar a la maceta para que destruya la planta y resetee su estado
        MacetaUI macetaPadre = objetoPlanta.GetComponentInParent<MacetaUI>();

        if (macetaPadre != null)
        {
            macetaPadre.LimpiarPlanta();
        }
        else
        {
            Destroy(objetoPlanta);
        }

        casilla.VaciarCasilla();

        if (inventario != null)
        {
            inventario.Moneda += precioFinal;
            PlayerPrefs.SetInt("Moneda", inventario.Moneda);
            PlayerPrefs.Save();
            inventario.ActualizarTextos();

            Debug.Log($"¡Planta Tipo {tipoFlor} vendida! Monedas ganadas: +${precioFinal}");
        }
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
        return esAdulta ? Mathf.RoundToInt(costoOriginal * factorAdulto) : Mathf.RoundToInt(costoOriginal * factorJoven);
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