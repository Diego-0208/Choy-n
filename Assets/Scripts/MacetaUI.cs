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

    [Header("Trasplante")]
    [SerializeField] private bool exigirMinijuegoAlTrasplantar = true;
    [SerializeField] private Color colorTrasplanteListo = new Color(0.75f, 1f, 0.75f, 1f);
    public bool trasplanteHabilitado = false; // se activa al ganar el minijuego (botón Trasplantar)

    private Transform padreOriginal;
    private CasillaUI casillaOrigen;
    private bool soltadaEnCasilla;   // la maceta fue soltada sobre una casilla (lo avisa CasillaUI.OnDrop)
    private bool arrastreCancelado;
    private Image imagen;
    private Color colorOriginal;
    private Canvas canvasPrincipal;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasPrincipal = GetComponentInParent<Canvas>();
        imagen = GetComponent<Image>();
        colorOriginal = imagen.color;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Con planta, la maceta solo se puede mover si ya se ganó el minijuego (botón Trasplantar)
        arrastreCancelado = false;
        if (tienePlanta && exigirMinijuegoAlTrasplantar && !trasplanteHabilitado)
        {
            Debug.Log("Usa el botón Trasplantar para poder mover esta maceta.");
            arrastreCancelado = true;
            eventData.pointerDrag = null;
            return;
        }

        padreOriginal = transform.parent;
        casillaOrigen = padreOriginal != null ? padreOriginal.GetComponent<CasillaUI>() : null;
        soltadaEnCasilla = false;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvasPrincipal.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (arrastreCancelado) return;
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (arrastreCancelado) return;
        canvasGroup.blocksRaycasts = true;

        // Si CasillaUI.OnDrop ya se hizo cargo (colocó la maceta o inició el minijuego), no hacemos nada.
        if (soltadaEnCasilla) return;

        // Soltada en cualquier otro lado (o en una casilla ocupada): vuelve a su lugar
        VolverAlOrigen();
    }

    /// <summary>Lo llama CasillaUI.OnDrop cuando esta maceta se suelta sobre una casilla libre.</summary>
    public void SoltarEnCasilla(CasillaUI destino)
    {
        soltadaEnCasilla = true;

        if (destino == casillaOrigen)
        {
            VolverAlOrigen();
            return;
        }

        MoverACasilla(destino);
    }

    /// <summary>Lo llama MinijuegoTrasplanteUI cuando el jugador gana el minijuego.</summary>
    public void HabilitarTrasplante()
    {
        trasplanteHabilitado = true;
        if (imagen != null) imagen.color = colorTrasplanteListo;
    }

    private void BloquearTrasplante()
    {
        trasplanteHabilitado = false;
        if (imagen != null) imagen.color = colorOriginal;
    }

    private void VolverAlOrigen()
    {
        if (padreOriginal != null) MoverAMacetaCasilla(padreOriginal);
    }

    private void MoverACasilla(CasillaUI destino)
    {
        if (casillaOrigen != null) casillaOrigen.VaciarCasilla();

        MoverAMacetaCasilla(destino.transform);
        destino.estaOcupada = true;
        casillaOrigen = destino;
        padreOriginal = destino.transform;
        BloquearTrasplante(); // el permiso vale para un solo movimiento

        GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
        if (grilla != null) grilla.GuardarEstadoGrilla();
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