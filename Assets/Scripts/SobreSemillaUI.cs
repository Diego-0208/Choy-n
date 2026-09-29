using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class SobreSemillaUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Identificador")]
    public int tipoFlor = 1;

    [Header("Prefab de la Planta a Instanciar")]
    public GameObject prefabPlanta; 

    private Transform padreOriginal;
    private Canvas canvasPrincipal;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Compra_01 inventario;
    
    // Guardamos la escala local original
    private Vector3 escalaOriginal;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasPrincipal = GetComponentInParent<Canvas>();
        inventario = FindFirstObjectByType<Compra_01>();
        
        // Guardar la escala local por defecto
        escalaOriginal = transform.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Cancelar si no hay stock
        if (GetCantidadDisponible() <= 0)
        {
            eventData.pointerDrag = null;
            return;
        }

        padreOriginal = transform.parent;
        
        // Asignamos el nuevo padre sin modificar la escala local
        transform.SetParent(canvasPrincipal.transform, false);
        transform.localScale = escalaOriginal; // Mantiene el tamaño visual correcto
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

        bool sembradoExitoso = false;

        foreach (RaycastResult resultado in resultados)
        {
            MacetaUI maceta = resultado.gameObject.GetComponent<MacetaUI>();

            if (maceta != null && !maceta.tienePlanta)
            {
                GameObject nuevaPlantaObj = Instantiate(prefabPlanta, maceta.transform);
                
                FlorItem_01 florScript = nuevaPlantaObj.GetComponent<FlorItem_01>();
                if (florScript != null)
                {
                    florScript.ActivarPlantaEnGrilla(maceta);
                }

                ConsumirSemilla();
                sembradoExitoso = true;
                break;
            }
        }

        if (!sembradoExitoso)
        {
            RegresarAlInventario();
        }
    }

    public void RegresarAlInventario()
    {
        Transform spawnPoint = (inventario != null) ? inventario.GetSpawnPorTipo(tipoFlor) : padreOriginal;

        if (spawnPoint != null)
        {
            transform.SetParent(spawnPoint, false);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localPosition = Vector3.zero;
            
            // Restablecer la escala local al volver al inventario
            transform.localScale = Vector3.one; 
        }

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void ConsumirSemilla()
    {
        if (inventario != null)
        {
            inventario.ConsumirFlor(tipoFlor);
        }
        RegresarAlInventario();
    }

    public int GetCantidadDisponible()
    {
        if (inventario == null) return 0;
        switch (tipoFlor)
        {
            case 1: return inventario.Flor1;
            case 2: return inventario.Flor2;
            case 3: return inventario.Flor3;
            case 4: return inventario.Flor4;
            default: return 0;
        }
    }
}