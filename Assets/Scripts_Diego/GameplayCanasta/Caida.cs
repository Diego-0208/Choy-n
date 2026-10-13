using UnityEngine;

public class Caida : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidadCaida = 350f;
    private RectTransform rectFruta;
    private RectTransform rectCanasta;

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
        
        rectFruta.anchoredPosition -= new Vector2(0, velocidadCaida * Time.deltaTime);

     
        if (rectCanasta != null)
        {
           
            float distancia = Vector2.Distance(rectFruta.anchoredPosition, rectCanasta.anchoredPosition);

            
            if (distancia < 70f)
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