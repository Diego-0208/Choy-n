using UnityEngine;
using UnityEngine.EventSystems;

public class PlagaItem : MonoBehaviour, IPointerClickHandler
{
    private GestorPlagas spawnerAsociado;
    private RectTransform rectTransform;
    
    private Vector2 posicionInicial;
    private float tiempoAleatorioX;
    private float tiempoAleatorioY;
    
    [Header("Configuración de Vuelo")]
    [SerializeField] private float velocidadMovimiento = 3f;
    [SerializeField] private float amplitudMovimiento = 15f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        posicionInicial = rectTransform.anchoredPosition;
        
        tiempoAleatorioX = Random.Range(0f, 100f);
        tiempoAleatorioY = Random.Range(0f, 100f);
    }

    private void Update()
    {
        float offsetX = Mathf.Sin(Time.time * velocidadMovimiento + tiempoAleatorioX) * amplitudMovimiento;
        float offsetY = Mathf.Cos(Time.time * velocidadMovimiento * 1.2f + tiempoAleatorioY) * amplitudMovimiento;

        rectTransform.anchoredPosition = posicionInicial + new Vector2(offsetX, offsetY);
    }

    public void Inicializar(GestorPlagas spawner)
    {
        spawnerAsociado = spawner;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        DestruirPlaga();
    }

    public void DestruirPlaga()
    {
        if (spawnerAsociado != null)
        {
            spawnerAsociado.RegistrarPlagaEliminada();
        }

        Destroy(gameObject);
    }
}