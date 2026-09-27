using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class FlorItem_01 : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Identificador")]
    public int tipoFlor = 1; 

    private Transform padreOriginal;
    private Canvas canvasPrincipal;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Compra_01 inventario;
    private PlantaBase scriptPlanta;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasPrincipal = GetComponentInParent<Canvas>();
        inventario = FindFirstObjectByType<Compra_01>();
        scriptPlanta = GetComponent<PlantaBase>();

        if (scriptPlanta != null)
        {
            scriptPlanta.enabled = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        padreOriginal = transform.parent;

        CasillaUI casillaAntigua = padreOriginal.GetComponent<CasillaUI>();
        if (casillaAntigua != null)
        {
            casillaAntigua.VaciarCasilla();
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

        foreach (RaycastResult resultado in resultados)
        {
            if (resultado.gameObject.GetComponent<ZonaVentaUI>() != null)
            {
                return;
            }
        }

        if (transform.parent == canvasPrincipal.transform)
        {
            DevolverAlInventario();
        }
    }

    public void ActivarPlantaEnGrilla()
    {
        if (scriptPlanta != null)
        {
            scriptPlanta.enabled = true; 
        }
    }

    public void DevolverAlInventario()
    {
        if (scriptPlanta != null)
        {
            scriptPlanta.enabled = false; 
        }

        Transform spawnPoint = inventario != null ? inventario.GetSpawnPorTipo(tipoFlor) : padreOriginal;

        if (spawnPoint != null)
        {
            transform.SetParent(spawnPoint, false);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localPosition = Vector3.zero;

            if (inventario != null && padreOriginal.GetComponent<CasillaUI>() != null)
            {
                inventario.DevolverFlor(tipoFlor);
            }
        }
    }
}