using System.Collections;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class FondoCicloDia : MonoBehaviour
{
    [Header("Sprites por fase")]
    [SerializeField] private Sprite spriteDia;
    [SerializeField] private Sprite spriteTarde;
    [SerializeField] private Sprite spriteNoche;
    [SerializeField] private Sprite spriteLluvia;

    [Header("Fundido (opcional)")]
    [Tooltip("Image hijo, del mismo tamaño, encima del fondo. Si está vacío, el cambio es instantáneo.")]
    [SerializeField] private Image imagenFundido;
    [SerializeField] private float duracionFundido = 1.5f;

    private Image imagen;
    private Coroutine fundido;
    private bool suscrito;

    private void Awake()
    {
        imagen = GetComponent<Image>();
        if (imagenFundido != null) imagenFundido.gameObject.SetActive(false);
    }

    private void Start()
    {
        Suscribir();
    }

    private void OnEnable()
    {
        if (TimeManager.Instance != null)
        {
            Suscribir();
            imagen.sprite = SpritePara(TimeManager.Instance.FaseActual);
        }
    }

    private void OnDisable()
    {
        if (suscrito && TimeManager.Instance != null)
        {
            TimeManager.Instance.OnFaseCambiada -= CambiarFase;
        }
        suscrito = false;
    }

    private void Suscribir()
    {
        if (suscrito || TimeManager.Instance == null) return;

        TimeManager.Instance.OnFaseCambiada += CambiarFase;
        suscrito = true;
        imagen.sprite = SpritePara(TimeManager.Instance.FaseActual);
    }

    private void CambiarFase(FaseDia fase)
    {
        Sprite nuevo = SpritePara(fase);
        if (nuevo == null || nuevo == imagen.sprite) return;

        if (imagenFundido == null || duracionFundido <= 0f || !isActiveAndEnabled)
        {
            imagen.sprite = nuevo;
            return;
        }

        if (fundido != null) StopCoroutine(fundido);
        fundido = StartCoroutine(Fundir(nuevo));
    }

    private IEnumerator Fundir(Sprite nuevo)
    {
        imagenFundido.sprite = nuevo;
        imagenFundido.gameObject.SetActive(true);

        Color c = imagenFundido.color;
        float t = 0f;
        while (t < duracionFundido)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Clamp01(t / duracionFundido);
            imagenFundido.color = c;
            yield return null;
        }

        imagen.sprite = nuevo;
        imagenFundido.gameObject.SetActive(false);
        c.a = 1f;
        imagenFundido.color = c;
        fundido = null;
    }

    private Sprite SpritePara(FaseDia fase)
    {
        switch (fase)
        {
            case FaseDia.Dia: return spriteDia;
            case FaseDia.Tarde: return spriteTarde;
            case FaseDia.Noche: return spriteNoche;
            case FaseDia.Lluvia: return spriteLluvia;
            default: return spriteDia;
        }
    }
}