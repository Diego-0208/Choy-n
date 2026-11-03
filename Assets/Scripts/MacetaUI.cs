using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum TamanoMaceta
{
    Grande = 0,
    Pequena = 1
}

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class MacetaUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Estado de Maceta")]
    public bool tienePlanta = false;
    public PlantaBase plantaActual;

    [Header("Tamaño")]
    [SerializeField] private TamanoMaceta tamano = TamanoMaceta.Grande;
    [SerializeField] private Sprite spriteGrande;
    [SerializeField] private Sprite spritePequena;
    [SerializeField] private Vector2 medidasGrande = new Vector2(225f, 225f);
    [SerializeField] private Vector2 medidasPequena = new Vector2(150f, 150f);
    [SerializeField] private float escalaPlantaGrande = 1f;
    [SerializeField] private float escalaPlantaPequena = 0.8f;

    [Header("Posición de la maceta")]
    [SerializeField] private float margenInferior = 0f;

    [Header("Posición de la planta")]
    [SerializeField, Range(0f, 1f)] private float alturaTierraGrande = 0.8f;
    [SerializeField, Range(0f, 1f)] private float alturaTierraPequena = 0.8f;
    [SerializeField] private float ajusteYGrande = 0f;
    [SerializeField] private float ajusteYPequena = 0f;

    [Header("Semillas")]
    [SerializeField] private bool grandeAceptaSemillas = false;

    [Header("Trasplante")]
    [SerializeField] private Color colorTrasplanteListo = new Color(0.75f, 1f, 0.75f, 1f);

    private bool trasplanteHabilitado;
    private Color colorOriginal = Color.white;

    private Transform padreOriginal;
    private CasillaUI casillaOrigen;
    private bool soltadaEnCasilla;
    private Canvas canvasPrincipal;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Image imagen;

    public static MacetaUI Arrastrada { get; private set; }

    public TamanoMaceta Tamano => tamano;
    public bool EsPequena => tamano == TamanoMaceta.Pequena;
    public bool AceptaSemillas => EsPequena || grandeAceptaSemillas;

    public bool PuedeTrasplantar => trasplanteHabilitado && tienePlanta && plantaActual != null;

    public void HabilitarTrasplante()
    {
        trasplanteHabilitado = true;
        if (imagen != null) imagen.color = colorTrasplanteListo;
    }

    public void ConsumirPermisoTrasplante()
    {
        trasplanteHabilitado = false;
        if (imagen != null) imagen.color = colorOriginal;
    }

    public static bool HayMacetaGrandeVacia()
    {
        foreach (MacetaUI m in FindObjectsByType<MacetaUI>(FindObjectsSortMode.None))
        {
            bool enGrilla = m.transform.parent != null && m.transform.parent.GetComponent<CasillaUI>() != null;
            if (enGrilla && !m.EsPequena && (!m.tienePlanta || m.plantaActual == null)) return true;
        }
        return false;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasPrincipal = GetComponentInParent<Canvas>();
        imagen = GetComponent<Image>();
        colorOriginal = imagen.color;
        AplicarTamanoVisual();
    }

    private void OnValidate()
    {
        if (Application.isPlaying && plantaActual != null) AcomodarPlanta();
    }

    public void CambiarTamano(TamanoMaceta nuevo)
    {
        tamano = nuevo;
        AplicarTamanoVisual();
    }

    private void AplicarTamanoVisual()
    {
        if (imagen == null) imagen = GetComponent<Image>();
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        Sprite s = EsPequena ? spritePequena : spriteGrande;
        if (s != null) imagen.sprite = s;
        rectTransform.sizeDelta = EsPequena ? medidasPequena : medidasGrande;

        if (transform.parent != null && transform.parent.GetComponent<CasillaUI>() != null)
            AcomodarEnCasilla(transform.parent);

        AcomodarPlanta();
    }

    public void AcomodarEnCasilla(Transform casilla)
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        transform.SetParent(casilla, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
        rectTransform.anchoredPosition = new Vector2(0f, margenInferior);
    }

    private void AcomodarPlanta()
    {
        if (plantaActual == null) return;
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (imagen == null) imagen = GetComponent<Image>();

        RectTransform rectPlanta = plantaActual.GetComponent<RectTransform>();
        if (rectPlanta != null)
        {
            float alturaTierra = EsPequena ? alturaTierraPequena : alturaTierraGrande;
            float ajuste = EsPequena ? ajusteYPequena : ajusteYGrande;

            ObtenerZonaDibujada(out float baseY, out float alturaDibujada);

            rectPlanta.anchorMin = new Vector2(0.5f, 0f);
            rectPlanta.anchorMax = new Vector2(0.5f, 0f);
            rectPlanta.pivot = new Vector2(0.5f, 0f);
            rectPlanta.anchoredPosition = new Vector2(0f, baseY + alturaDibujada * alturaTierra + ajuste);
        }

        float escala = EsPequena ? escalaPlantaPequena : escalaPlantaGrande;
        plantaActual.transform.localScale = new Vector3(escala, escala, 1f);
    }

    private void ObtenerZonaDibujada(out float baseY, out float alturaDibujada)
    {
        Rect r = rectTransform.rect;
        baseY = 0f;
        alturaDibujada = r.height;

        if (imagen == null || imagen.sprite == null || !imagen.preserveAspect) return;

        Rect s = imagen.sprite.rect;
        if (s.height <= 0f || r.width <= 0f || r.height <= 0f) return;

        float aspectoSprite = s.width / s.height;
        float aspectoRect = r.width / r.height;

        if (aspectoSprite > aspectoRect)
        {
            alturaDibujada = r.width / aspectoSprite;
            baseY = (r.height - alturaDibujada) * 0.5f;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        padreOriginal = transform.parent;
        casillaOrigen = padreOriginal != null ? padreOriginal.GetComponent<CasillaUI>() : null;
        soltadaEnCasilla = false;
        canvasGroup.blocksRaycasts = false;
        Arrastrada = this;

        if (canvasPrincipal == null) canvasPrincipal = GetComponentInParent<Canvas>();
        transform.SetParent(canvasPrincipal.transform);
        transform.SetAsLastSibling();

        if (MenuCuidadosUI.Instance != null) MenuCuidadosUI.Instance.OcultarPanel();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        if (Arrastrada == this) Arrastrada = null;
        if (soltadaEnCasilla) return;
        GestorSonidos.Reproducir(Sonido.MovimientoInvalido);
        VolverAlOrigen();
    }

    public void SoltarEnCasilla(CasillaUI destino)
    {
        soltadaEnCasilla = true;

        if (destino == casillaOrigen)
        {
            VolverAlOrigen();
            return;
        }

        if (casillaOrigen != null) casillaOrigen.VaciarCasilla();

        AcomodarEnCasilla(destino.transform);
        destino.estaOcupada = true;
        casillaOrigen = destino;
        padreOriginal = destino.transform;

        GuardarGrilla();
        GestorSonidos.Reproducir(Sonido.ColocarMaceta);

    }

    public bool GuardarEnInventario()
    {
        if (tienePlanta && plantaActual != null)
        {
            Debug.Log("Solo se pueden guardar macetas vacías. Vende o desecha la planta primero.");
            return false;
        }

        soltadaEnCasilla = true;
        if (casillaOrigen != null) casillaOrigen.VaciarCasilla();

        Compra_01 inventario = FindFirstObjectByType<Compra_01>(FindObjectsInactive.Include);
        if (inventario != null) inventario.Devolver(tamano);

        transform.SetParent(null);
        Destroy(gameObject);
        GuardarGrilla();
        GestorSonidos.Reproducir(Sonido.GuardarMaceta); 
        return true;
    }

    private void VolverAlOrigen()
    {
        if (padreOriginal != null) AcomodarEnCasilla(padreOriginal);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (tienePlanta && plantaActual == null)
        {
            tienePlanta = false;
        }

        if (tienePlanta) return;

        GameObject objetoArrastrado = eventData.pointerDrag;
        if (objetoArrastrado == null) return;

        SobreSemillaUI sobreSemilla = objetoArrastrado.GetComponent<SobreSemillaUI>();
        if (sobreSemilla == null || sobreSemilla.prefabPlanta == null) return;

        if (!AceptaSemillas)
        {
            Debug.Log("Las semillas se siembran en un envase pequeño. Trasplanta después a una maceta grande.");
            return;
        }

        GameObject nuevaPlantaGO = Instantiate(sobreSemilla.prefabPlanta, transform);
        ColocarPlanta(nuevaPlantaGO.GetComponent<PlantaBase>());
        sobreSemilla.ConsumirSemilla();
        GestorSonidos.Reproducir(Sonido.Sembrar);
        GuardarGrilla();
    }

    public void ColocarPlanta(PlantaBase planta)
    {
        if (planta == null) return;

        planta.transform.SetParent(transform, false);

        plantaActual = planta;
        tienePlanta = true;
        planta.enabled = true;
        planta.ActualizarSpriteVisual();
        AcomodarPlanta();
    }

    public void SoltarPlanta()
    {
        plantaActual = null;
        tienePlanta = false;
    }

    public void LimpiarPlanta()
    {
        if (plantaActual != null)
        {
            Destroy(plantaActual.gameObject);
            plantaActual = null;
        }
        tienePlanta = false;
        ConsumirPermisoTrasplante();
    }

    private static void GuardarGrilla()
    {
        GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
        if (grilla != null) grilla.GuardarEstadoGrilla();
    }
}