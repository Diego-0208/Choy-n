using UnityEngine;

public class Caida : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidadCaida = 350f;
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
        
        rectFruta.anchoredPosition -= new Vector2(0, velocidadCaida * Time.deltaTime);

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
            Destroy(gameObject);
        }
    }

    void Atrapada()
    {
        Debug.Log("¡Fruta atrapada con éxito!");

       
        Destroy(gameObject);
    }
}