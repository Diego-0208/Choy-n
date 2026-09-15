using UnityEngine;
using UnityEngine.UI;

public class GrillaUI : MonoBehaviour
{
    [Header("Dimensiones de Grilla")]
    [SerializeField] private int columnas = 10;
    [SerializeField] private int filas = 20;

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

        for (int y = 0; y < filas; y++)
        {
            for (int x = 0; x < columnas; x++)
            {
                GameObject nuevaCasilla = Instantiate(casillaPrefab, transform);
                nuevaCasilla.name = $"Casilla_{x}_{y}";

                CasillaUI casillaScript = nuevaCasilla.GetComponent<CasillaUI>();
                if (casillaScript != null)
                {
                    casillaScript.Inicializar(x, y);
                }
            }
        }
    }

    public void SetVisibilidadGrilla(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}