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

    private Compra_01 inventario;
    private ArrastreVisual arrastre;

    private void Awake()
    {
        inventario = FindFirstObjectByType<Compra_01>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        arrastre = null;

        // Arrastre horizontal = mover el scroll del inventario
        if (ArrastreVisual.PasarAlScrollSiEsHorizontal(gameObject, eventData)) return;

        // Cancelar si no hay stock
        if (GetCantidadDisponible() <= 0)
        {
            eventData.pointerDrag = null;
            return;
        }

        arrastre = ArrastreVisual.Iniciar(gameObject, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (arrastre != null) arrastre.Mover(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (arrastre == null) return;
        arrastre.Terminar();
        arrastre = null;

        // Si MacetaUI.OnDrop ya sembró (se ejecuta antes que esto), la maceta ya tiene planta y no se repite.
        List<RaycastResult> resultados = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, resultados);

        foreach (RaycastResult resultado in resultados)
        {
            MacetaUI maceta = resultado.gameObject.GetComponent<MacetaUI>();

            if (maceta != null && !maceta.tienePlanta && maceta.AceptaSemillas)
            {
                GameObject nuevaPlantaObj = Instantiate(prefabPlanta, maceta.transform);
                maceta.ColocarPlanta(nuevaPlantaObj.GetComponent<PlantaBase>());
                ConsumirSemilla();

                GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
                if (grilla != null) grilla.GuardarEstadoGrilla();
                break;
            }
        }
    }

    /// <summary>Se mantiene por compatibilidad: el sobre ya no sale del inventario.</summary>
    public void RegresarAlInventario() { }

    public void ConsumirSemilla()
    {
        if (inventario == null) inventario = FindFirstObjectByType<Compra_01>();
        if (inventario != null)
        {
            inventario.ConsumirFlor(tipoFlor);
        }
    }

    public int GetCantidadDisponible()
    {
        if (inventario == null) inventario = FindFirstObjectByType<Compra_01>();
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