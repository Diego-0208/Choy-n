using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MohoItem : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private GestorMoho gestorAsociado;
    private Transform nodoAsociado; 
    private RectTransform rectTransform;
    private Image imagenMoho;

    [SerializeField] private float velocidadCrecimiento = 5f;

    [SerializeField] private float vidaMaxima = 100f;
    [SerializeField] private float danoPorRascado = 8f;
    [SerializeField] private bool regenerarSiAbandona = true;
    [SerializeField] private float velocidadRegeneracion = 15f;

    private float vidaActual;
    private float ultimaVezRascado;
    private bool estaDestruido = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        imagenMoho = GetComponent<Image>();
    }

    private void Start()
    {
        vidaActual = vidaMaxima;
        rectTransform.localScale = Vector3.zero;
        StartCoroutine(CrecerRutina());
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

    private IEnumerator CrecerRutina()
    {
        while (rectTransform.localScale.x < 1f)
        {
            float escalaActual = Mathf.MoveTowards(rectTransform.localScale.x, 1f, velocidadCrecimiento * Time.deltaTime);
            rectTransform.localScale = new Vector3(escalaActual, escalaActual, 1f);
            yield return null;
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
        AplicarRascado();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.delta.sqrMagnitude > 0.1f)
        {
            AplicarRascado();
        }
    }

    private void AplicarRascado()
    {
        if (estaDestruido) return;

        ultimaVezRascado = Time.time;
        vidaActual -= danoPorRascado;

        ActualizarAspectoVisual();

        if (vidaActual <= 0f)
        {
            LimpiarMoho();
        }
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

        if (gestorAsociado != null)
        {
            gestorAsociado.RegistrarMohoEliminado(this);
        }

        Destroy(gameObject);
    }
}