using UnityEngine;

public class ActionMenuController : MonoBehaviour
{
    public static ActionMenuController Instance;

    [Header("Referencias UI")]
    [SerializeField] private GameObject panelMenu;
    [SerializeField] private RectTransform menuRectTransform;

    [Header("Configuración de Cuidado")]
    [SerializeField] private float cantidadAguaRiego = 30f;
    [SerializeField] private float cantidadAguaExtraer = 30f;
    [SerializeField] private float segundosFertilizante = 10f;

    private CasillaUI casillaActual;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        OcultarMenu();
    }

    /// <summary>
    /// Activa el menú y lo posiciona sobre la casilla tocada.
    /// </summary>
    public void MostrarMenu(RectTransform casillaRect, CasillaUI casilla)
    {
        casillaActual = casilla;
        panelMenu.SetActive(true);

        // Ubicar el panel en la posición de la casilla en pantalla
        Vector3 posicionPantalla = casillaRect.position;
        menuRectTransform.position = posicionPantalla;
    }

    public void OcultarMenu()
    {
        if (panelMenu != null)
            panelMenu.SetActive(false);

        casillaActual = null;
    }

    // --- ACCIONES DE LOS BOTONES ---

    public void OnBotonRegar()
    {
        if (casillaActual != null)
        {
            GestorSaludPlanta gestorSalud = casillaActual.GetComponentInChildren<GestorSaludPlanta>();
            if (gestorSalud != null)
            {
                gestorSalud.Regar(cantidadAguaRiego);
                Debug.Log("Planta regada desde el menú.");
            }
            else
            {
                Debug.LogWarning("No hay una planta para regar en esta casilla.");
            }
        }
        OcultarMenu();
    }

    public void OnBotonSacaAgua()
    {
        if (casillaActual != null)
        {
            GestorSaludPlanta gestorSalud = casillaActual.GetComponentInChildren<GestorSaludPlanta>();
            if (gestorSalud != null)
            {
                gestorSalud.ExtraerAgua(cantidadAguaExtraer);
                Debug.Log("Agua extraída desde el menú.");
            }
            else
            {
                Debug.LogWarning("No hay una planta en esta casilla para extraer agua.");
            }
        }
        OcultarMenu();
    }

    public void OnBotonFertilizar()
    {
        if (casillaActual != null)
        {
            PlantaBase planta = casillaActual.GetComponentInChildren<PlantaBase>();
            if (planta != null)
            {
                if (!planta.EsAdulta)
                {
                    planta.AcelerarCrecimiento(segundosFertilizante);
                    Debug.Log("Planta fertilizada desde el menú.");
                }
                else
                {
                    Debug.Log($"{planta.Nombre} ya está completamente adulta.");
                }
            }
            else
            {
                Debug.LogWarning("No hay una planta para fertilizar en esta casilla.");
            }
        }
        OcultarMenu();
    }

    public void OnBotonBasura()
    {
        if (casillaActual != null)
        {
            PlantaBase planta = casillaActual.GetComponentInChildren<PlantaBase>();
            MacetaUI maceta = casillaActual.GetComponentInChildren<MacetaUI>();

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

                Debug.Log("Planta muerta tirada a la basura desde el menú.");

                GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
                if (grilla != null) grilla.GuardarEstadoGrilla();
            }
            else
            {
                Debug.LogWarning("No hay planta muerta para desechar en esta casilla.");
            }
        }
        OcultarMenu();
    }
}