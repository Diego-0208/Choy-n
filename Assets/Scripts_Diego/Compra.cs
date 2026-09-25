using UnityEngine;
using TMPro; 

public class Compra : MonoBehaviour  
{
    public int Moneda;
    public int Flor1; 
    public int Flor2;
    public int Flor3;
    public int Flor4;
    public TextMeshProUGUI monedas_text_Gameplay; 
    public TextMeshProUGUI monedas_text;

    private void Start()
    {
        
        Moneda = PlayerPrefs.GetInt("Moneda", 2000);    
        Flor1 = PlayerPrefs.GetInt("Flor1", 0);
        ActualizarTextos();
    }

    public void ComprarFlor1() 
    {
        if (Moneda >= 200)
        {
            Moneda -= 200; 

            Flor1 += 1;

            PlayerPrefs.SetInt("Moneda", Moneda);
            PlayerPrefs.SetInt("Flor1", Flor1);
            PlayerPrefs.Save();

            ActualizarTextos();
        }
        else 
        {
            print("No tienes la cantidad de monedas para comprar la flor de 200");
        }
    }

    public void ComprarFlor2()
    {
        if (Moneda >= 400)
        {
            Moneda -= 400;

            Flor2 += 1;

            PlayerPrefs.SetInt("Moneda", Moneda);
            PlayerPrefs.SetInt("Flor2", Flor2);
            PlayerPrefs.Save();

            ActualizarTextos();
        }
        else
        {
            print("No tienes la cantidad de monedas para comprar la flor de 400");
        }
    }

    public void ComprarFlor3()
    {
        if (Moneda >= 150)
        {
            Moneda -= 150;

            Flor3 += 1;

            PlayerPrefs.SetInt("Moneda", Moneda);
            PlayerPrefs.SetInt("Flor3", Flor3);
            PlayerPrefs.Save();

            ActualizarTextos();
        }
        else
        {
            print("No tienes la cantidad de monedas para comprar la flor de 150");
        }
    }

    public void ComprarFlor4()
    {
        if (Moneda >= 700)
        {
            Moneda -= 700;

            Flor4 += 1;

            PlayerPrefs.SetInt("Moneda", Moneda);
            PlayerPrefs.SetInt("Flor4", Flor4);
            PlayerPrefs.Save();

            ActualizarTextos();
        }
        else
        {
            print("No tienes la cantidad de monedas para comprar la flor de 700");
        }
    }
    public void ActualizarTextos()
    {
        if (monedas_text_Gameplay != null)
        {
            monedas_text_Gameplay.text = "Monedas: " + Moneda.ToString();
        }

        if (monedas_text != null)
        {
            monedas_text.text = "Monedas: " + Moneda.ToString();
        }
    }
}
