using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject Menu; 
    public GameObject Ajustes;
    public GameObject Almanaque;
    public GameObject Creditos;
    public static bool abrirAjustesAlCargar = false;

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

    public void PlayGame() 
    {
        SceneManager.LoadScene("Gameplay");  
    }

    public void OpenMenuPanel() 
    {
        Menu.SetActive(true);
        Ajustes.SetActive(false);
        Almanaque.SetActive(false);
        Creditos.SetActive(false);
    } 
    public void OpenAjustesPanel() 
    {
        Menu.SetActive(false);
        Ajustes.SetActive(true);
        Almanaque.SetActive(false);
        Creditos.SetActive(false);
    }
    public void OpenAlmanequePanel() 
    {
        Menu.SetActive(false);
        Ajustes.SetActive(false);
        Almanaque.SetActive(true);
        Creditos.SetActive(false); 
    }
    public void OpenCreditosPanel() 
    {
        Menu.SetActive(false);
        Ajustes.SetActive(false);
        Almanaque.SetActive(false);
        Creditos.SetActive(true);
    }
}
