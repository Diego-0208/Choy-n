using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class ObjetoArrastrable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform padreOriginal;
    private Canvas canvasPrincipal;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasPrincipal = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        padreOriginal = transform.parent;

        transform.SetParent(canvasPrincipal.transform);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == canvasPrincipal.transform)
        {
            transform.SetParent(padreOriginal);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}