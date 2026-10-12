using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(PlantaBase))]
public class FlorItem_01 : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Identificador")]
    public int tipoFlor = 1;

    private Transform padreOriginal;
    private MacetaUI macetaOrigen;
    private Canvas canvasPrincipal;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private PlantaBase scriptPlanta;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasPrincipal = GetComponentInParent<Canvas>();
        scriptPlanta = GetComponent<PlantaBase>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        padreOriginal = transform.parent;
        macetaOrigen = padreOriginal.GetComponent<MacetaUI>();

        // Al comenzar a arrastrar, liberamos la maceta anterior
        if (macetaOrigen != null)
        {
            macetaOrigen.tienePlanta = false;
            macetaOrigen.plantaActual = null;
        }

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

        List<RaycastResult> resultados = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, resultados);

        bool reubicada = false;

        foreach (RaycastResult resultado in resultados)
        {
            // Detecta si se soltó sobre una Zona de Venta o basurero
            if (resultado.gameObject.GetComponent<ZonaVentaUI>() != null)
            {
                Destroy(gameObject);
                return;
            }

            // Detecta si se soltó en otra maceta vacía
            MacetaUI nuevaMaceta = resultado.gameObject.GetComponent<MacetaUI>();
            if (nuevaMaceta != null && !nuevaMaceta.tienePlanta)
            {
                AcomodarEnMaceta(nuevaMaceta);
                reubicada = true;
                break;
            }
        }

        // Si se soltó fuera o en un área no válida, regresa a la maceta de donde salió
        if (!reubicada && macetaOrigen != null)
        {
            AcomodarEnMaceta(macetaOrigen);
        }
    }

    public void ActivarPlantaEnGrilla(MacetaUI nuevaMaceta = null)
    {
        if (scriptPlanta != null)
        {
            scriptPlanta.enabled = true;
            scriptPlanta.ActualizarSpriteVisual();
        }

        if (nuevaMaceta != null)
        {
            AcomodarEnMaceta(nuevaMaceta);
        }
    }

    public void AcomodarEnMaceta(MacetaUI maceta)
    {
        transform.SetParent(maceta.transform, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0f); // Alineado al fondo de la maceta para el crecimiento
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localPosition = Vector3.zero;

        maceta.tienePlanta = true;
        maceta.plantaActual = scriptPlanta;

        GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
        if (grilla != null) grilla.GuardarEstadoGrilla();
    }
}