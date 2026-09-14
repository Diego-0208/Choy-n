using UnityEngine;
using UnityEngine.UI;

public class GrillaUI : MonoBehaviour
{
    [Header("Dimensiones de Grilla")]
    [SerializeField] private int columnas = 10;
    [SerializeField] private int filas = 30;

    [Header("Referencias UI")]
    [SerializeField] private GameObject casillaPrefab;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        GenerarGrilla();
    }

    private void Start()
    {
        
        SetVisibilidadGrilla(true);
    }

    private void GenerarGrilla()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        int totalCasillas = columnas * filas; 

        for (int i = 0; i < totalCasillas; i++)
        {
            GameObject nuevaCasilla = Instantiate(casillaPrefab, transform);
            nuevaCasilla.name = $"Casilla_{i}";
        }
    }

    /// <summary>
   
    /// </summary>
    public void SetVisibilidadGrilla(bool visible)
    {
        if (canvasGroup != null)
        {
            
            canvasGroup.alpha = visible ? 1f : 0f;
            
            
            canvasGroup.blocksRaycasts = visible;
        }
    }
}