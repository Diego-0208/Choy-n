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

    [Header("Configuración Inclinación")]
    public RectTransform transformRegadera;
    public float anguloInclinación = -30f;
    public float velocidadInclinación = 10f;
    private Quaternion rotacionOriginal;

    [Header("Configuracion de Gotas")]
    public float frecuenciaGotas = 0.10f;
    public float velocidadGotas = 200f;
    private float temporizadorGotas = 0f;

    [Header ("Configuración de color y el Riego")]
    public Color colorSeco = Color.white;
    public Color colorHumedo = new Color(0.2f, 0.6f, 1f); 
    public Color colorExceso = Color.brown;

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

        if (imagenBasePlanta != null)
            imagenBasePlanta.color = colorSeco;
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

        actualizarColorPlanta();
        if (progresoAgua >= nivelMaximo) 
        {
            PlantaMuerePorExceso();
        }
    }

    void actualizarColorPlanta() 
    {
        if (imagenBasePlanta == null) return;

        float t = progresoAgua / nivelMaximo;
        if (progresoAgua < nivelMaximo)
        {
            imagenBasePlanta.color = Color.Lerp(colorSeco, colorHumedo, t);
        }
        else 
        {
            imagenBasePlanta.color = colorHumedo;
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

        if (imagenBasePlanta != null)
            imagenBasePlanta.color = colorExceso;

        Debug.Log("Te excediste con el agua de la planta");
    }
}
