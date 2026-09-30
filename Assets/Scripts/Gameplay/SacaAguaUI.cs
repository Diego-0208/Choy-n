using UnityEngine;
using UnityEngine.EventSystems;

public class SacaAguaUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Configuración de Secado")]
    [SerializeField] private float cantidadAguaAExtraer = 30f;

    public void OnPointerClick(PointerEventData eventData)
    {
        EjecutarSacaAgua();
    }

    public void EjecutarSacaAgua()
    {
        CasillaUI casilla = MenuCuidadosUI.Instance?.CasillaSeleccionada;

        if (casilla != null)
        {
            GestorSaludPlanta gestorSalud = casilla.GetComponentInChildren<GestorSaludPlanta>();

            if (gestorSalud != null)
            {
                gestorSalud.ExtraerAgua(cantidadAguaAExtraer);
                Debug.Log($"¡Se extrajo agua! Nivel de agua actual de {gestorSalud.gameObject.name}: {gestorSalud.NivelAgua}");
            }
            else
            {
                Debug.Log("No se encontró ninguna planta en esta casilla para extraer agua.");
            }
        }

        MenuCuidadosUI.Instance?.OcultarPanel();
    }
}