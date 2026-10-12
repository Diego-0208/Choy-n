using UnityEngine;
using UnityEngine.EventSystems;

public class RegaderaUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Configuración de Riego")]
    [SerializeField] private float cantidadAgua = 30f;

    public void OnPointerClick(PointerEventData eventData)
    {
        EjecutarRiego();
    }

    public void EjecutarRiego()
    {
        CasillaUI casilla = MenuCuidadosUI.Instance?.CasillaSeleccionada;

        if (casilla != null)
        {
            GestorSaludPlanta gestorSalud = casilla.GetComponentInChildren<GestorSaludPlanta>();

            if (gestorSalud != null)
            {
                gestorSalud.Regar(cantidadAgua);
                Debug.Log($"¡Regado exitoso! Agua actual de {gestorSalud.gameObject.name}: {gestorSalud.NivelAgua}");
            }
            else
            {
                Debug.Log("No se encontró ninguna planta en esta casilla para regar.");
            }
        }

        MenuCuidadosUI.Instance?.OcultarPanel();
    }
}