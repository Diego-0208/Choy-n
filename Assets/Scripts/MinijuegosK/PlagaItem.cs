using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlagaItem : MonoBehaviour, IPointerClickHandler
{
    private GestorPlagas spawnerAsociado;
    private RectTransform rectTransform;
    private CanvasGroup grupo;

    private Vector2 posicionInicial;
    private Vector3 escalaBase;
    private float tiempoAleatorioX;
    private float tiempoAleatorioY;
    private bool muerta;

    [Header("Configuración de Vuelo")]
    [SerializeField] private float velocidadMovimiento = 3f;
    [SerializeField] private float amplitudMovimiento = 15f;

    [Header("Aparición")]
    [SerializeField] private float duracionAparecer = 0.3f;

    [Header("Caída")]
    [SerializeField] private float retrasoGolpe = 0.1f;
    [SerializeField] private float duracionAplastar = 0.06f;
    [SerializeField] private float duracionVolteo = 0.15f;
    [SerializeField] private float alturaRebote = 20f;
    [SerializeField] private float desvioHorizontal = 30f;
    [SerializeField] private float velocidadCaida = 900f;
    [SerializeField] private float tiempoEnPiso = 0.5f;
    [SerializeField] private float duracionDesvanecer = 0.3f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        grupo = GetComponent<CanvasGroup>();
        if (grupo == null) grupo = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        posicionInicial = rectTransform.anchoredPosition;

        tiempoAleatorioX = Random.Range(0f, 100f);
        tiempoAleatorioY = Random.Range(0f, 100f);

        escalaBase = rectTransform.localScale;
        rectTransform.localScale = Vector3.zero;
        rectTransform.DOScale(escalaBase, duracionAparecer).SetEase(Ease.OutBack).SetLink(gameObject);
    }

    private void Update()
    {
        if (muerta) return;

        float offsetX = Mathf.Sin(Time.time * velocidadMovimiento + tiempoAleatorioX) * amplitudMovimiento;
        float offsetY = Mathf.Cos(Time.time * velocidadMovimiento * 1.2f + tiempoAleatorioY) * amplitudMovimiento;

        rectTransform.anchoredPosition = posicionInicial + new Vector2(offsetX, offsetY);
    }

    public void Inicializar(GestorPlagas spawner)
    {
        spawnerAsociado = spawner;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        DestruirPlaga();
    }

    public void DestruirPlaga()
    {
        if (muerta) return;
        muerta = true;

        GestorSonidos.Reproducir(Sonido.GolpePlaga);
        grupo.blocksRaycasts = false;

        if (spawnerAsociado == null)
        {
            Destroy(gameObject);
            return;
        }

        float pisoMundo = spawnerAsociado.ObtenerPisoMundo();
        transform.SetParent(spawnerAsociado.ContenedorEfectos, true);

        spawnerAsociado.RegistrarPlagaEliminada(transform.position);

        AnimarCaida(pisoMundo);
    }

    private void AnimarCaida(float pisoMundo)
    {
        rectTransform.DOKill();

        Animator animador = GetComponent<Animator>();
        if (animador != null) animador.enabled = false;

        Transform padre = rectTransform.parent;
        Vector3 local = rectTransform.localPosition;
        Vector3 puntoPiso = new Vector3(rectTransform.position.x, pisoMundo, rectTransform.position.z);

        float mitadAlto = rectTransform.rect.height * 0.5f * Mathf.Abs(escalaBase.y);
        float yPiso = Mathf.Min(padre.InverseTransformPoint(puntoPiso).y + mitadAlto, local.y);
        float yRebote = local.y + alturaRebote;
        float duracionCaida = Mathf.Max(0.15f, (yRebote - yPiso) / velocidadCaida);
        float xFinal = local.x + Random.Range(-desvioHorizontal, desvioHorizontal);
        float giro = (180f + Random.Range(-30f, 30f)) * (Random.value < 0.5f ? 1f : -1f);
        float inicioGiro = retrasoGolpe + duracionAplastar;

        Vector3 aplastada = new Vector3(escalaBase.x * 1.35f, escalaBase.y * 0.6f, escalaBase.z);

        Sequence secuencia = DOTween.Sequence().SetLink(gameObject);
        secuencia.AppendInterval(retrasoGolpe);
        secuencia.Append(rectTransform.DOScale(aplastada, duracionAplastar).SetEase(Ease.OutQuad));
        secuencia.Append(rectTransform.DOScale(escalaBase, duracionVolteo).SetEase(Ease.OutBack));
        secuencia.Join(rectTransform.DOLocalMoveY(yRebote, duracionVolteo).SetEase(Ease.OutQuad));
        secuencia.Append(rectTransform.DOLocalMoveY(yPiso, duracionCaida).SetEase(Ease.OutBounce));
        secuencia.Insert(inicioGiro, rectTransform.DOLocalRotate(new Vector3(0f, 0f, giro), duracionVolteo + duracionCaida * 0.7f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad));
        secuencia.Insert(inicioGiro, rectTransform.DOLocalMoveX(xFinal, duracionVolteo + duracionCaida).SetEase(Ease.OutSine));
        secuencia.AppendInterval(tiempoEnPiso);
        secuencia.Append(grupo.DOFade(0f, duracionDesvanecer));
        secuencia.OnComplete(() => Destroy(gameObject));
    }
}
