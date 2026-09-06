using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject Pausa;

    public void OpenPausePanel() 
    {
        Pausa.SetActive(true);
        Time.timeScale = 1f;
    }

    public void Renaudar() 
    {
        Pausa.SetActive(false); 
        Time.timeScale = 1f;
    }

    public void Ajustes() 
    {
        Time.timeScale = 1f;
        MainMenu.abrirAjustesAlCargar = true;
        SceneManager.LoadScene("MainMenu");
    }
    public void Menu() 
    {
        Time.timeScale = 1f;
        MainMenu.abrirAjustesAlCargar = false;
        SceneManager.LoadScene("MainMenu");
    }
}
