using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private enum Paso { PonerEnvase, AbrirTienda, ComprarAlbahaca, VolverTienda, PlantarSemilla, Terminado }

    [Header("Referencias del tutorial")]
    [SerializeField] private RectTransform mano;
    [SerializeField] private TextMeshProUGUI textoTutorial;

    [Header("Objetivos")]
    [SerializeField] private RectTransform envaseInventario;
    [SerializeField] private GrillaUI grilla;
    [SerializeField] private Vector2Int casillaDestino = new Vector2Int(0, 0);
    [SerializeField] private RectTransform botonTienda;
    [SerializeField] private RectTransform botonAlbahaca;
    [SerializeField] private RectTransform botonVolver;
    [SerializeField] private int tipoAlbahaca = 1;

    [Header("Textos")]
    [SerializeField, TextArea] private string textoEnvase = "Arrastra el envase de yogurt a la repisa";
    [SerializeField, TextArea] private string textoTienda = "Ahora presiona el icono de la tienda para comprar una nueva planta";
    [SerializeField, TextArea] private string textoComprar = "Compra la semilla de albahaca";
    [SerializeField, TextArea] private string textoVolver = "Presiona el botón para volver";
    [SerializeField, TextArea] private string textoSemilla = "Arrastra la semilla de albahaca al envase de yogurt";

    [Header("Oscurecer")]
    [SerializeField] private Color colorOscuro = new Color(0f, 0f, 0f, 0.75f);
    [SerializeField] private float margenResaltado = 15f;
    [SerializeField] private float duracionFade = 0.3f;

    [Header("Mano")]
    [SerializeField] private Vector2 offsetMano = new Vector2(30f, -40f);
    [SerializeField] private float duracionArrastre = 1.2f;
    [SerializeField] private float escalaPresion = 0.85f;

    [Header("Opciones")]
    [SerializeField] private bool mostrarSoloUnaVez = false;
    [SerializeField] private bool cerrarAlmanaqueAlComprar = true;

    private const string ClaveCompletado = "TutorialCompletado";

    private RectTransform raiz;
    private RectTransform contenedorSombras;
    private CanvasGroup grupoSombras;
    private CanvasGroup grupoMano;
    private RectTransform sombraArriba, sombraAbajo, sombraIzquierda, sombraDerecha;
    private readonly Vector3[] esquinas = new Vector3[4];
    private Sequence secuenciaMano;
    private Vector3 escalaMano;
    private Paso paso;
    private bool arrastrando;
    private bool sombrasVisibles;
    private RectTransform objetivo;
    private RectTransform destino;
    private Compra_01 compra;

    private bool EsPasoDeArrastre => paso == Paso.PonerEnvase || paso == Paso.PlantarSemilla;

    private void Awake()
    {
        raiz = (RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform;

        GameObject contenedor = new GameObject("Sombras", typeof(RectTransform), typeof(CanvasGroup));
        contenedorSombras = (RectTransform)contenedor.transform;
        contenedorSombras.SetParent(raiz, false);
        contenedorSombras.SetAsFirstSibling();
        contenedorSombras.anchorMin = Vector2.zero;
        contenedorSombras.anchorMax = Vector2.one;
        contenedorSombras.offsetMin = Vector2.zero;
        contenedorSombras.offsetMax = Vector2.zero;
        grupoSombras = contenedor.GetComponent<CanvasGroup>();
        grupoSombras.alpha = 0f;
        grupoSombras.blocksRaycasts = false;

        sombraArriba = CrearSombra("Arriba");
        sombraAbajo = CrearSombra("Abajo");
        sombraIzquierda = CrearSombra("Izquierda");
        sombraDerecha = CrearSombra("Derecha");

        if (textoTutorial != null) textoTutorial.raycastTarget = false;

        if (mano != null)
        {
            mano.SetAsLastSibling();
            mano.anchorMin = mano.anchorMax = new Vector2(0.5f, 0.5f);
            escalaMano = mano.localScale;
            grupoMano = mano.GetComponent<CanvasGroup>();
            if (grupoMano == null) grupoMano = mano.gameObject.AddComponent<CanvasGroup>();
            grupoMano.blocksRaycasts = false;
            grupoMano.interactable = false;
            grupoMano.alpha = 0f;
            foreach (Graphic g in mano.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;
        }
    }

    private void OnEnable()
    {
        MacetaItemUI.AlColocar += AlColocarMaceta;
        Shop_01.AlAbrirTienda += AlAbrirTienda;
        Shop_01.AlCerrarTienda += AlCerrarTienda;
        Compra_01.AlComprarFlor += AlComprarFlor;
        SobreSemillaUI.AlSembrar += AlSembrar;
    }

    private void OnDisable()
    {
        MacetaItemUI.AlColocar -= AlColocarMaceta;
        Shop_01.AlAbrirTienda -= AlAbrirTienda;
        Shop_01.AlCerrarTienda -= AlCerrarTienda;
        Compra_01.AlComprarFlor -= AlComprarFlor;
        SobreSemillaUI.AlSembrar -= AlSembrar;
        DetenerMano();
    }

    private void Start()
    {
        compra = FindFirstObjectByType<Compra_01>();

        if (mostrarSoloUnaVez && PlayerPrefs.GetInt(ClaveCompletado, 0) == 1)
        {
            Terminar(false);
            return;
        }

        IrAPaso(Paso.PonerEnvase);
    }

    private void LateUpdate()
    {
        if (paso == Paso.Terminado) return;

        bool ahoraArrastrando = ArrastreVisual.HayArrastre;
        if (ahoraArrastrando != arrastrando)
        {
            arrastrando = ahoraArrastrando;
            if (arrastrando) DetenerMano();
        }

        objetivo = Valido(objetivo) ?? BuscarObjetivo();
        destino = EsPasoDeArrastre ? (Valido(destino) ?? BuscarDestino()) : null;

        MostrarSombras(objetivo != null && !arrastrando);
        if (objetivo != null) AjustarHueco(objetivo);

        bool listo = mano != null && objetivo != null && !arrastrando && (!EsPasoDeArrastre || destino != null);

        if (!listo)
        {
            if (secuenciaMano != null) DetenerMano();
            return;
        }

        if (secuenciaMano == null) IniciarAnimacionMano();

        if (!EsPasoDeArrastre)
            mano.anchoredPosition = PosicionMano(objetivo);
    }

    private void IrAPaso(Paso nuevo)
    {
        paso = nuevo;
        objetivo = null;
        destino = null;
        DetenerMano();

        if (paso == Paso.Terminado)
        {
            Terminar(true);
            return;
        }

        if (textoTutorial != null)
        {
            textoTutorial.gameObject.SetActive(true);
            textoTutorial.text = TextoDe(paso);
        }
    }

    private string TextoDe(Paso p)
    {
        switch (p)
        {
            case Paso.PonerEnvase: return textoEnvase;
            case Paso.AbrirTienda: return textoTienda;
            case Paso.ComprarAlbahaca: return textoComprar;
            case Paso.VolverTienda: return textoVolver;
            case Paso.PlantarSemilla: return textoSemilla;
            default: return string.Empty;
        }
    }

    private void Terminar(bool guardar)
    {
        paso = Paso.Terminado;
        DetenerMano();
        MostrarSombras(false);
        if (textoTutorial != null) textoTutorial.gameObject.SetActive(false);

        if (guardar && mostrarSoloUnaVez)
        {
            PlayerPrefs.SetInt(ClaveCompletado, 1);
            PlayerPrefs.Save();
        }
    }

    private void AlColocarMaceta(TamanoMaceta tamano)
    {
        if (paso == Paso.PonerEnvase && tamano == TamanoMaceta.Pequena) IrAPaso(Paso.AbrirTienda);
    }

    private void AlAbrirTienda()
    {
        if (paso == Paso.AbrirTienda) IrAPaso(Paso.ComprarAlbahaca);
    }

    private void AlComprarFlor(int tipo)
    {
        if (paso != Paso.ComprarAlbahaca || tipo != tipoAlbahaca) return;

        if (cerrarAlmanaqueAlComprar)
        {
            AlmanaqueManager almanaque = FindFirstObjectByType<AlmanaqueManager>();
            if (almanaque != null) almanaque.CerrarAlmanaque();
        }

        IrAPaso(Paso.VolverTienda);
    }

    private void AlCerrarTienda()
    {
        if (paso == Paso.VolverTienda) IrAPaso(Paso.PlantarSemilla);
    }

    private void AlSembrar(int tipo)
    {
        if (paso == Paso.PlantarSemilla && tipo == tipoAlbahaca) IrAPaso(Paso.Terminado);
    }

    private RectTransform BuscarObjetivo()
    {
        switch (paso)
        {
            case Paso.PonerEnvase: return BuscarEnvaseInventario();
            case Paso.AbrirTienda: return Valido(botonTienda);
            case Paso.ComprarAlbahaca: return BuscarBotonAlbahacaTienda();
            case Paso.VolverTienda: return Valido(botonVolver);
            case Paso.PlantarSemilla: return BuscarSemillaAlbahaca();
            default: return null;
        }
    }

    private RectTransform BuscarDestino()
    {
        if (paso == Paso.PonerEnvase) return BuscarRepisa();
        if (paso == Paso.PlantarSemilla) return BuscarEnvaseEnRepisa();
        return null;
    }

    private RectTransform BuscarEnvaseInventario()
    {
        if (Valido(envaseInventario) != null) return envaseInventario;

        foreach (MacetaItemUI item in FindObjectsByType<MacetaItemUI>(FindObjectsSortMode.None))
        {
            if (item.tamano == TamanoMaceta.Pequena && item.gameObject.activeInHierarchy)
                return (RectTransform)item.transform;
        }
        return null;
    }

    private RectTransform BuscarBotonAlbahacaTienda()
    {
        if (Valido(botonAlbahaca) != null) return botonAlbahaca;

        string metodo = "ComprarFlor" + tipoAlbahaca;
        foreach (Button boton in FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            if (!boton.gameObject.activeInHierarchy) continue;
            for (int i = 0; i < boton.onClick.GetPersistentEventCount(); i++)
            {
                if (boton.onClick.GetPersistentMethodName(i) == metodo)
                    return (RectTransform)boton.transform;
            }
        }
        return null;
    }

    private RectTransform BuscarRepisa()
    {
        if (grilla == null) grilla = FindFirstObjectByType<GrillaUI>();

        if (grilla != null)
        {
            CasillaUI elegida = grilla.GetCasilla(casillaDestino.x, casillaDestino.y);
            if (elegida != null && !elegida.estaOcupada && elegida.gameObject.activeInHierarchy)
                return (RectTransform)elegida.transform;
        }

        foreach (CasillaUI casilla in FindObjectsByType<CasillaUI>(FindObjectsSortMode.InstanceID))
        {
            if (!casilla.estaOcupada && casilla.gameObject.activeInHierarchy)
                return (RectTransform)casilla.transform;
        }
        return null;
    }

    private RectTransform BuscarSemillaAlbahaca()
    {
        if (compra == null) compra = FindFirstObjectByType<Compra_01>();
        if (compra == null) return null;

        Transform spawn = compra.GetSpawnPorTipo(tipoAlbahaca);
        if (spawn == null || spawn.childCount == 0) return null;

        return Valido(spawn.GetChild(0) as RectTransform);
    }

    private RectTransform BuscarEnvaseEnRepisa()
    {
        foreach (MacetaUI maceta in FindObjectsByType<MacetaUI>(FindObjectsSortMode.None))
        {
            bool enRepisa = maceta.transform.parent != null && maceta.transform.parent.GetComponent<CasillaUI>() != null;
            if (enRepisa && maceta.EsPequena && !maceta.tienePlanta && maceta.gameObject.activeInHierarchy)
                return (RectTransform)maceta.transform;
        }
        return null;
    }

    private static RectTransform Valido(RectTransform r)
    {
        return r != null && r.gameObject.activeInHierarchy ? r : null;
    }

    private void IniciarAnimacionMano()
    {
        mano.gameObject.SetActive(true);

        if (EsPasoDeArrastre) AnimarArrastre();
        else AnimarToque();
    }

    private void AnimarToque()
    {
        mano.localScale = escalaMano;
        mano.anchoredPosition = PosicionMano(objetivo);

        secuenciaMano = DOTween.Sequence()
            .SetUpdate(true)
            .Append(grupoMano.DOFade(1f, 0.2f))
            .Append(mano.DOScale(escalaMano * escalaPresion, 0.25f).SetEase(Ease.OutQuad))
            .Append(mano.DOScale(escalaMano, 0.25f).SetEase(Ease.OutBack))
            .AppendInterval(0.5f)
            .OnComplete(() => secuenciaMano = null);
    }

    private void AnimarArrastre()
    {
        Vector2 inicio = PosicionMano(objetivo);
        Vector2 fin = PosicionMano(destino);

        mano.localScale = escalaMano;
        mano.anchoredPosition = inicio;
        grupoMano.alpha = 0f;

        secuenciaMano = DOTween.Sequence()
            .SetUpdate(true)
            .Append(grupoMano.DOFade(1f, 0.25f))
            .Append(mano.DOScale(escalaMano * escalaPresion, 0.2f))
            .Append(mano.DOAnchorPos(fin, duracionArrastre).SetEase(Ease.InOutSine))
            .Append(mano.DOScale(escalaMano, 0.2f))
            .Append(grupoMano.DOFade(0f, 0.25f))
            .AppendInterval(0.35f)
            .OnComplete(() => secuenciaMano = null);
    }

    private void DetenerMano()
    {
        if (secuenciaMano != null) secuenciaMano.Kill();
        secuenciaMano = null;

        if (mano == null) return;
        mano.DOKill();
        if (grupoMano != null)
        {
            grupoMano.DOKill();
            grupoMano.alpha = 0f;
        }
        mano.localScale = escalaMano;
    }

    private Vector2 PosicionMano(RectTransform rt)
    {
        RectTransform espacio = (RectTransform)mano.parent;
        return RectEn(espacio, rt).center - espacio.rect.center + offsetMano;
    }

    private void MostrarSombras(bool mostrar)
    {
        if (mostrar == sombrasVisibles) return;
        sombrasVisibles = mostrar;
        grupoSombras.DOKill();
        grupoSombras.DOFade(mostrar ? 1f : 0f, duracionFade).SetUpdate(true);
        grupoSombras.blocksRaycasts = mostrar;
    }

    private void AjustarHueco(RectTransform rt)
    {
        Rect h = RectEn(contenedorSombras, rt);
        h.xMin -= margenResaltado;
        h.yMin -= margenResaltado;
        h.xMax += margenResaltado;
        h.yMax += margenResaltado;

        Rect r = contenedorSombras.rect;
        Colocar(sombraAbajo, r.xMin, r.yMin, r.xMax, h.yMin);
        Colocar(sombraArriba, r.xMin, h.yMax, r.xMax, r.yMax);
        Colocar(sombraIzquierda, r.xMin, h.yMin, h.xMin, h.yMax);
        Colocar(sombraDerecha, h.xMax, h.yMin, r.xMax, h.yMax);
    }

    private void Colocar(RectTransform sombra, float x0, float y0, float x1, float y1)
    {
        Vector2 centro = contenedorSombras.rect.center;
        sombra.anchoredPosition = new Vector2(x0, y0) - centro;
        sombra.sizeDelta = new Vector2(Mathf.Max(0f, x1 - x0), Mathf.Max(0f, y1 - y0));
    }

    private RectTransform CrearSombra(string nombre)
    {
        GameObject go = new GameObject("Sombra" + nombre, typeof(RectTransform), typeof(Image));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(contenedorSombras, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = Vector2.zero;
        go.GetComponent<Image>().color = colorOscuro;
        return rt;
    }

    private Rect RectEn(RectTransform espacio, RectTransform rt)
    {
        rt.GetWorldCorners(esquinas);
        Camera camaraObjetivo = CamaraDe(rt);
        Camera camaraEspacio = CamaraDe(espacio);

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        for (int i = 0; i < 4; i++)
        {
            Vector2 pantalla = RectTransformUtility.WorldToScreenPoint(camaraObjetivo, esquinas[i]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(espacio, pantalla, camaraEspacio, out Vector2 local);
            min = Vector2.Min(min, local);
            max = Vector2.Max(max, local);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private static Camera CamaraDe(Transform t)
    {
        Canvas c = t.GetComponentInParent<Canvas>();
        if (c == null) return null;
        c = c.rootCanvas;
        return c.renderMode == RenderMode.ScreenSpaceOverlay ? null : c.worldCamera;
    }
}