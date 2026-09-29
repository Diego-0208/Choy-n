using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Paneles de la UI")]
    [SerializeField] private GameObject panelPausa;
    [SerializeField] private GameObject panelAjustes;

    private void Start()
    {
        // Aseguramos el estado inicial de los paneles
        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelAjustes != null) panelAjustes.SetActive(false);
    }

    public void OpenPausePanel() 
    {
        panelPausa.SetActive(true);
        panelAjustes.SetActive(false);
        Time.timeScale = 0f;
    }

    public void Reanudar() 
    {
        panelPausa.SetActive(false); 
        panelAjustes.SetActive(false);
        Time.timeScale = 1f;
    }

    // Abre el panel de Ajustes en la misma escena sin cambiar a MainMenu
    public void AbrirAjustes() 
    {
        panelPausa.SetActive(false);
        panelAjustes.SetActive(true);
    }

    // Vuelve desde el panel de Ajustes al menú de Pausa
    public void VolverAPausa() 
    {
        panelAjustes.SetActive(false);
        panelPausa.SetActive(true);
    }

    public void IrAlMenuPrincipal() 
    {
        GuardarProgreso();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void GuardarProgreso()
    {
        GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
        if (grilla != null)
        {
            grilla.GuardarEstadoGrilla();
        }
    }
}