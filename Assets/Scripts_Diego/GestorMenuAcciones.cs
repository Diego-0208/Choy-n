using UnityEngine;

public class GestorMenuAcciones : MonoBehaviour
{
    [Header("Contenedor de Minijuegos en el Canvas")]
    public Transform panelMinijuegosPadre; // El objeto 'PanelMinijuegos' de tu jerarquía

    [Header("Prefabs de los Minijuegos")]
    public GameObject prefabMinijuegoFertilizante;
    public GameObject prefabMinijuegoRegar;
    public GameObject prefabMinijuegoCanasta;

    private GestorSaludPlanta plantaSeleccionada;

    // Método que recibe la planta cuando el usuario la toca
    public void SeleccionarPlanta(GestorSaludPlanta planta)
    {
        plantaSeleccionada = planta;
    }

    // --- BOTÓN: FERTILIZAR ---
    public void AbrirMinijuegoFertilizante()
    {
        if (ValidarPlantaSeleccionada()) return;
        InstanciarYAsignarMinijuego(prefabMinijuegoFertilizante);
    }

    // --- BOTÓN: REGAR ---
    public void AbrirMinijuegoRegar()
    {
        if (ValidarPlantaSeleccionada()) return;
        InstanciarYAsignarMinijuego(prefabMinijuegoRegar);
    }

    // --- BOTÓN: CANASTA ---
    public void AbrirMinijuegoCanasta()
    {
        if (ValidarPlantaSeleccionada()) return;
        InstanciarYAsignarMinijuego(prefabMinijuegoCanasta);
    }

    // Validación previa para asegurar que el usuario tocó una planta antes
    private bool ValidarPlantaSeleccionada()
    {
        if (plantaSeleccionada == null)
        {
            Debug.LogWarning("[Acción] ¡Primero debes hacer clic en una planta del huerto!");
            return true; // Retorna true si FALTA la planta (hay error)
        }
        return false;
    }

    // Instancia el prefab del minijuego y le inyecta la referencia de la planta
    private void InstanciarYAsignarMinijuego(GameObject prefabMinijuego)
    {
        if (prefabMinijuego == null || panelMinijuegosPadre == null) return;

        // Instancia el prefab contenedor dentro del panel de minijuegos
        GameObject instancia = Instantiate(prefabMinijuego, panelMinijuegosPadre);

        // Ajustar el RectTransform para que cubra todo el panel correctamente (Stretch)
        RectTransform rect = instancia.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        // Inyectamos la planta seleccionada al script controlador del minijuego
        ControlFertilizante controlFert = instancia.GetComponentInChildren<ControlFertilizante>();
        if (controlFert != null)
        {
            controlFert.AsignarPlantaObjetivo(plantaSeleccionada);
        }
    }
}