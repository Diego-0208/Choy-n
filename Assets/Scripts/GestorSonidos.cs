using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Sonido
{
    Ninguno = 0,
    Click = 1,
    Cerrar = 2,
    TomarMaceta = 3,
    ColocarMaceta = 4,
    GuardarMaceta = 5,
    MovimientoInvalido = 6,
    Sembrar = 7,
    Trasplantar = 8,
    Comprar = 9,
    SinDinero = 10,
    Vender = 11,
    Desechar = 12,
    Cosechar = 13,
    FrutaAtrapada = 14,
    GotaAgua = 15,
    RiegoCorrecto = 16,
    GotaFertilizante = 17,
    GolpePlaga = 18,
    RascarMoho = 19,
    MohoEliminado = 20,
    PuntoTrasplante = 21,
    Error = 22,
    MinijuegoGanado = 23,
    MinijuegoPerdido = 24,
    PlantaCrece = 25,
    PlantaMuere = 26,
    RecompensaDiaria = 27,
    AbrirMinijuego = 28
}

[DefaultExecutionOrder(-90)]
public class GestorSonidos : MonoBehaviour
{
    [Serializable]
    public class EntradaSonido
    {
        public Sonido sonido;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volumen = 1f;
        [Range(0f, 0.5f)] public float variacionTono = 0.05f;
        public float tiempoMinimoEntreRepeticiones = 0.05f;
    }

    public static GestorSonidos Instance { get; private set; }

    [SerializeField] private EntradaSonido[] sonidos;
    [SerializeField] private AudioMixerGroup grupoSFX;
    [SerializeField] private int cantidadFuentes = 8;
    [SerializeField] private bool clickAutomaticoEnBotones = true;
    [SerializeField] private bool avisarSonidosFaltantes = true;

    private const string KEY_SFX = "EstadoSFX";

    private readonly Dictionary<Sonido, EntradaSonido> tabla = new Dictionary<Sonido, EntradaSonido>();
    private readonly Dictionary<Sonido, float> ultimaVez = new Dictionary<Sonido, float>();
    private readonly HashSet<Sonido> avisados = new HashSet<Sonido>();
    private readonly HashSet<int> botonesProcesados = new HashSet<int>();
    private AudioSource[] fuentes;
    private int siguienteFuente;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ConstruirTabla();
        CrearFuentes();

        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void Start()
    {
        if (Instance != this) return;
        if (clickAutomaticoEnBotones) AgregarClickABotones();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= AlCargarEscena;
            Instance = null;
        }
    }

    private void ConstruirTabla()
    {
        tabla.Clear();
        if (sonidos == null) return;

        foreach (EntradaSonido e in sonidos)
        {
            if (e == null || e.sonido == Sonido.Ninguno) continue;
            tabla[e.sonido] = e;
        }
    }

    private void CrearFuentes()
    {
        int n = Mathf.Max(1, cantidadFuentes);
        fuentes = new AudioSource[n];

        for (int i = 0; i < n; i++)
        {
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = false;
            s.outputAudioMixerGroup = grupoSFX;
            s.ignoreListenerPause = true;
            fuentes[i] = s;
        }
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (clickAutomaticoEnBotones) AgregarClickABotones();
    }

    public void AgregarClickABotones()
    {
        Button[] botones = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button b in botones)
        {
            if (b.GetComponent<SonidoBoton>() != null) continue;
            if (!botonesProcesados.Add(b.GetInstanceID())) continue;
            b.onClick.AddListener(ReproducirClick);
        }
    }

    private static void ReproducirClick()
    {
        Reproducir(Sonido.Click);
    }

    public static void Reproducir(Sonido sonido)
    {
        if (Instance != null) Instance.ReproducirInterno(sonido);
    }

    private void ReproducirInterno(Sonido sonido)
    {
        if (sonido == Sonido.Ninguno) return;
        if (grupoSFX == null && PlayerPrefs.GetInt(KEY_SFX, 1) == 0) return;

        if (!tabla.TryGetValue(sonido, out EntradaSonido entrada) || entrada.clips == null || entrada.clips.Length == 0)
        {
            AvisarFaltante(sonido);
            return;
        }

        float ahora = Time.unscaledTime;
        if (ultimaVez.TryGetValue(sonido, out float ultima) && ahora - ultima < entrada.tiempoMinimoEntreRepeticiones) return;

        AudioClip clip = entrada.clips[UnityEngine.Random.Range(0, entrada.clips.Length)];
        if (clip == null)
        {
            AvisarFaltante(sonido);
            return;
        }

        ultimaVez[sonido] = ahora;

        AudioSource fuente = fuentes[siguienteFuente];
        siguienteFuente = (siguienteFuente + 1) % fuentes.Length;

        fuente.pitch = 1f + UnityEngine.Random.Range(-entrada.variacionTono, entrada.variacionTono);
        fuente.PlayOneShot(clip, entrada.volumen);
    }

    private void AvisarFaltante(Sonido sonido)
    {
        if (!avisarSonidosFaltantes || !Debug.isDebugBuild) return;
        if (avisados.Add(sonido)) Debug.Log($"[Sonidos] Falta asignar un clip para '{sonido}'. Se ignora.");
    }
}