using UnityEngine;
using UnityEngine.EventSystems;

public class BasuraUI : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        EjecutarBasura();
    }

    public void EjecutarBasura()
    {
        CasillaUI casilla = MenuCuidadosUI.Instance?.CasillaSeleccionada;

        if (casilla != null)
        {
            PlantaBase planta = casilla.GetComponentInChildren<PlantaBase>();
            MacetaUI maceta = casilla.GetComponentInChildren<MacetaUI>();

            if (planta != null && !planta.EstaViva)
            {
                if (maceta != null)
                {
                    maceta.LimpiarPlanta();
                }
                else
                {
                    Destroy(planta.gameObject);
                }

                Debug.Log("Planta muerta tirada a la basura.");

                GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
                if (grilla != null) grilla.GuardarEstadoGrilla();
            }
            else
            {
                Debug.Log("No hay una planta muerta para tirar en esta casilla.");
            }
        }

        MenuCuidadosUI.Instance?.OcultarPanel();
    }
}