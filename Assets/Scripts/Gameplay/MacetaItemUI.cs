using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class MacetaItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Configuración de Prefab")]
    public GameObject prefabMaceta; // <- Este campo aparecerá en el Inspector

    private Vector3 posicionInicialLocal;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform padreOriginal;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Compra_01 inventario = FindFirstObjectByType<Compra_01>();
        if (inventario != null && inventario.Macetas <= 0)
        {
            Debug.LogWarning("¡No tienes macetas disponibles!");
            eventData.pointerDrag = null;
            return;
        }

        posicionInicialLocal = rectTransform.localPosition;
        padreOriginal = transform.parent;

        // Desbloquear raycasts mientras se arrastra
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

        foreach (RaycastResult resultado in resultados)
        {
            CasillaUI casilla = resultado.gameObject.GetComponent<CasillaUI>();

            if (casilla != null && !casilla.estaOcupada)
            {
                if (prefabMaceta == null)
                {
                    Debug.LogError("¡No has asignado el Prefab de la Maceta en el Inspector!");
                    break;
                }

                // Instanciar el prefab dentro de la casilla
                GameObject nuevaMaceta = Instantiate(prefabMaceta, casilla.transform);

                RectTransform rectMaceta = nuevaMaceta.GetComponent<RectTransform>();
                rectMaceta.anchorMin = new Vector2(0.5f, 0.5f);
                rectMaceta.anchorMax = new Vector2(0.5f, 0.5f);
                rectMaceta.pivot = new Vector2(0.5f, 0.5f);
                rectMaceta.anchoredPosition = Vector2.zero;
                rectMaceta.localPosition = Vector3.zero;
                rectMaceta.localScale = Vector3.one;

                casilla.estaOcupada = true;

                Compra_01 inventario = FindFirstObjectByType<Compra_01>();
                if (inventario != null)
                {
                    inventario.ConsumirMaceta(); // Guarda en PlayerPrefs y resta la maceta
                }

                GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
                if (grilla != null) grilla.GuardarEstadoGrilla();

                Debug.Log($"Maceta colocada correctamente en casilla [{casilla.posX}, {casilla.posY}].");
                break;
            }
        }

        transform.SetParent(padreOriginal, false);
        rectTransform.localPosition = posicionInicialLocal;
    }
}