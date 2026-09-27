using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class SacaAguaUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Configuración de Secado")]
    [SerializeField] private float cantidadAguaAExtraer = 30f; 

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

        bool aguaExtraida = false;

        foreach (RaycastResult resultado in resultados)
        {
            GestorSaludPlanta gestorSalud = resultado.gameObject.GetComponentInParent<GestorSaludPlanta>();

            if (gestorSalud != null)
            {
                gestorSalud.ExtraerAgua(cantidadAguaAExtraer);
                aguaExtraida = true;
                Debug.Log($"¡Se extrajo agua! Nivel de agua actual de {gestorSalud.gameObject.name}: {gestorSalud.NivelAgua}");
                break;
            }
        }

        if (!aguaExtraida)
        {
            Debug.Log("No se encontró ninguna planta debajo del saca-agua.");
        }

        rectTransform.localPosition = posicionInicialLocal;
    }
}