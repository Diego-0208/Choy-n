using UnityEngine;
using UnityEngine.UI;

public class AjusteVolumen : MonoBehaviour
{
    [Header("Referencia del UI volumen")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderFX;
    [SerializeField] private Toggle toggleMute;

    private const string MUSIC_KEY = "VolumenMusica";
    private const string FX_KEY = "VolumenFX";
    private const string MUTE_KEY = "Muteado";

   
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

        float musicaVal = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float fxVal = PlayerPrefs.GetFloat(FX_KEY, 1f);
        bool isMuted = PlayerPrefs.GetInt(MUTE_KEY, 0) == 1;

        
        if (toggleMute != null) toggleMute.SetIsOnWithoutNotify(isMuted);

        if (isMuted)
        {
            if (sliderMusica != null) sliderMusica.SetValueWithoutNotify(0f);
            if (sliderFX != null) sliderFX.SetValueWithoutNotify(0f);
            AudioListener.volume = 0f;
        }
        else
        {
            if (sliderMusica != null) sliderMusica.SetValueWithoutNotify(musicaVal);
            if (sliderFX != null) sliderFX.SetValueWithoutNotify(fxVal);
            AudioListener.volume = 1f;
        }

        estaActualizandoUI = false; 
    }

    private void AgregarListeners()
    {
        RemoverListeners(); 
        if (sliderMusica != null) sliderMusica.onValueChanged.AddListener(SetVolumenMusica);
        if (sliderFX != null) sliderFX.onValueChanged.AddListener(SetVolumenFX);
        if (toggleMute != null) toggleMute.onValueChanged.AddListener(SetMute);
    }

    private void RemoverListeners()
    {
        if (sliderMusica != null) sliderMusica.onValueChanged.RemoveListener(SetVolumenMusica);
        if (sliderFX != null) sliderFX.onValueChanged.RemoveListener(SetVolumenFX);
        if (toggleMute != null) toggleMute.onValueChanged.RemoveListener(SetMute);
    }

    public void SetVolumenMusica(float valor)
    {
        if (estaActualizandoUI) return;

        PlayerPrefs.SetFloat(MUSIC_KEY, valor);
    }

    public void SetVolumenFX(float valor)
    {
        if (estaActualizandoUI) return;

        PlayerPrefs.SetFloat(FX_KEY, valor);
    }

    public void SetMute(bool estaMuteado)
    {
        if (estaActualizandoUI) return;

        PlayerPrefs.SetInt(MUTE_KEY, estaMuteado ? 1 : 0);

        estaActualizandoUI = true;

        if (estaMuteado)
        {
            AudioListener.volume = 0f;
            if (sliderMusica != null) sliderMusica.SetValueWithoutNotify(0f);
            if (sliderFX != null) sliderFX.SetValueWithoutNotify(0f);
        }
        else
        {
            AudioListener.volume = 1f;
            float musicaVal = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
            float fxVal = PlayerPrefs.GetFloat(FX_KEY, 1f);

            if (sliderMusica != null) sliderMusica.SetValueWithoutNotify(musicaVal);
            if (sliderFX != null) sliderFX.SetValueWithoutNotify(fxVal);
        }

        estaActualizandoUI = false;
    }
}