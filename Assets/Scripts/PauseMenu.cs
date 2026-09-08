using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject Pausa; 
    public static bool abrirPausaAlCargar = false;

    private void Start ()
    {
        if (abrirPausaAlCargar)
        {
            OpenPausePanel();
            abrirPausaAlCargar = false; 
        }

    }

    public void OpenPausePanel() 
    {
        Pausa.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Reanudar() 
    {
        Pausa.SetActive(false); 
        Time.timeScale = 1f;
    }

    public void Ajustes() 
    {
        Time.timeScale = 1f;
        MainMenu.abrirAjustesAlCargar = true;
        MainMenu.vieneDePausa = true;
        SceneManager.LoadScene("MainMenu");
    }
    public void Menu() 
    {
        Time.timeScale = 1f;
        MainMenu.abrirAjustesAlCargar = false;
        SceneManager.LoadScene("MainMenu");
    }
}
