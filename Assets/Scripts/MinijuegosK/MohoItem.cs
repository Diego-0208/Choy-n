using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MohoItem : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private GestorMoho gestorAsociado;
    private Transform nodoAsociado;
    private RectTransform rectTransform;
    private Image imagenMoho;

    [SerializeField] private float duracionCrecimiento = 0.35f;

    [SerializeField] private float vidaMaxima = 100f;
    [SerializeField] private float danoPorRascado = 8f;
    [SerializeField] private bool regenerarSiAbandona = true;
    [SerializeField] private float velocidadRegeneracion = 15f;

    [Header("Sacudida al frotar")]
    [SerializeField] private float anguloSacudida = 10f;
    [SerializeField] private float duracionSacudida = 0.15f;

    private float vidaActual;
    private float ultimaVezRascado;
    private bool estaDestruido = false;
    private Tween crecimiento;
    private Tween sacudida;

    public bool EstaDestruido => estaDestruido;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        imagenMoho = GetComponent<Image>();
    }

    private void Start()
    {
        vidaActual = vidaMaxima;
        rectTransform.localScale = Vector3.zero;
        crecimiento = rectTransform.DOScale(Vector3.one, duracionCrecimiento)
            .SetEase(Ease.OutBack)
            .SetLink(gameObject);
    }

    private void Update()
    {
        if (estaDestruido) return;

        if (regenerarSiAbandona && Time.time - ultimaVezRascado > 0.5f && vidaActual < vidaMaxima)
        {
            vidaActual += velocidadRegeneracion * Time.deltaTime;
            vidaActual = Mathf.Min(vidaActual, vidaMaxima);
            ActualizarAspectoVisual();
        }
    }

    public void Inicializar(GestorMoho gestor, Transform nodo)
    {
        gestorAsociado = gestor;
        nodoAsociado = nodo;
    }

    public Transform ObtenerNodo()
    {
        return nodoAsociado;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AplicarRascado(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.delta.sqrMagnitude > 0.1f)
        {
            AplicarRascado(eventData);
        }
    }

    private void AplicarRascado(PointerEventData eventData)
    {
        if (estaDestruido) return;

        crecimiento?.Kill();

        ultimaVezRascado = Time.time;
        vidaActual -= danoPorRascado;
        GestorSonidos.Reproducir(Sonido.RascarMoho);

        if (gestorAsociado != null) gestorAsociado.NotificarRascado(eventData);

        ActualizarAspectoVisual();
        Sacudir();

        if (vidaActual <= 0f)
        {
            LimpiarMoho();
        }
    }

    private void Sacudir()
    {
        if (sacudida != null && sacudida.IsActive()) return;

        sacudida = rectTransform.DOShakeRotation(duracionSacudida, new Vector3(0f, 0f, anguloSacudida), 20, 90f, true)
            .SetLink(gameObject);
    }

    private void ActualizarAspectoVisual()
    {
        float porcentajeVida = Mathf.Clamp01(vidaActual / vidaMaxima);

        rectTransform.localScale = Vector3.one * porcentajeVida;

        if (imagenMoho != null)
        {
            Color colorActual = imagenMoho.color;
            colorActual.a = porcentajeVida;
            imagenMoho.color = colorActual;
        }
    }

    public void LimpiarMoho()
    {
        if (estaDestruido) return;
        estaDestruido = true;

        rectTransform.DOKill();

        if (gestorAsociado != null)
        {
            gestorAsociado.RegistrarMohoEliminado(this);
        }

        Destroy(gameObject);
    }
}
