using UnityEngine;

public class AjusteVolumen : MonoBehaviour
{
    [header("Referencia del UI volumen") ]
    [serializeField] private Slider sliderMusica;  
    [serializeField] private Slider sliderFX;
    [serializeField] private Toggle togleMute; 

    private const string  MUSIC_KEY  = "VolumenMusica"; 
    private const string FX_KEY  = "VolumenFX"; 
    private const string MUTE_KEY = "Muteado"; 
    
    private void Start()
    {
        CargarAjustes();  
    }

    public void CargarAjustes() 
    {
        float musicaVal = PlayerPrefs.GetFloat(MUSIC_KEY,1f); 
        float fxVAL = PlayerPrefs.GetFloat(FX_KEY,1f); 

        bool isMuted = PlayerPrefs.GetIn(MUTE_KEY,0) ==1; 

        sliderMusica.value = musicaVal; 
        sliderFX.value = fxVAL;
        toggleMute.isON = isMuted;

        AplicarCambiosAudio();  
    }

    public void SetVolumenMusica(float valor)
    {
        PlayerPrefs.SetFloat(MUSIC_KEY,valor); 
        PlayerPrefs.Save(); 
        AplicarCambiosAudio();
    }

    Public void SetVolumenFX(float valor)
    {
        PlayerPrefs.SetFloat(FX_KEY,valor)
        PlayerPrefs.Save(); 
        AplicarCambiosAudio; 
    }

    public void 
}
