using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PalitaUI : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Image imagen;
    [SerializeField] private Sprite spriteAbierta;
    [SerializeField] private Sprite spriteCerrada;

    [Header("Movimiento")]
    [SerializeField] private Vector2 desplazamientoEntrada = new Vector2(60f, 80f);
    [SerializeField] private float anguloEntrada = 30f;
    [SerializeField] private float anguloCorte = 0f;
    [SerializeField] private Vector2 desplazamientoSalida = new Vector2(80f, 60f);

    [Header("Cortes")]
    [SerializeField] private int cantidadCortes = 2;
    [SerializeField] private float tiempoAbierta = 0.07f;
    [SerializeField] private float tiempoCerrada = 0.08f;
    [SerializeField] private Vector3 escalaCorte = new Vector3(0.9f, 1.1f, 1f);
    [SerializeField] private float anguloSacudidaCorte = 6f;

    [Header("Tiempos")]
    [SerializeField] private float duracionEntrada = 0.12f;
    [SerializeField] private float duracionSalida = 0.18f;

    [Header("Recortes")]
    [SerializeField] private Image prefabTerron;
    [SerializeField] private int cantidadTerrones = 4;
    [SerializeField] private float fuerzaSalto = 60f;
    [SerializeField] private float distanciaTerrones = 70f;
    [SerializeField] private float duracionTerrones = 0.45f;

    private void Awake()
    {
        if (imagen == null) imagen = GetComponent<Image>();
    }

    private void Start()
    {
        RectTransform rect = (RectTransform)transform;

        foreach (Graphic grafico in GetComponentsInChildren<Graphic>())
        {
            grafico.raycastTarget = false;
        }

        CanvasGroup grupo = GetComponent<CanvasGroup>();
        if (grupo == null) grupo = gameObject.AddComponent<CanvasGroup>();
        grupo.blocksRaycasts = false;
        grupo.alpha = 0f;

        Vector3 destino = rect.localPosition;
        Vector3 escalaBase = rect.localScale;

        CambiarSprite(spriteAbierta);
        rect.localPosition = destino + (Vector3)desplazamientoEntrada;
        rect.localRotation = Quaternion.Euler(0f, 0f, anguloEntrada);

        Sequence secuencia = DOTween.Sequence().SetLink(gameObject);
        secuencia.Append(grupo.DOFade(1f, duracionEntrada * 0.5f));
        secuencia.Join(rect.DOLocalMove(destino, duracionEntrada).SetEase(Ease.OutQuad));
        secuencia.Join(rect.DOLocalRotate(new Vector3(0f, 0f, anguloCorte), duracionEntrada).SetEase(Ease.OutQuad));

        for (int i = 0; i < cantidadCortes; i++)
        {
            bool primero = i == 0;
            bool ultimo = i == cantidadCortes - 1;
            float giro = anguloCorte + (i % 2 == 0 ? -anguloSacudidaCorte : anguloSacudidaCorte);

            secuencia.AppendInterval(tiempoAbierta);
            secuencia.AppendCallback(() =>
            {
                CambiarSprite(spriteCerrada);
                if (primero) LanzarTerrones(destino);
            });
            secuencia.Append(rect.DOScale(Vector3.Scale(escalaBase, escalaCorte), tiempoCerrada * 0.4f).SetEase(Ease.OutQuad));
            secuencia.Join(rect.DOLocalRotate(new Vector3(0f, 0f, giro), tiempoCerrada * 0.4f));
            secuencia.Append(rect.DOScale(escalaBase, tiempoCerrada * 0.6f).SetEase(Ease.OutBack));
            secuencia.Join(rect.DOLocalRotate(new Vector3(0f, 0f, anguloCorte), tiempoCerrada * 0.6f));

            if (!ultimo)
            {
                secuencia.AppendCallback(() => CambiarSprite(spriteAbierta));
            }
        }

        secuencia.Append(rect.DOLocalMove(destino + (Vector3)desplazamientoSalida, duracionSalida).SetEase(Ease.InQuad));
        secuencia.Join(rect.DOLocalRotate(new Vector3(0f, 0f, anguloEntrada), duracionSalida));
        secuencia.Join(grupo.DOFade(0f, duracionSalida));
        secuencia.OnComplete(() => Destroy(gameObject));
    }

    private void CambiarSprite(Sprite sprite)
    {
        if (imagen != null && sprite != null) imagen.sprite = sprite;
    }

    private void LanzarTerrones(Vector3 origen)
    {
        if (prefabTerron == null) return;

        Transform padre = transform.parent;

        for (int i = 0; i < cantidadTerrones; i++)
        {
            Image terron = Instantiate(prefabTerron, padre);
            terron.raycastTarget = false;
            terron.transform.SetSiblingIndex(transform.GetSiblingIndex());

            RectTransform rectTerron = terron.rectTransform;
            rectTerron.localPosition = origen;
            rectTerron.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            Vector3 destino = origen + new Vector3(
                Random.Range(-distanciaTerrones, distanciaTerrones),
                Random.Range(-distanciaTerrones * 0.2f, distanciaTerrones * 0.3f),
                0f);

            float duracion = duracionTerrones * Random.Range(0.8f, 1.2f);

            DOTween.Sequence()
                .SetLink(terron.gameObject)
                .Append(rectTerron.DOLocalJump(destino, fuerzaSalto * Random.Range(0.7f, 1.3f), 1, duracion).SetEase(Ease.Linear))
                .Join(rectTerron.DOLocalRotate(new Vector3(0f, 0f, Random.Range(-360f, 360f)), duracion, RotateMode.LocalAxisAdd))
                .Insert(duracion * 0.6f, terron.DOFade(0f, duracion * 0.4f))
                .OnComplete(() => Destroy(terron.gameObject));
        }
    }
}