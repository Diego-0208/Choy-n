using UnityEngine;
using UnityEngine.EventSystems;

public class PlantaInteractivaUI : MonoBehaviour, IPointerClickHandler
{
    private GestorSaludPlanta saludPlanta;
    private GestorMenuAcciones gestorMenuAcciones;

    private void Start()
    {
        // Busca el componente de salud en este objeto o en sus padres/hijos
        saludPlanta = GetComponent<GestorSaludPlanta>();
        if (saludPlanta == null)
            saludPlanta = GetComponentInParent<GestorSaludPlanta>();

        // Busca el gestor del menú en la escena de forma moderna
        gestorMenuAcciones = Object.FindFirstObjectByType<GestorMenuAcciones>();
    }

    // Este método de la UI detecta clics o toques sin necesidad de físicas ni colliders
    public void OnPointerClick(PointerEventData eventData)
    {
        if (gestorMenuAcciones != null && saludPlanta != null)
        {
            gestorMenuAcciones.SeleccionarPlanta(saludPlanta);
            Debug.Log($"[Planta UI] ¡Toque detectado en: {gameObject.name}! Planta seleccionada.");
        }
        else
        {
            Debug.LogWarning("[Planta UI] Faltan referencias: Asegúrate de que el objeto tenga GestorSaludPlanta y que exista un GestorMenuAcciones en la escena.");
        }
    }
}