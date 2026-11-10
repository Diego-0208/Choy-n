using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GeneradorFruta : MonoBehaviour
{
    [Header("Raíz del minijuego")]
    [Tooltip("Objeto que contiene todo el minijuego (fondo, canasta, textos...). Si se deja vacío se usa este mismo objeto.")]
    [SerializeField] private GameObject raizMinijuego;

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

    [Header ("Configuración de la naimacion de la fruta")]
    public float DuracionDeLaCaida = 2.5f; 
    public float alturaDestino = - 700f; 
    public float duracionGiro360 = 1.0f; 

    [Header("Configuración del juego")]
    public float tiempo = 30f;

    public event Action Terminado;

    public GameObject Raiz => raizMinijuego != null ? raizMinijuego : gameObject;

    private readonly List<GameObject> frutasActivas = new List<GameObject>();
    private Action<int> alTerminar;
    private Coroutine rutinaSpawn;

    private float tiempoRestante;
    private bool juegoActivo = false;
    private int frutasRecolectadas = 0;

    private void OnDisable()
    {
        juegoActivo = false;
        LimpiarFrutas();
    }

    // Compatibilidad con el prototipo original (botones de escena que llamaban a IniciarMinijuego).
    public void IniciarMinijuego()
    {
        Iniciar(null);
    }

    public void Iniciar(Action<int> alTerminar)
    {
        StopAllCoroutines();
        LimpiarFrutas();

        this.alTerminar = alTerminar;

        tiempoRestante = tiempo;
        frutasRecolectadas = 0;
        juegoActivo = true;

        ActualizarTextoContador();

        rutinaSpawn = StartCoroutine(RutinaSpawnFrutas());
        StartCoroutine(RutinaTemporizador());
    }

    private IEnumerator RutinaSpawnFrutas()
    {
        while (juegoActivo)
        {
            CrearFruta();
            yield return new WaitForSeconds(tiempoEntreFrutas);
        }
    }

    private IEnumerator RutinaTemporizador()
    {
        while (tiempoRestante > 0f && juegoActivo)
        {
            if (texTiempo != null)
            {
                texTiempo.text = "Tiempo: " + Mathf.CeilToInt(tiempoRestante).ToString() + "s";
            }

            yield return new WaitForSeconds(1f);
            tiempoRestante -= 1f;
        }

        Finalizar();
    }

    private void CrearFruta()
    {
        if (prefabFruta == null || panelJuego == null) return;

        GameObject nuevaFruta = Instantiate(prefabFruta, panelJuego);
        RectTransform recFruta = nuevaFruta.GetComponent<RectTransform>();

        // Las frutas no deben bloquear el arrastre de la canasta.
        Graphic grafico = nuevaFruta.GetComponent<Graphic>();
        if (grafico != null) grafico.raycastTarget = false;

        float posXAleatorio = UnityEngine.Random.Range(limiteIzquierdo, limiteDerecho);
        recFruta.anchoredPosition = new Vector2(posXAleatorio, alturaSpawn); 

        recFruta.DOAnchorPosY(alturaDestino,DuracionDeLaCaida)
        .SetEase(Ease.Linear)
        .OnComplete(() => {
            if(nuevaFruta != null)
            {
                frutasActivas.Remove(nuevaFruta); 
                Destroy(nuevaFruta); 
            }         
        }); 

        recFruta.DORotate(new Vector3( 0,0,360),duracionGiro360, RotateMode.FastBeyond360) 
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Incremental); 


        frutasActivas.RemoveAll(f => f == null);
        frutasActivas.Add(nuevaFruta);
    }

    // La llama la fruta cuando cae dentro de la canasta.
    public void SumarFruta()
    {
        if (!juegoActivo) return;

        frutasRecolectadas++;
        ActualizarTextoContador();
    }

    private void ActualizarTextoContador()
    {
        if (texContadorFrutas != null)
        {
            texContadorFrutas.text = "Frutas: " + frutasRecolectadas;
        }
    }

    private void Finalizar()
    {
        if (!juegoActivo) return;

        juegoActivo = false;

        if (rutinaSpawn != null)
        {
            StopCoroutine(rutinaSpawn);
            rutinaSpawn = null;
        }

        LimpiarFrutas();

        if (texTiempo != null)
        {
            texTiempo.text = "¡Tiempo agotado!";
        }

        Debug.Log("Minijuego de cosecha finalizado. Frutas recolectadas: " + frutasRecolectadas);

        Action<int> cb = alTerminar;
        alTerminar = null;

        cb?.Invoke(frutasRecolectadas);
        Terminado?.Invoke();
    }

    private void LimpiarFrutas()
    {
        foreach (GameObject fruta in frutasActivas)
        {
            if (fruta != null) Destroy(fruta);
        }

        frutasActivas.Clear();
    }
}
