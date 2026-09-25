using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject Menu; 
    public GameObject Ajustes;
    public GameObject Almanaque;
    public GameObject Creditos;

    public static bool abrirAjustesAlCargar = false;
    public static bool vieneDePausa = false;

    private void Start()
    {
        if (abrirAjustesAlCargar)
        {
            OpenAjustesPanel();
            abrirAjustesAlCargar = false;
        }
        else 
        {
            OpenMenuPanel();
        }
    }

    public void VolverDesdeAjustes()
    {
        if (vieneDePausa)
        {
            vieneDePausa = false; 
            PauseMenu.abrirPausaAlCargar = true; 
            SceneManager.LoadScene("Gameplay");
        }
        else
        {
            OpenMenuPanel(); 
        }
    }
    public void PlayGame() 
    {
        SceneManager.LoadScene("Gameplay");  
    }

    public void OpenMenuPanel() => MostrarSolo(Menu);

    public void OpenAjustesPanel() => MostrarSolo(Ajustes);
   

    public void OpenAlmanequePanel() => MostrarSolo(Almanaque);

    public void OpenCreditosPanel() => MostrarSolo(Creditos);

   
    private void MostrarSolo(GameObject panelAmostrar)
    {
        if (Menu != null) Menu.SetActive(Menu == panelAmostrar);
        if (Ajustes != null) Ajustes.SetActive(Ajustes == panelAmostrar);
        if (Almanaque != null) Almanaque.SetActive(Almanaque == panelAmostrar);
        if (Creditos != null) Creditos.SetActive(Creditos == panelAmostrar);
    }
}
