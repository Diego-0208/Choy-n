using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems; 
using UnityEngine.UI; 

public class Regadera : MonoBehaviour, IPointerDownHandler, IPointerUpHandler  
{
    [Header("Referencias UI")]
    public GameObject prefabGota;
    public RectTransform contenedorPanel;
    public RectTransform puntoSalidaGota;
    public RectTransform zonaColisionMaceta;
    public Image imagenBasePlanta; 
    public Image imagenLineaMeta;

    [Header("Configuración Inclinación")]
    public RectTransform transformRegadera;
    public float anguloInclinación = -30f;
    public float velocidadInclinación = 10f;
    private Quaternion rotacionOriginal;

    [Header("Configuracion de Gotas")]
    public float frecuenciaGotas = 0.10f;
    public float velocidadGotas = 200f;
    private float temporizadorGotas = 0f;

    [Header("Sprites de las plantas(estado en la que se encuentre)")]
    public Sprite spriteBase; 
    public Sprite spriteHumedo;
    public Sprite spriteMuerta;

    [Header ("Colores de la linead de meta ")]
    public Color colorlineaPendiente = Color.white;
    public Color colorlinraAlcanzada = Color.green; 

    public float progresoAgua = 0f;
    public float nivelMinimo = 60f;
    public float nivelMaximo = 100f;
    public float incrementoPorGota = 4f;

    private bool estaRegado = false;
    private bool miniJuegoCompletado = false;

    private void Start()
    {
        if (transformRegadera == null)
            transformRegadera = GetComponent<RectTransform>();

        rotacionOriginal = transformRegadera.localRotation;

        if (imagenBasePlanta != null && spriteBase != null)
            imagenBasePlanta.sprite = spriteBase;

        if (imagenLineaMeta != null)
            imagenLineaMeta.color = colorlineaPendiente;
    }

    private void Update()
    {
        Quaternion rotacionObjetivo = estaRegado
            ? Quaternion.Euler(0, 0, anguloInclinación)
            : rotacionOriginal;

        transformRegadera.localRotation = Quaternion.Lerp(transformRegadera.localRotation, rotacionObjetivo, Time.deltaTime * velocidadInclinación);

        if (miniJuegoCompletado) return;

        if (estaRegado) 
        {
            temporizadorGotas += Time.deltaTime;
            if (temporizadorGotas >= frecuenciaGotas) 
            {
                temporizadorGotas = 0;
                CrearGota();
            }
        }
    }

    void CrearGota() 
    {
        if (prefabGota == null || contenedorPanel == null || puntoSalidaGota == null) return;

        GameObject nuevaGota = Instantiate(prefabGota, contenedorPanel);
        RectTransform rectGota = nuevaGota.GetComponent<RectTransform>();

        rectGota.anchoredPosition = puntoSalidaGota.anchoredPosition;

        GotaAgua scriptGota = nuevaGota.AddComponent<GotaAgua>();
        scriptGota.Inicializar(this, velocidadGotas, zonaColisionMaceta);
    }

    public void RegistrarImpactoGota() 
    {
        if (miniJuegoCompletado) return;

        progresoAgua += incrementoPorGota;
        progresoAgua = math.clamp(progresoAgua, 0f, nivelMaximo);

        actualizarEstadoJuego();
        if (progresoAgua >= nivelMaximo) 
        {
            PlantaMuerePorExceso();
        }
    }

    void actualizarEstadoJuego() 
    {
        if (imagenBasePlanta != null) 
        {
            if (progresoAgua >= nivelMinimo && spriteHumedo != null)
            {
                imagenBasePlanta.sprite = spriteHumedo;
            }
            else if (progresoAgua < nivelMinimo && spriteBase != null) 
            {
                imagenBasePlanta.sprite = spriteBase;
            }
        }

        if (imagenLineaMeta != null) 
        {
            if (progresoAgua >= nivelMinimo)
            {
                imagenLineaMeta.color = colorlinraAlcanzada;
            }
            else 
            {
                imagenLineaMeta.color = colorlineaPendiente;
            }
        }
       
    }

    public void OnPointerDown(PointerEventData eventData) 
    {
        if (miniJuegoCompletado) return;
        estaRegado = true;

        CrearGota();
        temporizadorGotas = 0f; 
    }

    public void OnPointerUp(PointerEventData eventData) 
    {
        if (miniJuegoCompletado) return;
        estaRegado = false;
        EvaluarResultadoRiesgo();
    }

    void EvaluarResultadoRiesgo() 
    {
        miniJuegoCompletado = true;

        if (progresoAgua >= nivelMinimo)
        {
            Debug.Log("Regaste bien la planta");
        }
        else 
        {
            Debug.Log("Le echaste poca agua a la planta"); 
        }
    }

    void PlantaMuerePorExceso() 
    {
        miniJuegoCompletado = true;
        estaRegado = false ;

       if(imagenBasePlanta != null && spriteMuerta != null)
          imagenBasePlanta.sprite = spriteMuerta;

        Debug.Log("Te excediste con el agua de la planta");
    }
}
