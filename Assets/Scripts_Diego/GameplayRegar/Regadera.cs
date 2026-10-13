using UnityEngine;
using UnityEngine.EventSystems; 
using UnityEngine.UI; 

public class Regadera : MonoBehaviour, IPointerDownHandler, IPointerUpHandler  
{
    [Header("Referencias UI")]
    public GameObject agua; 
    public Slider barraProgresoAgua; 

    [Header("Configuración del riego ")]
    public float velocidadRiego = 25f; 
    private float estaRegado = false; 
    private bool miniJuegoCompletado = false; 

    [Header ("Limites del riego(0 a 100)")]
    public float nivelMinimo = 60f; 
    public float nivelMaximo = 100f; 

    private void Start()
    {
        if(chorroAgua != null)
        {
            chorroAgua.SetActive(false); 
        }
        if(barraProgresoAgua != null)
        {
            barraProgresoAgua.minValue = 0f;  
            barraProgresoAgua.maxValue = 100f; 
            barraProgresoAgua.value = 0f; 
        }
    }

    private void Update()
    {
        if(miniJuegoCompletado)return; 

        if (estaRegado) 
        {
            if(barraProgresoAgua != null)
            {
                barraProgresoAgua.value += velocidadRiego * Time.deltaTime; 

                if(barraProgresoAgua.value >= nivelMaximo)
                {
                    PlantaMuerePorExceso(); 
                }
            }
        } 
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(miniJuegoCompletado)return;

        estaRegado = true; 
        if(chorroAgua != null)
        {
            chorroAgua.SetActive(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(miniJuegoCompletado)return;

        estaRegado = false; 
        if(chorroAgua != null)
        {
            chorroAgua.SetActive(false);
        }

        EvaluarResultadoRiesgo();
    } 

    void EvaluarResultadoRiesgo()
    {
        miniJuegoCompletado = true; 

        if(barraProgresoAgua.value >= nivelMinimo)
        {
          Debug.Log("Regaste bien la planta no te excediste con la cantidad de agua"); 
        }
        else
        {
            Debug.Log("Le echaste poca agua a la planta")
        } 
    }

    void PlantaMuerePorExceso () 
    {
        miniJuegoCompletado = true; 
        estaRegado = false ; 

        if(chorroAgua != null)
        {
            chorroAgua.SetActive(false); 
        }

        Debug.Log("Le hechaste mucha agua a la planta, la mataste")
    }
}
