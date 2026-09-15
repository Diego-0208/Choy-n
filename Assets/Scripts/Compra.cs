using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
using System.Collections.Generic;  

public class Compra : MonoBehaviour  
{
    public int Moneda; 
    public Text monedas_text;  

    void Start() 
    {   
        Moneda =  PlayerPrefs.GetInt("Moneda"); 
        Moneda = 2000; 
    }

    public void Flor200()
    {
        if (Moneda >= 200)
        {
            Moneda -= 200 ;
            monedas_text.text = Moneda.ToString();  
            PlayerPrefs.SetInt("Moneda" , Moneda);
        }
        else
        {
            print ("No tienes la cantidad de monedas para comprar la flor de 200");
        }
    } 
}
