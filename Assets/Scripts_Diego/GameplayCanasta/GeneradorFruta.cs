using UnityEngine;
using System.Collections;
using TMPro; 
public class GeneradorFruta : MonoBehaviour
{
    [Header("Referencias de la UI")]
    public GameObject prefabFruta;
    public RectTransform panelJuego;
    public TextMeshProUGUI texTiempo;
    public TextMeshProUGUI texContadorFrutas;

    [Header("Configuración de las frutas")]
    public float tiempoEntreFrutas = 1.0f;
    public float limiteIzquierdo = -430f;
    public float limiteDerecho = 430f;
    public float alturaSpawn = 700f;

    [Header("Configuración del juego")]
    public float tiempo = 30f;

    private float tiempoRestante; 
    private bool juegoActivo = false;

    private int frutasRecolectadas = 0; 

    void Start()
    {
        IniciarMinijuego();     
    }

    public void IniciarMinijuego() 
    {
        tiempoRestante = tiempo; 
        juegoActivo = true;

        frutasRecolectadas = 0;
        ActualizarTextoContador();

        StartCoroutine(RutinaSpawnFrutas());
        StartCoroutine(RutinaTemporizador());
    }

    IEnumerator RutinaSpawnFrutas() 
    {
        while (juegoActivo) 
        {
            CrearFruta(); 
            yield return new WaitForSeconds(tiempoEntreFrutas);
        } 
    }

    IEnumerator RutinaTemporizador() 
    {
        while (tiempoRestante > 0 && juegoActivo) 
        {
            if (texTiempo != null) 
            {
              texTiempo.text = "Tiempo: " + Mathf.CeilToInt(tiempoRestante).ToString() + "s";
            }
            yield return new WaitForSeconds(1f);
            tiempoRestante -= 1f;
        }

        FinalizarMinijuego();
    }

    void CrearFruta() 
    {
        if(prefabFruta == null || panelJuego == null) return;

        GameObject nuevaFruta = Instantiate(prefabFruta, panelJuego); 
        RectTransform recFruta = nuevaFruta.GetComponent<RectTransform>();

        float posXAleatorio = Random.Range(limiteIzquierdo, limiteDerecho);
        recFruta.anchoredPosition = new Vector2(posXAleatorio, alturaSpawn); 
    }

    public void SumarFruta() 
    {
        if(!juegoActivo)return;
        frutasRecolectadas++;
        ActualizarTextoContador();
    }

    void ActualizarTextoContador() 
    {
        if (texContadorFrutas != null) 
        {
            texContadorFrutas.text = "Frutas" + frutasRecolectadas;
        }
    }

    void FinalizarMinijuego() 
    {
        juegoActivo = false;
        if (texTiempo != null) 
        {
            texTiempo.text = "¡Tiempo agotado!";
        }

        Debug.Log("Minijuego finalizado. Evaluando recompensa...");
    }
}
