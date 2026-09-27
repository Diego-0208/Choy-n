using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class RegaderaUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Configuración de Riego")]
    [SerializeField] private float cantidadAgua = 30f;

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

        bool plantaRegada = false;

        foreach (RaycastResult resultado in resultados)
        {
            GestorSaludPlanta gestorSalud = resultado.gameObject.GetComponentInParent<GestorSaludPlanta>();

            if (gestorSalud != null)
            {
                gestorSalud.Regar(cantidadAgua);
                plantaRegada = true;
                Debug.Log($"¡Regado exitoso! Agua actual de {gestorSalud.gameObject.name}: {gestorSalud.NivelAgua}");
                break;
            }
        }

        if (!plantaRegada)
        {
            Debug.Log("No se encontró ninguna planta debajo de la regadera.");
        }

        rectTransform.localPosition = posicionInicialLocal;
    }
}