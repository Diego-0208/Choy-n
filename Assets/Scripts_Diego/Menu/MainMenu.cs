using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Paneles del Menú Principal")]
    public GameObject Menu; 
    public GameObject Ajustes;
    public GameObject Almanaque;
    public GameObject Creditos;

    private void Start()
    {
        OpenMenuPanel();
    }

    public void VolverDesdeAjustes()
    {
        // Al estar en MainMenu, simplemente regresa al panel principal
        OpenMenuPanel(); 
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