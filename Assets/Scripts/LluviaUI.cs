using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(CanvasGroup))]
public class LluviaUI : MonoBehaviour
{
    [System.Serializable]
    public class Capa
    {
        public RawImage imagen;
        [Tooltip("Velocidad del desplazamiento (en repeticiones de la textura por segundo). Y negativo = cae.")]
        public Vector2 velocidad = new Vector2(-0.12f, -1.2f);
        [Tooltip("Cuántas veces se repite la textura a lo ancho y alto de la pantalla.")]
        public Vector2 repeticiones = new Vector2(2f, 3f);
    }

    [Tooltip("Normalmente 2 capas: una lejana (lenta, pequeña, transparente) y una cercana (rápida, grande).")]
    [SerializeField] private Capa[] capas;
    [SerializeField] private float duracionFundido = 1.5f;
    [SerializeField, Range(0f, 1f)] private float opacidadMaxima = 0.8f;

    private CanvasGroup grupo;
    private float objetivo;
    private bool suscrito;

    private void Awake()
    {
        grupo = GetComponent<CanvasGroup>();
        grupo.alpha = 0f;
        grupo.blocksRaycasts = false;
        grupo.interactable = false;

        foreach (Capa c in capas)
        {
            if (c.imagen == null) continue;
            c.imagen.raycastTarget = false;
            if (c.imagen.texture != null) c.imagen.texture.wrapMode = TextureWrapMode.Repeat;
            c.imagen.uvRect = new Rect(0f, 0f, c.repeticiones.x, c.repeticiones.y);
        }
    }

    private void Start() => Suscribir();
    private void OnEnable() => Suscribir();

    private void OnDisable()
    {
        if (suscrito && TimeManager.Instance != null) TimeManager.Instance.OnLluviaCambiada -= CambiarLluvia;
        suscrito = false;
    }

    private void Suscribir()
    {
        if (suscrito || TimeManager.Instance == null) return;
        TimeManager.Instance.OnLluviaCambiada += CambiarLluvia;
        suscrito = true;

        CambiarLluvia(TimeManager.Instance.Lloviendo);
        grupo.alpha = objetivo; 
    }

    private void CambiarLluvia(bool llueve)
    {
        objetivo = llueve ? opacidadMaxima : 0f;
    }

    private void Update()
    {
        if (!Mathf.Approximately(grupo.alpha, objetivo))
        {
            float paso = opacidadMaxima / Mathf.Max(0.01f, duracionFundido) * Time.unscaledDeltaTime;
            grupo.alpha = Mathf.MoveTowards(grupo.alpha, objetivo, paso);
        }

        if (grupo.alpha <= 0f) return;

        foreach (Capa c in capas)
        {
            if (c.imagen == null) continue;
            Rect uv = c.imagen.uvRect;
            uv.x = Mathf.Repeat(uv.x - c.velocidad.x * Time.unscaledDeltaTime, 1f);
            uv.y = Mathf.Repeat(uv.y - c.velocidad.y * Time.unscaledDeltaTime, 1f);
            uv.width = c.repeticiones.x;
            uv.height = c.repeticiones.y;
            c.imagen.uvRect = uv;
        }
    }
}