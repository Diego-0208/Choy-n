using UnityEngine; 
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
   public GameObject Tienda; 
   public GameObject Gameplay; 
   
   

   public void OpenShop()
   {
	   Tienda.SetActive(true);
	   Time.timeScale = 1f;
   } 

    public void RegresarAlCanvasGameplay()
    {
       Gameplay.SetActive(true);
       Tienda.SetActive(false); 
    }
}
