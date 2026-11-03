using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AjusteVolumen : MonoBehaviour
{
    
    [SerializeField] private float dbMusicaActiva = -10f;
    [SerializeField] private float dbSFXActivo = -10f;
    [Header("UI References")]
    [SerializeField] private Toggle toggleMusica;
    [SerializeField] private Toggle toggleSFX;

    [Header("Audio Mixer (Opcional pero recomendado)")]
    
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string parametroMusica = "VolumenMusica";
    [SerializeField] private string parametroSFX = "VolumenSFX";

    private const string MUSIC_KEY = "EstadoMusica";
    private const string SFX_KEY = "EstadoSFX";

    private bool estaActualizandoUI = false;

    private void OnEnable()
    {
        AgregarListeners();
        CargarAjustes();
    }

    private void OnDisable()
    {
        RemoverListeners();
        PlayerPrefs.Save();
    }

    public void CargarAjustes()
    {
        estaActualizandoUI = true;

        // Por defecto activados (1 = activado, 0 = desactivado)
        bool musicaActiva = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        bool sfxActivo = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;

        if (toggleMusica != null) toggleMusica.SetIsOnWithoutNotify(musicaActiva);
        if (toggleSFX != null) toggleSFX.SetIsOnWithoutNotify(sfxActivo);

        AplicarEstadoAudio(parametroMusica, musicaActiva);
        AplicarEstadoAudio(parametroSFX, sfxActivo);

        estaActualizandoUI = false;
    }

    private void AgregarListeners()
    {
        RemoverListeners();
        if (toggleMusica != null) toggleMusica.onValueChanged.AddListener(SetMusica);
        if (toggleSFX != null) toggleSFX.onValueChanged.AddListener(SetSFX);
    }

    private void RemoverListeners()
    {
        if (toggleMusica != null) toggleMusica.onValueChanged.RemoveListener(SetMusica);
        if (toggleSFX != null) toggleSFX.onValueChanged.RemoveListener(SetSFX);
    }

    public void SetMusica(bool estado)
    {
        if (estaActualizandoUI) return;

        PlayerPrefs.SetInt(MUSIC_KEY, estado ? 1 : 0);
        AplicarEstadoAudio(parametroMusica, estado);
    }

    public void SetSFX(bool estado)
    {
        if (estaActualizandoUI) return;

        PlayerPrefs.SetInt(SFX_KEY, estado ? 1 : 0);
        AplicarEstadoAudio(parametroSFX, estado);
    }

    private void AplicarEstadoAudio(string parametroMixer, bool estaActivado)
    {
        if (audioMixer != null)
        {
            float dbActivo = parametroMixer == parametroMusica ? dbMusicaActiva : dbSFXActivo;
            float volumenEnDb = estaActivado ? dbActivo : -80f;
            audioMixer.SetFloat(parametroMixer, volumenEnDb);
        }
        else if (parametroMixer == parametroMusica)
        {
            AudioListener.volume = estaActivado ? 1f : 0f;
        }
    }
}