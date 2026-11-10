using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ManoTutorialAnimacion : MonoBehaviour
{
    [SerializeField] private Vector2 direccion = new Vector2(1f, -1f);
    [SerializeField] private float distancia = 15f;
    [SerializeField] private float velocidad = 3f;
    [SerializeField] private bool pulsarEscala = true;
    [SerializeField] private float escalaExtra = 0.08f;

    private RectTransform rect;
    private Vector2 posicionInicial;
    private Vector3 escalaInicial;
    private bool inicializado;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        posicionInicial = rect.anchoredPosition;
        escalaInicial = rect.localScale;
        inicializado = true;
    }

    private void OnEnable()
    {
        if (!inicializado) return;
        rect.anchoredPosition = posicionInicial;
        rect.localScale = escalaInicial;
    }

    private void OnDisable()
    {
        if (!inicializado) return;
        rect.anchoredPosition = posicionInicial;
        rect.localScale = escalaInicial;
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.unscaledTime * velocidad) + 1f) * 0.5f;

        rect.anchoredPosition = posicionInicial + direccion.normalized * distancia * t;

        if (pulsarEscala)
            rect.localScale = escalaInicial * (1f + escalaExtra * (1f - t));
    }
}