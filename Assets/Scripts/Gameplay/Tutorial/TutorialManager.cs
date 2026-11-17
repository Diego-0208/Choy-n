using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private enum Paso
    {
        PonerEnvase,
        AbrirTienda,
        ComprarAlbahaca,
        VolverTienda,
        RevisarAlmanaque,
        ExplorarAlmanaque,
        PlantarSemilla,
        VerInfo,
        Regar,
        Fertilizar,
        Trasplantar,
        QuitarMoho,
        QuitarPlagas,
        Cosechar,
        Terminado
    }

    private enum Accion { Ninguna, TocarPlanta, TocarBoton, PonerMacetaGrande, ArrastrarPlanta, EnMinijuego }

    [Serializable]
    private class TextosCuidado
    {
        [TextArea] public string tocarPlanta;
        [TextArea] public string tocarBoton;

        public TextosCuidado() { }

        public TextosCuidado(string planta, string boton)
        {
            tocarPlanta = planta;
            tocarBoton = boton;
        }
    }

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
    [SerializeField] private RectTransform botonAlmanaque;
    [SerializeField] private int tipoAlbahaca = 1;

    [Header("Cuidados")]
    [SerializeField] private GestorMinijuegosPrincipal gestorMinijuegos;
    [SerializeField] private BotonesCuidadosUI botonesCuidados;
    [SerializeField] private RectTransform macetaGrandeInventario;
    [SerializeField] private float margenAguaSeca = 5f;
    [SerializeField] private float nivelAguaSano = 60f;
    [SerializeField] private int cantidadMohoTutorial = 3;
    [SerializeField] private int cantidadPlagasTutorial = 3;

    [Header("Textos")]
    [SerializeField, TextArea] private string textoEnvase = "Arrastra el envase de yogurt a la repisa";
    [SerializeField, TextArea] private string textoTienda = "Ahora presiona el icono de la tienda para comprar una nueva planta";
    [SerializeField, TextArea] private string textoComprar = "Compra la semilla de albahaca";
    [SerializeField, TextArea] private string textoVolver = "Presiona el botón para volver";
    [SerializeField, TextArea] private string textoAlmanaque = "¡Has desbloqueado una nueva planta en el almanaque! Presiona el almanaque para revisarla";
    [SerializeField, TextArea] private string textoExplorarAlmanaque = "";
    [SerializeField, TextArea] private string textoSemilla = "Arrastra la semilla de albahaca al envase de yogurt";

    [Header("Panel de info")]
    [SerializeField, TextArea] private string textoVerInfo = "Toca tu planta para ver sus detalles";
    [SerializeField, TextArea] private string textoPanelInfo = "Aquí puedes ver los detalles de tu planta: su nombre, su etapa y su salud";
    [SerializeField] private float duracionPanelInfo = 3.5f;
    [SerializeField, TextArea] private string textoNecesitaAgua = "¡Oh, tu planta necesita agua! Presiona el icono de regar";
    [SerializeField] private RectTransform panelInfo;

    [Header("Textos de cuidados")]
    [SerializeField] private TextosCuidado textosRegar = new TextosCuidado(
        "¡Tu planta está seca! Tócala para ver qué necesita",
        "Presiona el icono de regar");
    [SerializeField] private TextosCuidado textosFertilizar = new TextosCuidado(
        "Ahora ayudemos a que crezca. Toca tu planta",
        "Presiona el icono de fertilizar");
    [SerializeField] private TextosCuidado textosTrasplantar = new TextosCuidado(
        "Tu planta necesita más espacio. Tócala",
        "Presiona el icono de trasplantar");
    [SerializeField] private TextosCuidado textosMoho = new TextosCuidado(
        "¡Oh no, le salió moho! Toca tu planta",
        "Presiona el icono de quitar moho");
    [SerializeField] private TextosCuidado textosPlagas = new TextosCuidado(
        "¡Llegaron plagas! Toca tu planta",
        "Presiona el icono de quitar plagas");
    [SerializeField] private TextosCuidado textosCosechar = new TextosCuidado(
        "¡Tu planta está lista para cosechar! Tócala",
        "Presiona el icono de cosechar");
    [SerializeField, TextArea] private string textoPonerMacetaGrande = "Arrastra una maceta grande a la repisa";
    [SerializeField, TextArea] private string textoArrastrarPlanta = "Arrastra tu planta a la maceta grande";
    [SerializeField, TextArea] private string textoFinal = "¡Muy bien! Ya sabes cuidar tus plantas";
    [SerializeField] private float duracionTextoFinal = 3f;

    [Header("Oscurecer")]
    [SerializeField] private Color colorOscuro = new Color(0f, 0f, 0f, 0.75f);
    [SerializeField] private float margenResaltado = 15f;
    [SerializeField] private float duracionFade = 0.3f;

    [Header("Mano")]
    [SerializeField] private Vector2 offsetMano = new Vector2(30f, -40f);
    [SerializeField] private float duracionArrastre = 1.2f;
    [SerializeField] private float escalaPresion = 0.85f;

    [Header("Posición del texto")]
    [SerializeField] private RectTransform contenedorTexto;
    [SerializeField] private float margenTexto = 20f;
    [SerializeField] private float margenBordeTexto = 20f;
    [SerializeField] private float tiempoMovimientoTexto = 0.15f;
    [SerializeField] private int ordenEncima = 100;

    [Header("Opciones")]
    [SerializeField] private bool cerrarAlmanaqueAlComprar = true;
    [SerializeField] private bool empezarEnCuidados = false;
    [SerializeField] private bool reiniciarTutorialAlEmpezar = false;

    private const string ClaveCompletado = "TutorialCompletado";
    private const string ClavePaso = "TutorialPaso";

    public static bool TutorialActivo { get; private set; }
    public static CasillaUI CasillaPermitida { get; private set; }

    public static bool PuedeColocarEn(CasillaUI casilla)
    {
        return !TutorialActivo || (casilla != null && casilla == CasillaPermitida);
    }

    private RectTransform raiz;
    private RectTransform contenedorSombras;
    private CanvasGroup grupoSombras;
    private CanvasGroup grupoMano;
    private RectTransform sombraArriba, sombraAbajo, sombraIzquierda, sombraDerecha;
    private readonly Vector3[] esquinas = new Vector3[4];
    private Sequence secuenciaMano;
    private Tween tweenTextoFinal;
    private Vector3 escalaMano;
    private Paso paso;
    private Accion accion;
    private bool arrastrando;
    private bool sombrasVisibles;
    private bool pasoPreparado;
    private bool retomando;
    private Paso pasoGuardado;
    private RectTransform objetivo;
    private RectTransform objetivoExtra;
    private RectTransform destino;
    private CasillaUI casillaElegida;
    private Compra_01 compra;
    private PlantaBase planta;
    private GestorSaludPlanta salud;
    private RectTransform rectTexto;
    private RectTransform espacioTexto;
    private Canvas canvasTexto;
    private Canvas canvasMano;
    private Vector2 posTextoOriginal;
    private Vector2 posTextoMeta;
    private Vector2 velocidadTexto;
    private bool textoVisibleAntes;
    private bool esperandoRecompensa;
    private bool vinoDeInfo;
    private float tiempoPanelInfo;

    private bool MostrandoPanelInfo => paso == Paso.VerInfo && accion == Accion.TocarBoton;
    private readonly List<Rect> obstaculos = new List<Rect>();
    private readonly List<Vector2> candidatos = new List<Vector2>();

    private bool EsPasoCuidado => paso >= Paso.VerInfo && paso < Paso.Terminado;

    private bool EsPasoDeArrastre =>
        paso == Paso.PonerEnvase || paso == Paso.PlantarSemilla ||
        accion == Accion.PonerMacetaGrande || accion == Accion.ArrastrarPlanta;

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

        rectTexto = contenedorTexto != null ? contenedorTexto : (textoTutorial != null ? textoTutorial.rectTransform : null);
        if (rectTexto != null)
        {
            espacioTexto = rectTexto.parent as RectTransform;
            posTextoOriginal = rectTexto.anchoredPosition;
            posTextoMeta = posTextoOriginal;
            canvasTexto = CanvasEncima(rectTexto.gameObject, ordenEncima);
        }

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
            canvasMano = CanvasEncima(mano.gameObject, ordenEncima + 1);
        }
    }

    private void OnEnable()
    {
        MacetaItemUI.AlColocar += AlColocarMaceta;
        Shop_01.AlAbrirTienda += AlAbrirTienda;
        Shop_01.AlCerrarTienda += AlCerrarTienda;
        Compra_01.AlComprarFlor += AlComprarFlor;
        AlmanaqueManager.AlAbrirAlmanaque += AlAbrirAlmanaque;
        AlmanaqueManager.AlCerrarAlmanaque += AlCerrarAlmanaque;
        SobreSemillaUI.AlSembrar += AlSembrar;
        GestorMinijuegosPrincipal.AlCerrarMinijuego += AlCerrarMinijuego;
    }

    private void OnDisable()
    {
        MacetaItemUI.AlColocar -= AlColocarMaceta;
        Shop_01.AlAbrirTienda -= AlAbrirTienda;
        Shop_01.AlCerrarTienda -= AlCerrarTienda;
        Compra_01.AlComprarFlor -= AlComprarFlor;
        AlmanaqueManager.AlAbrirAlmanaque -= AlAbrirAlmanaque;
        AlmanaqueManager.AlCerrarAlmanaque -= AlCerrarAlmanaque;
        SobreSemillaUI.AlSembrar -= AlSembrar;
        GestorMinijuegosPrincipal.AlCerrarMinijuego -= AlCerrarMinijuego;
        DetenerMano();
    }

    private void OnDestroy()
    {
        if (tweenTextoFinal != null) tweenTextoFinal.Kill();
        TutorialActivo = false;
        CasillaPermitida = null;
    }

    private void Start()
    {
        compra = FindFirstObjectByType<Compra_01>();
        if (gestorMinijuegos == null) gestorMinijuegos = FindFirstObjectByType<GestorMinijuegosPrincipal>(FindObjectsInactive.Include);
        if (botonesCuidados == null) botonesCuidados = FindFirstObjectByType<BotonesCuidadosUI>(FindObjectsInactive.Include);

        if (reiniciarTutorialAlEmpezar) ReiniciarTutorial();

        if (PlayerPrefs.GetInt(ClaveCompletado, 0) == 1)
        {
            Terminar(false);
            return;
        }

        TutorialActivo = true;

        int guardado = PlayerPrefs.GetInt(ClavePaso, 0);
        if (guardado < 0 || guardado >= (int)Paso.Terminado) guardado = 0;

        pasoGuardado = empezarEnCuidados ? Paso.VerInfo : (Paso)guardado;
        retomando = true;
        MostrarTexto(string.Empty);
    }

    [ContextMenu("Reiniciar tutorial")]
    private void ReiniciarTutorial()
    {
        PlayerPrefs.DeleteKey(ClaveCompletado);
        PlayerPrefs.DeleteKey(ClavePaso);
        PlayerPrefs.Save();
    }

    private Paso PasoAlRetomar(Paso p)
    {
        if (p >= Paso.VerInfo) return p;
        if (HayPlantaEnRepisa()) return Paso.VerInfo;
        if (p == Paso.ExplorarAlmanaque) return Paso.PlantarSemilla;
        if (p == Paso.RevisarAlmanaque || p == Paso.PlantarSemilla) return p;

        bool tieneSemilla = BuscarSemillaAlbahaca() != null;
        if (p == Paso.PonerEnvase && !HayEnvaseEnRepisa()) return Paso.PonerEnvase;
        return tieneSemilla ? Paso.RevisarAlmanaque : Paso.AbrirTienda;
    }

    private bool HayEnvaseEnRepisa()
    {
        foreach (MacetaUI maceta in FindObjectsByType<MacetaUI>(FindObjectsSortMode.None))
        {
            bool enRepisa = maceta.transform.parent != null && maceta.transform.parent.GetComponent<CasillaUI>() != null;
            if (enRepisa && maceta.EsPequena) return true;
        }
        return false;
    }

    private bool HayPlantaEnRepisa()
    {
        foreach (MacetaUI maceta in FindObjectsByType<MacetaUI>(FindObjectsSortMode.None))
        {
            bool enRepisa = maceta.transform.parent != null && maceta.transform.parent.GetComponent<CasillaUI>() != null;
            if (enRepisa && maceta.tienePlanta && maceta.plantaActual != null && maceta.plantaActual.EstaViva) return true;
        }
        return false;
    }

    private void LateUpdate()
    {
        if (RecompensaDiaria.HayRecompensaPendiente)
        {
            EsperarRecompensa();
            return;
        }

        if (esperandoRecompensa) RetomarTrasRecompensa();

        ActualizarTutorial();
        AcomodarTexto();
    }

    private void EsperarRecompensa()
    {
        if (esperandoRecompensa) return;
        esperandoRecompensa = true;

        CasillaPermitida = null;
        DetenerMano();
        MostrarSombras(false);
        textoVisibleAntes = false;
        if (textoTutorial != null) textoTutorial.gameObject.SetActive(false);
    }

    private void RetomarTrasRecompensa()
    {
        esperandoRecompensa = false;
        if (paso == Paso.Terminado || retomando) return;

        if (EsPasoCuidado) accion = Accion.Ninguna;
        else MostrarTexto(TextoDe(paso));
    }

    private void ActualizarTutorial()
    {
        if (retomando)
        {
            retomando = false;
            IrAPaso(PasoAlRetomar(pasoGuardado));
        }

        if (paso == Paso.Terminado) return;

        if (EsPasoCuidado && !ActualizarCuidado())
        {
            CasillaPermitida = null;
            if (secuenciaMano != null) DetenerMano();
            MostrarSombras(false);
            return;
        }

        bool ahoraArrastrando = HayArrastre();
        if (ahoraArrastrando != arrastrando)
        {
            arrastrando = ahoraArrastrando;
            if (arrastrando) DetenerMano();
        }

        objetivo = Valido(objetivo) ?? BuscarObjetivo();
        objetivoExtra = ObjetivoExtra();
        destino = EsPasoDeArrastre ? (Valido(destino) ?? BuscarDestino()) : null;
        CasillaPermitida = CalcularCasillaPermitida();

        MostrarSombras(objetivo != null && !arrastrando);
        if (objetivo != null) AjustarHueco(objetivo, objetivoExtra);

        bool listo = mano != null && objetivo != null && !arrastrando && !MostrandoPanelInfo && (!EsPasoDeArrastre || destino != null);

        if (!listo)
        {
            if (secuenciaMano != null) DetenerMano();
            return;
        }

        if (secuenciaMano == null) IniciarAnimacionMano();

        if (!EsPasoDeArrastre)
            mano.anchoredPosition = PosicionMano(objetivo);
    }

    private bool HayArrastre()
    {
        if (ArrastreVisual.HayArrastre || MacetaUI.Arrastrada != null) return true;
        return EsPasoCuidado && planta != null && planta.MacetaActual == null;
    }

    private void IrAPaso(Paso nuevo)
    {
        paso = nuevo;
        accion = Accion.Ninguna;
        vinoDeInfo = false;
        tiempoPanelInfo = 0f;
        pasoPreparado = false;
        LimpiarObjetivos();

        if (paso == Paso.Terminado)
        {
            Terminar(true);
            return;
        }

        PlayerPrefs.SetInt(ClavePaso, (int)paso);
        PlayerPrefs.Save();

        if (EsPasoCuidado) return;

        MostrarTexto(TextoDe(paso));
    }

    private void LimpiarObjetivos()
    {
        objetivo = null;
        objetivoExtra = null;
        destino = null;
        casillaElegida = null;
        CasillaPermitida = null;
        DetenerMano();
    }

    private void MostrarTexto(string texto)
    {
        if (textoTutorial == null) return;
        textoTutorial.gameObject.SetActive(!string.IsNullOrEmpty(texto));
        textoTutorial.text = texto;
        AplicarOrden(canvasTexto, ordenEncima);
    }

    private string TextoDe(Paso p)
    {
        switch (p)
        {
            case Paso.PonerEnvase: return textoEnvase;
            case Paso.AbrirTienda: return textoTienda;
            case Paso.ComprarAlbahaca: return textoComprar;
            case Paso.VolverTienda: return textoVolver;
            case Paso.RevisarAlmanaque: return textoAlmanaque;
            case Paso.ExplorarAlmanaque: return textoExplorarAlmanaque;
            case Paso.PlantarSemilla: return textoSemilla;
            default: return string.Empty;
        }
    }

    private void Terminar(bool guardar)
    {
        paso = Paso.Terminado;
        accion = Accion.Ninguna;
        TutorialActivo = false;
        CasillaPermitida = null;
        DetenerMano();
        MostrarSombras(false);

        if (salud != null)
        {
            salud.PausadaPorTutorial = false;
            if (guardar) salud.FijarNivelAgua(Mathf.Max(salud.NivelAgua, nivelAguaSano));
        }

        if (guardar && textoTutorial != null && !string.IsNullOrEmpty(textoFinal))
        {
            MostrarTexto(textoFinal);
            tweenTextoFinal = DOVirtual.DelayedCall(duracionTextoFinal, () =>
            {
                if (textoTutorial != null) textoTutorial.gameObject.SetActive(false);
            }).SetUpdate(true);
        }
        else if (textoTutorial != null)
        {
            textoTutorial.gameObject.SetActive(false);
        }

        if (guardar)
        {
            PlayerPrefs.SetInt(ClaveCompletado, 1);
            PlayerPrefs.DeleteKey(ClavePaso);
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
        if (paso == Paso.VolverTienda) IrAPaso(Paso.RevisarAlmanaque);
    }

    private void AlAbrirAlmanaque()
    {
        if (paso == Paso.RevisarAlmanaque) IrAPaso(Paso.ExplorarAlmanaque);
    }

    private void AlCerrarAlmanaque()
    {
        if (paso == Paso.ExplorarAlmanaque) IrAPaso(Paso.PlantarSemilla);
    }

    private void AlSembrar(int tipo)
    {
        if (paso == Paso.PlantarSemilla && tipo == tipoAlbahaca) IrAPaso(Paso.VerInfo);
    }

    private bool ActualizarCuidado()
    {
        if (!ObtenerPlanta())
        {
            MostrarTexto(string.Empty);
            return false;
        }

        if (!pasoPreparado)
        {
            while (paso < Paso.Terminado && !Aplica(paso)) paso++;

            if (paso == Paso.Terminado)
            {
                Terminar(true);
                return false;
            }

            Preparar(paso);
            pasoPreparado = true;
            accion = Accion.Ninguna;
        }

        if (paso == Paso.VerInfo)
        {
            if (accion == Accion.TocarBoton && MenuAbiertoEnPlanta())
            {
                tiempoPanelInfo += Time.unscaledDeltaTime;
                if (tiempoPanelInfo >= duracionPanelInfo)
                {
                    IrAPaso(Paso.Regar);
                    vinoDeInfo = true;
                    return false;
                }
            }
            else
            {
                tiempoPanelInfo = 0f;
            }
        }

        if (paso == Paso.Trasplantar && planta.YaTrasplantada)
        {
            SiguientePasoCuidado();
            return false;
        }

        Accion nueva = CalcularAccion();
        if (nueva != accion)
        {
            accion = nueva;
            LimpiarObjetivos();
            MostrarTexto(TextoCuidado());
        }

        return accion != Accion.EnMinijuego;
    }

    private bool ObtenerPlanta()
    {
        if (planta != null && planta.EstaViva) return true;

        planta = null;
        salud = null;

        PlantaBase elegida = null;
        foreach (MacetaUI maceta in FindObjectsByType<MacetaUI>(FindObjectsSortMode.InstanceID))
        {
            bool enRepisa = maceta.transform.parent != null && maceta.transform.parent.GetComponent<CasillaUI>() != null;
            if (!enRepisa || !maceta.tienePlanta || maceta.plantaActual == null || !maceta.plantaActual.EstaViva) continue;

            if (elegida == null || maceta.EsPequena) elegida = maceta.plantaActual;
            if (maceta.EsPequena) break;
        }

        if (elegida == null) return false;

        planta = elegida;
        salud = planta.GetComponent<GestorSaludPlanta>();
        if (salud != null) salud.PausadaPorTutorial = true;
        return salud != null;
    }

    private bool Aplica(Paso p)
    {
        GestorMinijuegosPrincipal g = gestorMinijuegos;

        switch (p)
        {
            case Paso.VerInfo:
                return true;
            case Paso.Regar:
                return planta.Permite(PlantaBase.Minijuego.Riego) && g != null && g.TieneMinijuegoRiego;
            case Paso.Fertilizar:
                return planta.Permite(PlantaBase.Minijuego.Fertilizante) && g != null && g.TieneMinijuegoFertilizante && !planta.EsAdulta;
            case Paso.Trasplantar:
                return planta.Permite(PlantaBase.Minijuego.Trasplante) && planta.HorasHastaTrasplante >= 0f;
            case Paso.QuitarMoho:
                return planta.Permite(PlantaBase.Minijuego.Moho) && g != null;
            case Paso.QuitarPlagas:
                return planta.Permite(PlantaBase.Minijuego.Plagas) && g != null;
            case Paso.Cosechar:
                return planta.Permite(PlantaBase.Minijuego.Cosecha) && g != null && g.TieneMinijuegoCosecha && planta.TemporizadorCosecha <= 0f;
            default:
                return false;
        }
    }

    private void Preparar(Paso p)
    {
        switch (p)
        {
            case Paso.Regar:
                salud.FijarNivelAgua(salud.UmbralSequia - margenAguaSeca);
                break;
            case Paso.Trasplantar:
                float faltan = planta.HorasHastaTrasplante;
                if (faltan > 0f) planta.AcelerarCrecimiento(faltan + 0.01f);
                break;
            case Paso.QuitarMoho:
                if (salud.CantidadMoho <= 0) salud.AgregarMoho(cantidadMohoTutorial);
                break;
            case Paso.QuitarPlagas:
                if (salud.CantidadPlagas <= 0) salud.AgregarPlagas(cantidadPlagasTutorial);
                break;
            case Paso.Cosechar:
                if (!planta.EsAdulta)
                {
                    float horas = (planta.EtapaMaxima - planta.EtapaActual + 1) * planta.TiempoParaCrecer;
                    planta.AcelerarCrecimiento(horas);
                }
                break;
        }
    }

    private void SiguientePasoCuidado()
    {
        IrAPaso(paso + 1);
    }

    private void AlCerrarMinijuego(PlantaBase.Minijuego minijuego)
    {
        if (!EsPasoCuidado || !pasoPreparado || minijuego != MinijuegoDe(paso) || salud == null) return;

        switch (paso)
        {
            case Paso.Regar:
                if (salud.NivelAgua <= salud.UmbralSequia) return;
                if (salud.NivelAgua >= salud.UmbralExcesoAgua) salud.FijarNivelAgua(nivelAguaSano);
                break;
            case Paso.QuitarMoho:
                if (salud.CantidadMoho > 0) return;
                break;
            case Paso.QuitarPlagas:
                if (salud.CantidadPlagas > 0) return;
                break;
            case Paso.Trasplantar:
                return;
        }

        SiguientePasoCuidado();
    }

    private PlantaBase.Minijuego MinijuegoDe(Paso p)
    {
        switch (p)
        {
            case Paso.Regar: return PlantaBase.Minijuego.Riego;
            case Paso.Fertilizar: return PlantaBase.Minijuego.Fertilizante;
            case Paso.Trasplantar: return PlantaBase.Minijuego.Trasplante;
            case Paso.QuitarMoho: return PlantaBase.Minijuego.Moho;
            case Paso.QuitarPlagas: return PlantaBase.Minijuego.Plagas;
            case Paso.Cosechar: return PlantaBase.Minijuego.Cosecha;
            default: return PlantaBase.Minijuego.Ninguno;
        }
    }

    private Accion CalcularAccion()
    {
        if (gestorMinijuegos != null && gestorMinijuegos.HayMinijuegoAbierto) return Accion.EnMinijuego;

        MacetaUI maceta = planta.MacetaActual;
        if (maceta == null) return accion;

        if (paso == Paso.Trasplantar)
        {
            if (maceta.PuedeTrasplantar) return Accion.ArrastrarPlanta;
            if (!MacetaUI.HayMacetaGrandeVacia()) return Accion.PonerMacetaGrande;
        }

        return MenuAbiertoEnPlanta() ? Accion.TocarBoton : Accion.TocarPlanta;
    }

    private bool MenuAbiertoEnPlanta()
    {
        CasillaUI casilla = CasillaDePlanta();
        return MenuCuidadosUI.Instance != null && casilla != null && MenuCuidadosUI.Instance.CasillaSeleccionada == casilla;
    }

    private RectTransform ObjetivoExtra()
    {
        if (accion == Accion.TocarPlanta) return MacetaDePlanta();
        return null;
    }

    private RectTransform PanelInfo()
    {
        if (Valido(panelInfo) != null) return panelInfo;

        InfoPlantaUI info = FindFirstObjectByType<InfoPlantaUI>();
        return info != null ? Valido(info.transform as RectTransform) : null;
    }

    private string TextoCuidado()
    {
        switch (accion)
        {
            case Accion.EnMinijuego: return string.Empty;
            case Accion.PonerMacetaGrande: return textoPonerMacetaGrande;
            case Accion.ArrastrarPlanta: return textoArrastrarPlanta;
        }

        if (paso == Paso.VerInfo) return accion == Accion.TocarBoton ? textoPanelInfo : textoVerInfo;
        if (paso == Paso.Regar && vinoDeInfo && accion == Accion.TocarBoton && !string.IsNullOrEmpty(textoNecesitaAgua)) return textoNecesitaAgua;

        TextosCuidado textos = TextosDe(paso);
        if (textos == null) return string.Empty;
        return accion == Accion.TocarBoton ? textos.tocarBoton : textos.tocarPlanta;
    }

    private TextosCuidado TextosDe(Paso p)
    {
        switch (p)
        {
            case Paso.Regar: return textosRegar;
            case Paso.Fertilizar: return textosFertilizar;
            case Paso.Trasplantar: return textosTrasplantar;
            case Paso.QuitarMoho: return textosMoho;
            case Paso.QuitarPlagas: return textosPlagas;
            case Paso.Cosechar: return textosCosechar;
            default: return null;
        }
    }

    private CasillaUI CasillaDePlanta()
    {
        MacetaUI maceta = planta != null ? planta.MacetaActual : null;
        if (maceta == null || maceta.transform.parent == null) return null;
        return maceta.transform.parent.GetComponent<CasillaUI>();
    }

    private RectTransform MacetaDePlanta()
    {
        MacetaUI maceta = planta != null ? planta.MacetaActual : null;
        return maceta != null ? Valido((RectTransform)maceta.transform) : null;
    }

    private RectTransform PlantaRect()
    {
        return planta != null ? Valido((RectTransform)planta.transform) : null;
    }

    private RectTransform BuscarObjetivo()
    {
        switch (paso)
        {
            case Paso.PonerEnvase: return BuscarMacetaInventario(TamanoMaceta.Pequena, envaseInventario);
            case Paso.AbrirTienda: return Valido(botonTienda);
            case Paso.ComprarAlbahaca: return BuscarBotonAlbahacaTienda();
            case Paso.VolverTienda: return Valido(botonVolver);
            case Paso.RevisarAlmanaque: return BuscarBotonAlmanaque();
            case Paso.ExplorarAlmanaque: return null;
            case Paso.PlantarSemilla: return BuscarSemillaAlbahaca();
        }

        switch (accion)
        {
            case Accion.TocarPlanta: return PlantaRect();
            case Accion.TocarBoton: return paso == Paso.VerInfo ? PanelInfo() : BuscarBotonCuidado();
            case Accion.PonerMacetaGrande: return BuscarMacetaInventario(TamanoMaceta.Grande, macetaGrandeInventario);
            case Accion.ArrastrarPlanta: return PlantaRect();
            default: return null;
        }
    }

    private RectTransform BuscarDestino()
    {
        if (paso == Paso.PonerEnvase) return ElegirCasilla(casillaDestino);
        if (paso == Paso.PlantarSemilla) return BuscarEnvaseEnRepisa();
        if (accion == Accion.PonerMacetaGrande) return ElegirCasillaJuntoAlEnvase();
        if (accion == Accion.ArrastrarPlanta) return BuscarMacetaGrandeVacia();
        return null;
    }

    private RectTransform BuscarBotonAlmanaque()
    {
        if (Valido(botonAlmanaque) != null) return botonAlmanaque;

        AvisoAlmanaqueUI aviso = FindFirstObjectByType<AvisoAlmanaqueUI>();
        return aviso != null ? Valido(aviso.transform as RectTransform) : null;
    }

    private RectTransform BuscarBotonCuidado()
    {
        if (botonesCuidados == null) botonesCuidados = FindFirstObjectByType<BotonesCuidadosUI>(FindObjectsInactive.Include);
        if (botonesCuidados == null) return null;

        GameObject boton = botonesCuidados.BotonDe(MinijuegoDe(paso));
        return boton != null ? Valido(boton.transform as RectTransform) : null;
    }

    private RectTransform BuscarMacetaInventario(TamanoMaceta tamano, RectTransform asignado)
    {
        if (Valido(asignado) != null) return asignado;

        foreach (MacetaItemUI item in FindObjectsByType<MacetaItemUI>(FindObjectsSortMode.None))
        {
            if (item.tamano == tamano && item.gameObject.activeInHierarchy)
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

    private CasillaUI CalcularCasillaPermitida()
    {
        if (paso == Paso.PonerEnvase || accion == Accion.PonerMacetaGrande) return casillaElegida;
        if (accion == Accion.ArrastrarPlanta && destino != null) return destino.GetComponentInParent<CasillaUI>();
        return null;
    }

    private RectTransform ElegirCasillaJuntoAlEnvase()
    {
        if (casillaElegida != null && !casillaElegida.estaOcupada && casillaElegida.gameObject.activeInHierarchy)
            return (RectTransform)casillaElegida.transform;

        casillaElegida = null;
        CasillaUI centro = CasillaDePlanta();
        int mejor = int.MaxValue;

        foreach (CasillaUI casilla in FindObjectsByType<CasillaUI>(FindObjectsSortMode.InstanceID))
        {
            if (casilla.estaOcupada || !casilla.gameObject.activeInHierarchy) continue;

            int puntaje = centro != null ? Puntaje(centro, casilla) : 0;
            if (puntaje < mejor)
            {
                mejor = puntaje;
                casillaElegida = casilla;
            }
        }

        return casillaElegida != null ? (RectTransform)casillaElegida.transform : null;
    }

    private static int Puntaje(CasillaUI centro, CasillaUI casilla)
    {
        int dx = casilla.posX - centro.posX;
        int dy = Mathf.Abs(casilla.posY - centro.posY);
        return dy * 1000 + Mathf.Abs(dx) * 2 + (dx > 0 ? 0 : 1);
    }

    private RectTransform ElegirCasilla(Vector2Int posicion)
    {
        if (casillaElegida != null && !casillaElegida.estaOcupada && casillaElegida.gameObject.activeInHierarchy)
            return (RectTransform)casillaElegida.transform;

        casillaElegida = null;
        if (grilla == null) grilla = FindFirstObjectByType<GrillaUI>();

        if (grilla != null)
        {
            CasillaUI elegida = grilla.GetCasilla(posicion.x, posicion.y);
            if (elegida != null && !elegida.estaOcupada && elegida.gameObject.activeInHierarchy)
                casillaElegida = elegida;
        }

        if (casillaElegida == null)
        {
            foreach (CasillaUI casilla in FindObjectsByType<CasillaUI>(FindObjectsSortMode.InstanceID))
            {
                if (!casilla.estaOcupada && casilla.gameObject.activeInHierarchy)
                {
                    casillaElegida = casilla;
                    break;
                }
            }
        }

        return casillaElegida != null ? (RectTransform)casillaElegida.transform : null;
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

    private RectTransform BuscarMacetaGrandeVacia()
    {
        MacetaUI origen = planta != null ? planta.MacetaActual : null;
        CasillaUI centro = origen != null && origen.transform.parent != null ? origen.transform.parent.GetComponent<CasillaUI>() : null;
        MacetaUI elegida = null;
        int mejor = int.MaxValue;

        foreach (MacetaUI maceta in FindObjectsByType<MacetaUI>(FindObjectsSortMode.InstanceID))
        {
            if (maceta == origen || maceta.EsPequena || !maceta.gameObject.activeInHierarchy) continue;
            if (maceta.tienePlanta && maceta.plantaActual != null) continue;

            CasillaUI casilla = maceta.transform.parent != null ? maceta.transform.parent.GetComponent<CasillaUI>() : null;
            if (casilla == null) continue;

            int puntaje = centro != null ? Puntaje(centro, casilla) : 0;
            if (puntaje < mejor)
            {
                mejor = puntaje;
                elegida = maceta;
            }
        }

        return elegida != null ? (RectTransform)elegida.transform : null;
    }

    private static RectTransform Valido(RectTransform r)
    {
        return r != null && r.gameObject.activeInHierarchy ? r : null;
    }

    private void IniciarAnimacionMano()
    {
        mano.gameObject.SetActive(true);
        AplicarOrden(canvasMano, ordenEncima + 1);

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

    private void AjustarHueco(RectTransform rt, RectTransform extra)
    {
        Rect h = RectEn(contenedorSombras, rt);
        if (extra != null)
        {
            Rect e = RectEn(contenedorSombras, extra);
            h = Rect.MinMaxRect(Mathf.Min(h.xMin, e.xMin), Mathf.Min(h.yMin, e.yMin), Mathf.Max(h.xMax, e.xMax), Mathf.Max(h.yMax, e.yMax));
        }

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

    private Canvas CanvasEncima(GameObject go, int orden)
    {
        Canvas c = go.GetComponent<Canvas>();
        if (c == null) c = go.AddComponent<Canvas>();
        AplicarOrden(c, orden);
        return c;
    }

    private void AplicarOrden(Canvas c, int orden)
    {
        if (c == null) return;
        Canvas raizCanvas = raiz != null ? raiz.GetComponent<Canvas>() : null;
        c.overrideSorting = true;
        if (raizCanvas != null)
        {
            c.sortingLayerID = raizCanvas.sortingLayerID;
            c.sortingOrder = raizCanvas.sortingOrder + orden;
        }
        else
        {
            c.sortingOrder = orden;
        }
    }

    private void AcomodarTexto()
    {
        if (rectTexto == null || espacioTexto == null || textoTutorial == null) return;

        bool visible = textoTutorial.gameObject.activeInHierarchy && !string.IsNullOrEmpty(textoTutorial.text);
        if (!visible)
        {
            textoVisibleAntes = false;
            return;
        }

        ReunirObstaculos();

        Rect area = espacioTexto.rect;
        area.xMin += margenBordeTexto;
        area.yMin += margenBordeTexto;
        area.xMax -= margenBordeTexto;
        area.yMax -= margenBordeTexto;

        Rect actual = RectEn(espacioTexto, rectTexto);
        Vector2 posActual = rectTexto.anchoredPosition;

        if (Choque(RectEnPos(actual, posActual, posTextoOriginal)) <= 0f)
        {
            posTextoMeta = posTextoOriginal;
        }
        else if (Choque(RectEnPos(actual, posActual, posTextoMeta)) > 0f)
        {
            posTextoMeta = MejorPosicion(actual, posActual, area);
        }

        if (!textoVisibleAntes)
        {
            rectTexto.anchoredPosition = posTextoMeta;
            velocidadTexto = Vector2.zero;
        }
        else
        {
            rectTexto.anchoredPosition = Vector2.SmoothDamp(posActual, posTextoMeta, ref velocidadTexto, tiempoMovimientoTexto, Mathf.Infinity, Time.unscaledDeltaTime);
        }

        textoVisibleAntes = true;
    }

    private void ReunirObstaculos()
    {
        obstaculos.Clear();
        if (paso != Paso.Terminado)
        {
            AgregarObstaculo(objetivo);
            AgregarObstaculo(objetivoExtra);
            AgregarObstaculo(destino);

            if (EsPasoCuidado)
            {
                AgregarObstaculo(PlantaRect());
                AgregarObstaculo(MacetaDePlanta());
            }

            if (secuenciaMano != null && !EsPasoDeArrastre) AgregarObstaculo(mano);
        }

        if (MacetaUI.Arrastrada != null) AgregarObstaculo(MacetaUI.Arrastrada.transform as RectTransform);
        AgregarObstaculo(ArrastreVisual.FantasmaActual);
    }

    private void AgregarObstaculo(RectTransform rt)
    {
        if (Valido(rt) == null) return;
        Rect r = RectEn(espacioTexto, rt);
        r.xMin -= margenTexto;
        r.yMin -= margenTexto;
        r.xMax += margenTexto;
        r.yMax += margenTexto;
        obstaculos.Add(r);
    }

    private Vector2 MejorPosicion(Rect actual, Vector2 posActual, Rect area)
    {
        candidatos.Clear();
        candidatos.Add(posTextoOriginal);

        Rect original = RectEnPos(actual, posActual, posTextoOriginal);

        candidatos.Add(posTextoOriginal + new Vector2(0f, area.yMax - original.yMax));
        candidatos.Add(posTextoOriginal + new Vector2(0f, area.yMin - original.yMin));

        bool hayUnion = false;
        Rect union = default;

        foreach (Rect o in obstaculos)
        {
            AgregarCandidatosAlrededor(o, original);
            union = hayUnion ? Unir(union, o) : o;
            hayUnion = true;
        }

        if (hayUnion) AgregarCandidatosAlrededor(union, original);

        Vector2 mejor = posTextoOriginal;
        float mejorPuntaje = float.MaxValue;

        foreach (Vector2 c in candidatos)
        {
            Vector2 p = Encerrar(c, actual, posActual, area);
            float puntaje = Choque(RectEnPos(actual, posActual, p)) * 10f + Vector2.Distance(p, posTextoOriginal);
            if (puntaje < mejorPuntaje)
            {
                mejorPuntaje = puntaje;
                mejor = p;
            }
        }

        return mejor;
    }

    private void AgregarCandidatosAlrededor(Rect o, Rect original)
    {
        candidatos.Add(posTextoOriginal + new Vector2(0f, o.yMax - original.yMin));
        candidatos.Add(posTextoOriginal + new Vector2(0f, o.yMin - original.yMax));
        candidatos.Add(posTextoOriginal + new Vector2(o.xMax - original.xMin, 0f));
        candidatos.Add(posTextoOriginal + new Vector2(o.xMin - original.xMax, 0f));

        float centroX = o.center.x - original.center.x;
        candidatos.Add(posTextoOriginal + new Vector2(centroX, o.yMax - original.yMin));
        candidatos.Add(posTextoOriginal + new Vector2(centroX, o.yMin - original.yMax));
    }

    private Vector2 Encerrar(Vector2 p, Rect actual, Vector2 posActual, Rect area)
    {
        Rect r = RectEnPos(actual, posActual, p);
        Vector2 ajuste = Vector2.zero;

        if (r.width > area.width) ajuste.x = area.center.x - r.center.x;
        else if (r.xMin < area.xMin) ajuste.x = area.xMin - r.xMin;
        else if (r.xMax > area.xMax) ajuste.x = area.xMax - r.xMax;

        if (r.height > area.height) ajuste.y = area.center.y - r.center.y;
        else if (r.yMin < area.yMin) ajuste.y = area.yMin - r.yMin;
        else if (r.yMax > area.yMax) ajuste.y = area.yMax - r.yMax;

        return p + ajuste;
    }

    private static Rect RectEnPos(Rect actual, Vector2 posActual, Vector2 pos)
    {
        actual.position += pos - posActual;
        return actual;
    }

    private float Choque(Rect r)
    {
        float total = 0f;
        foreach (Rect o in obstaculos)
        {
            float x = Mathf.Min(r.xMax, o.xMax) - Mathf.Max(r.xMin, o.xMin);
            float y = Mathf.Min(r.yMax, o.yMax) - Mathf.Max(r.yMin, o.yMin);
            if (x > 0f && y > 0f) total += x * y;
        }
        return total;
    }

    private static Rect Unir(Rect a, Rect b)
    {
        return Rect.MinMaxRect(Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin), Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));
    }
}