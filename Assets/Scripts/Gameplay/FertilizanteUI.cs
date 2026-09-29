using UnityEngine;
using UnityEngine.EventSystems;

public class FertilizanteUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Configuración Fertilizante")]
    [SerializeField] private float segundosAcelerados = 10f;

    public void OnPointerClick(PointerEventData eventData)
    {
        EjecutarFertilizar();
    }

    public void EjecutarFertilizar()
    {
        CasillaUI casilla = MenuCuidadosUI.Instance?.CasillaSeleccionada;

        if (casilla != null)
        {
            PlantaBase planta = casilla.GetComponentInChildren<PlantaBase>();

            if (planta != null)
            {
                if (!planta.EsAdulta)
                {
                    planta.AcelerarCrecimiento(segundosAcelerados);
                    Debug.Log("Planta fertilizada con éxito.");
                }
                else
                {
                    Debug.Log($"{planta.Nombre} ya está completamente adulta.");
                }
            }
            else
            {
                Debug.Log("No se encontró ninguna planta para fertilizar.");
            }
        }

        MenuCuidadosUI.Instance?.OcultarPanel();
    }
}