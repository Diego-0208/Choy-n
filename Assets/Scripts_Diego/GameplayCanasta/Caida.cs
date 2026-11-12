using DG.Tweening;
using UnityEngine;

public class Caida : MonoBehaviour
{
    [Header("Configuración")]     
    private RectTransform rectFruta;
    private RectTransform rectCanasta;
    private bool fueAtrapada = false; 

    private void Start()
    {
        rectFruta = GetComponent<RectTransform>();

        
        GameObject objCanasta = GameObject.FindWithTag("Canasta");
        if (objCanasta != null)
        {
            rectCanasta = objCanasta.GetComponent<RectTransform>();
        }
    }

    private void Update()
    { 
        if(fueAtrapada) return;        
       

        if (rectCanasta != null) 
        {
            float distancia = Vector2.Distance(rectFruta.position, rectCanasta.position);

            if (distancia < 70) 
            {
                Atrapada();
                return; 
            }
        }
        if (rectFruta.anchoredPosition.y < -650f)
        {
            transform.DOKill();
            Destroy(gameObject);
        }

    }

    void Atrapada()
    {
        if(fueAtrapada)return;
        fueAtrapada=true;
        Debug.Log("La fruta fue atrapada"); 

        GeneradorFruta generador = FindAnyObjectByType<GeneradorFruta>();
        if (generador != null)
        {
            generador.SumarFruta();
        }
        MovimientoCanasta canasta = FindAnyObjectByType<MovimientoCanasta>();
        if (canasta != null)
        {
            canasta.ReproducirEfectoCaptura();
        }

        transform.DOKill();
        Destroy(gameObject);
        
    }
}