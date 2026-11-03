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

    private MacetaUI macetaOrigen;
    private Canvas canvasPrincipal;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private PlantaBase scriptPlanta;
    private bool arrastrando;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasPrincipal = GetComponentInParent<Canvas>();
        scriptPlanta = GetComponent<PlantaBase>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        arrastrando = false;
        macetaOrigen = transform.parent != null ? transform.parent.GetComponent<MacetaUI>() : null;

        if (macetaOrigen == null)
        {
            eventData.pointerDrag = null;
            return;
        }

        if (!macetaOrigen.PuedeTrasplantar)
        {
            eventData.pointerDrag = macetaOrigen.gameObject;
            ExecuteEvents.Execute(macetaOrigen.gameObject, eventData, ExecuteEvents.beginDragHandler);
            return;
        }

        arrastrando = true;
        macetaOrigen.SoltarPlanta();

        if (canvasPrincipal == null) canvasPrincipal = GetComponentInParent<Canvas>();
        transform.SetParent(canvasPrincipal.rootCanvas.transform, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;

        if (MenuCuidadosUI.Instance != null) MenuCuidadosUI.Instance.OcultarPanel();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!arrastrando) return;
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!arrastrando) return;
        arrastrando = false;
        canvasGroup.blocksRaycasts = true;

        List<RaycastResult> resultados = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, resultados);

        MacetaUI destino = null;
        foreach (RaycastResult resultado in resultados)
        {
            MacetaUI m = resultado.gameObject.GetComponentInParent<MacetaUI>();
            if (m != null && m != macetaOrigen && !m.EsPequena && (!m.tienePlanta || m.plantaActual == null))
            {
                destino = m;
                break;
            }
        }

        if (destino != null)
        {
            macetaOrigen.ConsumirPermisoTrasplante();
            destino.ColocarPlanta(scriptPlanta);
            GestorSonidos.Reproducir(Sonido.Trasplantar);
        }
        else
        {
            Debug.Log("Suelta el brote sobre una maceta grande vacía.");
            GestorSonidos.Reproducir(Sonido.MovimientoInvalido); 
            macetaOrigen.ColocarPlanta(scriptPlanta);
        }

        GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
        if (grilla != null) grilla.GuardarEstadoGrilla();
    }

    public void ActivarPlantaEnGrilla(MacetaUI nuevaMaceta = null)
    {
        if (nuevaMaceta != null) nuevaMaceta.ColocarPlanta(scriptPlanta);
        else if (scriptPlanta != null)
        {
            scriptPlanta.enabled = true;
            scriptPlanta.ActualizarSpriteVisual();
        }
    }

    public void AcomodarEnMaceta(MacetaUI maceta)
    {
        if (maceta != null) maceta.ColocarPlanta(scriptPlanta);
    }
}