using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class MacetaUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Estado de Maceta")]
    public bool tienePlanta = false;
    public PlantaBase plantaActual;

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
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvasPrincipal.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        List<RaycastResult> resultados = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, resultados);

        bool colocadaEnNuevaCasilla = false;

        foreach (RaycastResult resultado in resultados)
        {
            CasillaUI casillaObjetivo = resultado.gameObject.GetComponent<CasillaUI>();

            if (casillaObjetivo != null && casillaObjetivo.transform.childCount == 0)
            {
                MoverAMacetaCasilla(casillaObjetivo.transform);
                colocadaEnNuevaCasilla = true;

                GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
                if (grilla != null) grilla.GuardarEstadoGrilla();

                break;
            }
        }

        if (!colocadaEnNuevaCasilla)
        {
            MoverAMacetaCasilla(padreOriginal);
        }
    }

    private void MoverAMacetaCasilla(Transform nuevoPadre)
    {
        transform.SetParent(nuevoPadre, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localPosition = Vector3.zero;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Auto-corrección: Si tienePlanta figura como true pero la referencia no existe o se eliminó
        if (tienePlanta && (plantaActual == null || transform.childCount == 0))
        {
            tienePlanta = false;
            plantaActual = null;
        }

        if (tienePlanta) return;

        GameObject objetoArrastrado = eventData.pointerDrag;
        if (objetoArrastrado == null) return;

        // CASO 1: Es un Sobre de Semillas que va a sembrar una planta nueva
        SobreSemillaUI sobreSemilla = objetoArrastrado.GetComponent<SobreSemillaUI>();
        if (sobreSemilla != null && sobreSemilla.prefabPlanta != null)
        {
            GameObject nuevaPlantaGO = Instantiate(sobreSemilla.prefabPlanta, this.transform);
            AjustarTransformPlanta(nuevaPlantaGO);

            plantaActual = nuevaPlantaGO.GetComponent<PlantaBase>();
            if (plantaActual != null)
            {
                plantaActual.enabled = true;
                plantaActual.ActualizarSpriteVisual();
            }

            tienePlanta = true;
            sobreSemilla.ConsumirSemilla();

            GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
            if (grilla != null) grilla.GuardarEstadoGrilla();

            return;
        }
    }

    private void AjustarTransformPlanta(GameObject objetoPlanta)
    {
        RectTransform rect = objetoPlanta.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.localPosition = Vector3.zero;
        }
    }

    public void LimpiarPlanta()
    {
        if (plantaActual != null)
        {
            Destroy(plantaActual.gameObject);
            plantaActual = null;
        }
        tienePlanta = false;
    }
}