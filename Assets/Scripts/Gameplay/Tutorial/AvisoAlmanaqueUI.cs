using DG.Tweening;
using UnityEngine;

public class AvisoAlmanaqueUI : MonoBehaviour
{
    public const string ClaveAviso = "AlmanaqueAvisoPendiente";

    [SerializeField] private RectTransform signo;
    [SerializeField] private float escalaPulso = 1.25f;
    [SerializeField] private float duracionPulso = 0.45f;
    [SerializeField] private float duracionAparecer = 0.3f;

    private Vector3 escalaBase = Vector3.one;
    private Tween animacion;
    private bool visible;

    public static void MarcarPendiente()
    {
        PlayerPrefs.SetInt(ClaveAviso, 1);
        PlayerPrefs.Save();
    }

    public static void MarcarVisto()
    {
        PlayerPrefs.SetInt(ClaveAviso, 0);
        PlayerPrefs.Save();
    }

    private void Awake()
    {
        if (signo != null) escalaBase = signo.localScale;
    }

    private void OnEnable()
    {
        Compra_01.AlDesbloquearPlanta += AlDesbloquear;
        AlmanaqueManager.AlAbrirAlmanaque += Refrescar;
        visible = false;
        Refrescar();
    }

    private void OnDisable()
    {
        Compra_01.AlDesbloquearPlanta -= AlDesbloquear;
        AlmanaqueManager.AlAbrirAlmanaque -= Refrescar;
        Detener();
    }

    private void AlDesbloquear(int tipo)
    {
        Refrescar();
    }

    private void Refrescar()
    {
        if (signo == null) return;

        bool mostrar = PlayerPrefs.GetInt(ClaveAviso, 0) == 1;
        if (mostrar && visible && animacion != null) return;
        visible = mostrar;

        Detener();

        if (!mostrar)
        {
            signo.gameObject.SetActive(false);
            return;
        }

        signo.gameObject.SetActive(true);
        signo.localScale = Vector3.zero;

        animacion = signo.DOScale(escalaBase, duracionAparecer)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                animacion = signo.DOScale(escalaBase * escalaPulso, duracionPulso)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);
            });
    }

    private void Detener()
    {
        if (animacion != null) animacion.Kill();
        animacion = null;
        if (signo != null)
        {
            signo.DOKill();
            signo.localScale = escalaBase;
        }
    }
}