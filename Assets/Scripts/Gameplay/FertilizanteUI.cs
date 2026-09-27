using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class FertilizanteUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Configuración Fertilizante")]
    [SerializeField] private float segundosAcelerados = 10f; 

    private Vector3 posicionInicialLocal;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        posicionInicialLocal = rectTransform.localPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = eventData.position
        };

        List<RaycastResult> resultados = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, resultados);

        bool fertilizado = false;

        foreach (RaycastResult resultado in resultados)
        {
            PlantaBase planta = resultado.gameObject.GetComponentInParent<PlantaBase>();

            if (planta != null)
            {
                if (!planta.EsAdulta)
                {
                    planta.AcelerarCrecimiento(segundosAcelerados);
                    fertilizado = true;
                }
                else
                {
                    Debug.Log($"{planta.Nombre} ya está completamente adulta.");
                }
                break;
            }
        }

        if (!fertilizado)
        {
            Debug.Log("No se fertilizó ninguna planta.");
        }

        rectTransform.localPosition = posicionInicialLocal;
    }
}