using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class MacetaItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static event System.Action<TamanoMaceta> AlColocar;

    [Header("Configuración de Prefab")]
    public GameObject prefabMaceta;
    [Tooltip("Grande = maceta normal. Pequena = envase de yogurt (acepta semillas).")]
    public TamanoMaceta tamano = TamanoMaceta.Grande;

    private ArrastreVisual arrastre;

    public void OnBeginDrag(PointerEventData eventData)
    {
        arrastre = null;

        if (ArrastreVisual.PasarAlScrollSiEsHorizontal(gameObject, eventData)) return;

        Compra_01 inventario = FindFirstObjectByType<Compra_01>();
        if (inventario != null && inventario.CantidadDe(tamano) <= 0)
        {
            Debug.Log(tamano == TamanoMaceta.Pequena
                ? "¡No tienes envases! Cómpralos en la tienda."
                : "¡No tienes macetas! Cómpralas en la tienda.");
            eventData.pointerDrag = null;
            GestorSonidos.Reproducir(Sonido.SinDinero);
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

        List<RaycastResult> resultados = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, resultados);

        foreach (RaycastResult resultado in resultados)
        {
            CasillaUI casilla = resultado.gameObject.GetComponent<CasillaUI>();
            if (casilla == null || casilla.estaOcupada) continue;

            if (prefabMaceta == null)
            {
                Debug.LogError("¡No has asignado el Prefab de la Maceta en el Inspector!");
                break;
            }

            GameObject nuevaMaceta = Instantiate(prefabMaceta, casilla.transform);

            MacetaUI maceta = nuevaMaceta.GetComponent<MacetaUI>();
            if (maceta != null)
            {
                maceta.AcomodarEnCasilla(casilla.transform);
                maceta.CambiarTamano(tamano);
            }

            casilla.estaOcupada = true;

            Compra_01 inventario = FindFirstObjectByType<Compra_01>();
            if (inventario != null) inventario.Consumir(tamano);
            GestorSonidos.Reproducir(Sonido.ColocarMaceta);

            AlColocar?.Invoke(tamano);

            GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
            if (grilla != null) grilla.GuardarEstadoGrilla();
            break;
        }
    }
}