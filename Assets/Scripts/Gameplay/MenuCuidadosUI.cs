using UnityEngine;

public class MenuCuidadosUI : MonoBehaviour
{
    public static MenuCuidadosUI Instance;

    [Header("Referencia al Panel de Opciones")]
    [SerializeField] private GameObject panelOpciones;

    // Propiedad que guarda la casilla que el jugador tocó
    public CasillaUI CasillaSeleccionada { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        OcultarPanel();
    }


    // Muestra el panel y guarda la casilla seleccionada.
    public void MostrarPanel(CasillaUI casilla)
    {
        CasillaSeleccionada = casilla;

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);
        }
    }

    // Desactiva el panel y limpia la casilla seleccionada.

    public void OcultarPanel()
    {
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        CasillaSeleccionada = null;
    }
}