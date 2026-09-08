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

    private void OnEnable()
    {
        CargarAjustes();  
    }

    private void OnDisable()
    {
      
        RemoverListeners();
    }

    public void CargarAjustes() 
    {         
        RemoverListeners();

        
        float musicaVal = PlayerPrefs.GetFloat(MUSIC_KEY, 1f); 
        float fxVal = PlayerPrefs.GetFloat(FX_KEY, 1f); 
        bool isMuted = PlayerPrefs.GetInt(MUTE_KEY, 0) == 1; 

        
        if (sliderMusica != null) sliderMusica.value = musicaVal; 
        if (sliderFX != null) sliderFX.value = fxVal;
        if (toggleMute != null) toggleMute.isOn = isMuted;

        AplicarCambiosAudio();  

        
        AgregarListeners();
    }

    private void AgregarListeners()
    {
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
        PlayerPrefs.SetFloat(MUSIC_KEY, valor); 
        PlayerPrefs.Save(); 
        AplicarCambiosAudio();
    }

    public void SetVolumenFX(float valor)
    {
        PlayerPrefs.SetFloat(FX_KEY, valor);
        PlayerPrefs.Save(); 
        AplicarCambiosAudio(); 
    }

    public void SetMute(bool estaMuteado)
    {
        PlayerPrefs.SetInt(MUTE_KEY, estaMuteado ? 1 : 0);
        PlayerPrefs.Save();
        AplicarCambiosAudio();
    }  

    private void AplicarCambiosAudio()
    {
        if (toggleMute != null && toggleMute.isOn)
        {
            AudioListener.volume = 0f; 

            if (sliderMusica != null) sliderMusica.value = 0f;
            if (sliderFX != null) sliderFX.value = 0f;
        }
        else 
        {
            AudioListener.volume = 1f;

            if (sliderMusica != null) sliderMusica.value = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
            if (sliderFX != null) sliderFX.value = PlayerPrefs.GetFloat(FX_KEY, 1f);
        }
     
        AgregarListeners();
    }
}