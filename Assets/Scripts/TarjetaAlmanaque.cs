using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TarjetaAlmanaque : MonoBehaviour
{
    private const string PrefijoNueva = "PlantaNueva_";

    [Header("Configuración")]
    public Almanaque datosAlmanaque;

    [Header("Referencias UI")]
    [SerializeField] private Button botonTarjeta;
    [SerializeField] private Image imagenPlanta;

    [Header("Signo de nueva")]
    [SerializeField] private RectTransform signoNueva;
    [SerializeField] private float escalaPulso = 1.25f;
    [SerializeField] private float duracionPulso = 0.45f;
    [SerializeField] private float duracionAparecer = 0.3f;

    private AlmanaqueManager manager;
    private Vector3 escalaSigno = Vector3.one;
    private Tween animacion;
    private bool signoVisible;

    public static void MarcarNueva(string idPlanta)
    {
        if (string.IsNullOrEmpty(idPlanta)) return;
        PlayerPrefs.SetInt(PrefijoNueva + idPlanta.Trim(), 1);
        PlayerPrefs.Save();
    }

    public static void MarcarVista(Almanaque planta)
    {
        if (planta == null || string.IsNullOrEmpty(planta.idPlanta)) return;
        PlayerPrefs.SetInt(PrefijoNueva + planta.idPlanta.Trim(), 0);
        PlayerPrefs.Save();
    }

    public static bool EsNueva(Almanaque planta)
    {
        if (planta == null || string.IsNullOrEmpty(planta.idPlanta)) return false;
        return PlayerPrefs.GetInt(PrefijoNueva + planta.idPlanta.Trim(), 0) == 1;
    }

    private void Awake()
    {
        if (manager == null)
        {
            manager = FindFirstObjectByType<AlmanaqueManager>();
        }

        if (signoNueva != null)
        {
            escalaSigno = signoNueva.localScale;
            foreach (Graphic g in signoNueva.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;
        }
    }

    private void OnEnable()
    {
        signoVisible = false;
        RefrescarSigno();
    }

    private void OnDisable()
    {
        DetenerSigno();
    }

    public void Inicializar(AlmanaqueManager almanaqueManager)
    {
        manager = almanaqueManager;

        if (botonTarjeta == null)
            botonTarjeta = GetComponent<Button>();

        if (imagenPlanta == null)
            imagenPlanta = GetComponent<Image>();

        if (botonTarjeta != null)
        {
            botonTarjeta.onClick.RemoveListener(OnClickTarjeta);
            botonTarjeta.onClick.AddListener(OnClickTarjeta);
        }

        ActualizarEstado();
    }

    public void ActualizarEstado()
    {
        if (manager == null)
            manager = FindFirstObjectByType<AlmanaqueManager>();

        if (datosAlmanaque == null || manager == null) return;

        bool desbloqueada = manager.EstaPlantaDesbloqueada(datosAlmanaque);

        gameObject.SetActive(desbloqueada);

        if (desbloqueada)
        {
            if (imagenPlanta != null && datosAlmanaque.img != null)
            {
                imagenPlanta.sprite = datosAlmanaque.img;
            }

            if (botonTarjeta != null)
            {
                botonTarjeta.interactable = true;
            }

            RefrescarSigno();
        }
    }

    public void OnClickTarjeta()
    {
        if (gameObject.activeSelf && manager != null && datosAlmanaque != null)
        {
            manager.BotonPlanta(datosAlmanaque);
        }
    }

    public void RefrescarSigno()
    {
        if (signoNueva == null) return;

        bool mostrar = EsNueva(datosAlmanaque);
        if (mostrar && signoVisible && animacion != null) return;
        signoVisible = mostrar;

        DetenerSigno();

        if (!mostrar || !gameObject.activeInHierarchy)
        {
            signoNueva.gameObject.SetActive(mostrar);
            return;
        }

        signoNueva.gameObject.SetActive(true);
        signoNueva.localScale = Vector3.zero;

        animacion = signoNueva.DOScale(escalaSigno, duracionAparecer)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                animacion = signoNueva.DOScale(escalaSigno * escalaPulso, duracionPulso)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);
            });
    }

    private void DetenerSigno()
    {
        if (animacion != null) animacion.Kill();
        animacion = null;
        if (signoNueva != null)
        {
            signoNueva.DOKill();
            signoNueva.localScale = escalaSigno;
        }
    }
}