using UnityEngine;
using UnityEngine.EventSystems;

public class FertilizanteUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Configuración Fertilizante")]
    [SerializeField] private float segundosAcelerados = 10f;

    [Header("Minijuego")]
    [Tooltip("Si está activo y GestorMinijuegosPrincipal tiene asignado el minijuego de fertilizante, este botón abre el minijuego en vez de fertilizar directamente.")]
    [SerializeField] private bool usarMinijuego = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (usarMinijuego)
        {
            GestorMinijuegosPrincipal gestor = FindFirstObjectByType<GestorMinijuegosPrincipal>();
            if (gestor != null && gestor.TieneMinijuegoFertilizante)
            {
                gestor.AbrirFertilizante();
                return;
            }
        }

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