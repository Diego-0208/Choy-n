using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ManoPanoUI : MonoBehaviour
{
    [SerializeField] private RectTransform visual;
    [SerializeField] private Vector2 desplazamiento = new Vector2(20f, -20f);
    [SerializeField] private float suavizado = 25f;
    [SerializeField] private float tiempoParaOcultar = 0.35f;
    [SerializeField] private float duracionFade = 0.15f;

    [Header("Frotado")]
    [SerializeField] private float amplitudFrotado = 18f;
    [SerializeField] private float anguloFrotado = 12f;
    [SerializeField] private float periodoFrotado = 0.22f;

    [Header("Demostración")]
    [SerializeField] private float amplitudDemostracion = 35f;
    [SerializeField] private float periodoDemostracion = 0.45f;
    [SerializeField] private float duracionDemostracion = 2f;

    [Header("Limpieza final")]
    [SerializeField] private float distanciaBarrido = 60f;
    [SerializeField] private float duracionBarrido = 0.25f;

    private RectTransform rect;
    private CanvasGroup grupo;
    private Sequence frotado;
    private Sequence barrido;
    private Tween fade;
    private Tween finDemostracion;
    private Vector3 objetivo;
    private float ultimoSeguimiento;
    private bool visible;
    private bool siguiendo;

    public bool EnDemostracion { get; private set; }
    public bool Ocupada => siguiendo || EnDemostracion || (barrido != null && barrido.IsActive());
    public float DuracionDemostracion => duracionDemostracion;

    private void Awake()
    {
        rect = (RectTransform)transform;
        grupo = GetComponent<CanvasGroup>();
        grupo.blocksRaycasts = false;
        grupo.interactable = false;
        grupo.alpha = 0f;

        if (visual == null && rect.childCount > 0) visual = (RectTransform)rect.GetChild(0);

        foreach (Graphic grafico in GetComponentsInChildren<Graphic>(true))
        {
            grafico.raycastTarget = false;
        }
    }

    private void OnDisable()
    {
        frotado?.Kill();
        barrido?.Kill();
        fade?.Kill();
        finDemostracion?.Kill();

        siguiendo = false;
        visible = false;
        EnDemostracion = false;

        if (grupo != null) grupo.alpha = 0f;
        ReiniciarVisual();
    }

    private void Update()
    {
        if (!siguiendo) return;

        float k = 1f - Mathf.Exp(-suavizado * Time.deltaTime);
        rect.position = Vector3.Lerp(rect.position, objetivo, k);

        if (Time.time - ultimoSeguimiento > tiempoParaOcultar)
        {
            Ocultar();
        }
    }

    public void Seguir(PointerEventData eventData)
    {
        RectTransform padre = rect.parent as RectTransform;
        if (padre == null) return;

        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(padre, eventData.position, eventData.pressEventCamera, out Vector3 mundo))
            return;

        objetivo = mundo + padre.TransformVector(desplazamiento);
        ultimoSeguimiento = Time.time;

        if (siguiendo) return;

        CancelarDemostracion();
        barrido?.Kill();

        if (!visible) rect.position = objetivo;

        siguiendo = true;
        Mostrar();
        IniciarFrotado(amplitudFrotado, periodoFrotado);
    }

    public void MostrarDemostracion(Transform objetivoDemo)
    {
        if (Ocupada || objetivoDemo == null) return;

        EnDemostracion = true;
        rect.position = objetivoDemo.position + rect.parent.TransformVector(desplazamiento);

        Mostrar();
        IniciarFrotado(amplitudDemostracion, periodoDemostracion);

        finDemostracion?.Kill();
        finDemostracion = DOVirtual.DelayedCall(duracionDemostracion, () =>
        {
            EnDemostracion = false;
            Ocultar();
        }).SetLink(gameObject);
    }

    public void LimpiezaFinal(Vector3 posicionMundo)
    {
        CancelarDemostracion();
        siguiendo = false;
        frotado?.Kill();
        barrido?.Kill();
        fade?.Kill();

        Vector3 centro = posicionMundo + rect.parent.TransformVector(desplazamiento);
        Vector3 direccion = rect.parent.TransformVector(new Vector3(distanciaBarrido, 0f, 0f));

        visible = true;
        grupo.alpha = 1f;

        barrido = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(visual.DOLocalMove(Vector3.zero, 0.05f))
            .Join(visual.DOLocalRotate(new Vector3(0f, 0f, anguloFrotado), 0.05f))
            .Append(rect.DOMove(centro + direccion, duracionBarrido).SetEase(Ease.OutQuad))
            .Join(visual.DOLocalRotate(new Vector3(0f, 0f, -anguloFrotado * 1.5f), duracionBarrido).SetEase(Ease.OutQuad))
            .Append(grupo.DOFade(0f, duracionFade))
            .OnComplete(() =>
            {
                visible = false;
                ReiniciarVisual();
            });
    }

    public void Ocultar()
    {
        siguiendo = false;
        frotado?.Kill();

        fade?.Kill();
        fade = grupo.DOFade(0f, duracionFade)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                visible = false;
                ReiniciarVisual();
            });
    }

    private void Mostrar()
    {
        fade?.Kill();
        visible = true;
        fade = grupo.DOFade(1f, duracionFade).SetLink(gameObject);
    }

    private void IniciarFrotado(float amplitud, float periodo)
    {
        frotado?.Kill();
        ReiniciarVisual();

        frotado = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(visual.DOLocalMove(new Vector3(amplitud, -amplitud * 0.3f, 0f), periodo * 0.5f).SetEase(Ease.InOutSine))
            .Join(visual.DOLocalRotate(new Vector3(0f, 0f, -anguloFrotado), periodo * 0.5f).SetEase(Ease.InOutSine))
            .Append(visual.DOLocalMove(new Vector3(-amplitud, amplitud * 0.3f, 0f), periodo).SetEase(Ease.InOutSine))
            .Join(visual.DOLocalRotate(new Vector3(0f, 0f, anguloFrotado), periodo).SetEase(Ease.InOutSine))
            .Append(visual.DOLocalMove(Vector3.zero, periodo * 0.5f).SetEase(Ease.InOutSine))
            .Join(visual.DOLocalRotate(Vector3.zero, periodo * 0.5f).SetEase(Ease.InOutSine))
            .SetLoops(-1);
    }

    private void CancelarDemostracion()
    {
        EnDemostracion = false;
        finDemostracion?.Kill();
    }

    private void ReiniciarVisual()
    {
        if (visual == null) return;
        visual.DOKill();
        visual.localPosition = Vector3.zero;
        visual.localRotation = Quaternion.identity;
    }
}
