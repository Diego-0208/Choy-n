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
        Flor2 = PlayerPrefs.GetInt("Flor2", 0);
        Flor3 = PlayerPrefs.GetInt("Flor3", 0);
        Flor4 = PlayerPrefs.GetInt("Flor4", 0);
        ActualizarTextos();
    }

    public void DesbloquearPlantaEnAlmanaque(string idPlanta)
    {
        PlayerPrefs.SetInt("Desbloqueada_" + idPlanta, 1);
        PlayerPrefs.Save();
    }

    public void ComprarFlor1()
    {
        if (Moneda >= 200)
        {
            Moneda -= 200;
            Flor1 += 1;

            PlayerPrefs.SetInt("Moneda", Moneda);
            PlayerPrefs.SetInt("Flor1", Flor1);

            // Guardar la clave exacta
            PlayerPrefs.SetInt("Desbloqueada_1", 1);
            PlayerPrefs.Save();

            Debug.Log("COMPRA REALIZADA: Se guardó 'Desbloqueada_1' con valor 1");

            ActualizarTextos();
        }
        else
        {
            Debug.Log("No tienes suficientes monedas para comprar Flor 1");
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
            PlayerPrefs.SetInt("Desbloqueada_2", 2);
            PlayerPrefs.Save();

            Debug.Log("COMPRA REALIZADA: Se guardó 'Desbloqueada_2' con valor 2");

            ActualizarTextos();
        }
        else
        {
            Debug.Log("No tienes suficientes monedas para comprar Flor 2");
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
            PlayerPrefs.SetInt("Desbloqueada_3", 3);
            PlayerPrefs.Save();

            Debug.Log("COMPRA REALIZADA: Se guardó 'Desbloqueada_3' con valor 3");

            ActualizarTextos();
        }
        else
        {
            Debug.Log("No tienes suficientes monedas para comprar Flor 3");
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
            PlayerPrefs.SetInt("Desbloqueada_4", 4);
            PlayerPrefs.Save();

            Debug.Log("COMPRA REALIZADA: Se guardó 'Desbloqueada_4' con valor 4");

            ActualizarTextos();
        }
        else
        {
            Debug.Log("No tienes suficientes monedas para comprar Flor 4");
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