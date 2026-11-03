using System.Collections;
using UnityEngine;
using UnityEngine.Audio;


public class MusicaCicloDia : MonoBehaviour
{
    [Header("Temas")]
    [SerializeField] private AudioClip musicaDia;
    [SerializeField] private AudioClip musicaTarde;
    [SerializeField] private AudioClip musicaNoche;

    [Header("Lluvia (opcional)")]
    [Tooltip("Sonido de lluvia en loop que se suma encima de la música mientras llueve.")]
    [SerializeField] private AudioClip sonidoLluvia;
    [SerializeField, Range(0f, 1f)] private float volumenLluvia = 0.5f;

    [Header("Mezcla")]
    [SerializeField, Range(0f, 1f)] private float volumenMusica = 0.7f;
    [SerializeField] private float duracionFundido = 2f;
    [Tooltip("Grupo 'Music' del AudioMixer, para que el toggle de música de AjusteVolumen también lo silencie.")]
    [SerializeField] private AudioMixerGroup grupoMusica;

    private AudioSource fuenteA;
    private AudioSource fuenteB;
    private AudioSource fuenteLluvia;
    private AudioSource activa;
    private Coroutine fundidoMusica;
    private Coroutine fundidoLluvia;
    private bool suscrito;

    private void Awake()
    {
        fuenteA = CrearFuente(true);
        fuenteB = CrearFuente(true);
        fuenteLluvia = CrearFuente(true);
        activa = fuenteA;
    }

    private AudioSource CrearFuente(bool loop)
    {
        AudioSource s = gameObject.AddComponent<AudioSource>();
        s.loop = loop;
        s.playOnAwake = false;
        s.volume = 0f;
        s.outputAudioMixerGroup = grupoMusica;
        s.ignoreListenerPause = true; 
        return s;
    }

    private void Start()
    {
        Suscribir();
    }

    private void OnEnable()
    {
        Suscribir();
    }

    private void OnDisable()
    {
        if (suscrito && TimeManager.Instance != null)
        {
            TimeManager.Instance.OnFaseHorariaCambiada -= CambiarTema;
            TimeManager.Instance.OnLluviaCambiada -= CambiarLluvia;
        }
        suscrito = false;
    }

    private void Suscribir()
    {
        if (suscrito || TimeManager.Instance == null) return;

        TimeManager.Instance.OnFaseHorariaCambiada += CambiarTema;
        TimeManager.Instance.OnLluviaCambiada += CambiarLluvia;
        suscrito = true;

        CambiarTema(TimeManager.Instance.FaseHoraria);
        CambiarLluvia(TimeManager.Instance.Lloviendo);
    }


    private void CambiarTema(FaseDia fase)
    {
        AudioClip clip = ClipPara(fase);
        if (clip == null || (activa.clip == clip && activa.isPlaying)) return;

        AudioSource saliente = activa;
        AudioSource entrante = activa == fuenteA ? fuenteB : fuenteA;

        entrante.clip = clip;
        entrante.volume = 0f;
        entrante.Play();
        activa = entrante;

        if (fundidoMusica != null) StopCoroutine(fundidoMusica);
        fundidoMusica = StartCoroutine(FundidoCruzado(saliente, entrante));
    }

    private IEnumerator FundidoCruzado(AudioSource saliente, AudioSource entrante)
    {
        float inicioSaliente = saliente.volume;
        float t = 0f;

        while (t < duracionFundido)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duracionFundido);
            entrante.volume = Mathf.Lerp(0f, volumenMusica, k);
            saliente.volume = Mathf.Lerp(inicioSaliente, 0f, k);
            yield return null;
        }

        entrante.volume = volumenMusica;
        saliente.Stop();
        saliente.volume = 0f;
        fundidoMusica = null;
    }

    private AudioClip ClipPara(FaseDia fase)
    {
        switch (fase)
        {
            case FaseDia.Tarde: return musicaTarde;
            case FaseDia.Noche: return musicaNoche;
            default: return musicaDia;
        }
    }


    private void CambiarLluvia(bool llueve)
    {
        if (sonidoLluvia == null) return;

        if (fundidoLluvia != null) StopCoroutine(fundidoLluvia);
        fundidoLluvia = StartCoroutine(FundirLluvia(llueve));
    }

    private IEnumerator FundirLluvia(bool llueve)
    {
        if (llueve && !fuenteLluvia.isPlaying)
        {
            fuenteLluvia.clip = sonidoLluvia;
            fuenteLluvia.Play();
        }

        float desde = fuenteLluvia.volume;
        float hasta = llueve ? volumenLluvia : 0f;
        float t = 0f;

        while (t < duracionFundido)
        {
            t += Time.unscaledDeltaTime;
            fuenteLluvia.volume = Mathf.Lerp(desde, hasta, Mathf.Clamp01(t / duracionFundido));
            yield return null;
        }

        fuenteLluvia.volume = hasta;
        if (!llueve) fuenteLluvia.Stop();
        fundidoLluvia = null;
    }
}