using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MatamoscasUI : MonoBehaviour
{
    [Header("Dirección")]
    [SerializeField] private Vector2 direccionEntrada = new Vector2(1f, -1f);
    [SerializeField] private float variacionAngulo = 15f;
    [SerializeField] private float anguloMuneca = 35f;

    [Header("Tiempos")]
    [SerializeField] private float duracionEntrada = 0.07f;
    [SerializeField] private float duracionImpacto = 0.06f;
    [SerializeField] private float duracionPausa = 0.05f;
    [SerializeField] private float duracionSalida = 0.15f;

    [Header("Impacto")]
    [SerializeField] private Vector3 escalaImpacto = new Vector3(1.15f, 0.85f, 1f);

    private bool iniciado;

    private void OnEnable()
    {
        if (iniciado) return;
        iniciado = true;

        Destroy(gameObject, 2f);

        foreach (Graphic grafico in GetComponentsInChildren<Graphic>(true))
        {
            grafico.raycastTarget = false;
        }

        CanvasGroup grupo = GetComponent<CanvasGroup>();
        if (grupo != null) grupo.blocksRaycasts = false;

        DOVirtual.DelayedCall(0f, Animar).SetLink(gameObject);
    }

    private void Animar()
    {
        Transform t = transform;

        Vector2 direccion = Quaternion.Euler(0f, 0f, Random.Range(-variacionAngulo, variacionAngulo)) * direccionEntrada.normalized;
        float anguloBase = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg + 90f;
        float distancia = CalcularDistanciaFueraDePantalla();

        Vector3 destino = t.localPosition;
        Vector3 fuera = destino + (Vector3)(direccion * distancia);
        Vector3 escalaBase = t.localScale;

        t.localPosition = fuera;
        t.localRotation = Quaternion.Euler(0f, 0f, anguloBase - anguloMuneca);

        DOTween.Sequence()
            .SetLink(gameObject)
            .Append(t.DOLocalMove(destino, duracionEntrada).SetEase(Ease.InQuad))
            .Join(t.DOLocalRotate(new Vector3(0f, 0f, anguloBase), duracionEntrada).SetEase(Ease.InQuad))
            .Append(t.DOScale(Vector3.Scale(escalaBase, escalaImpacto), duracionImpacto * 0.5f).SetEase(Ease.OutQuad))
            .Append(t.DOScale(escalaBase, duracionImpacto * 0.5f))
            .AppendInterval(duracionPausa)
            .Append(t.DOLocalMove(fuera, duracionSalida).SetEase(Ease.InQuad))
            .Join(t.DOLocalRotate(new Vector3(0f, 0f, anguloBase - anguloMuneca), duracionSalida).SetEase(Ease.InQuad))
            .OnComplete(() => Destroy(gameObject));
    }

    private float CalcularDistanciaFueraDePantalla()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return 2000f;

        RectTransform raiz = (RectTransform)canvas.rootCanvas.transform;
        Vector3 escalaRaiz = raiz.lossyScale;
        Vector3 escalaPadre = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        float factor = escalaPadre.x != 0f ? escalaRaiz.x / escalaPadre.x : 1f;

        return (raiz.rect.width + raiz.rect.height) * factor;
    }
}
