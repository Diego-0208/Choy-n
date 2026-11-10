using UnityEngine;

public class Shop_01 : MonoBehaviour
{
    public static event System.Action AlAbrirTienda;

    public GameObject Tienda;
    public GameObject Gameplay;

    public void OpenShop()
    {
        AlAbrirTienda?.Invoke();
        if (Tienda != null) Tienda.SetActive(true);
        if (Gameplay != null) Gameplay.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RegresarAlCanvasGameplay()
    {
        if (Gameplay != null) Gameplay.SetActive(true);
        if (Tienda != null) Tienda.SetActive(false);
    }
}