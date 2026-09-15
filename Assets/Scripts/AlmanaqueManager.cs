using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor.Search;
using Unity.VisualScripting;
using System.Collections.Generic;
public class AlmanaqueManager : MonoBehaviour
{
    public string nombre;
    [SerializeField] private Image ImgPlanta;
    [SerializeField] private TextMeshProUGUI NombrePlanta;
    [SerializeField] private TextMeshProUGUI DescripcionTexto;
    [SerializeField] private TextMeshProUGUI AlturaTexto;
    [SerializeField] private TextMeshProUGUI CuidadosText;
    public Sprite spritePlanta;
    private Almanaque almanaque;

    [SerializeField] private List<Almanaque> plantasAlmanaque;

    //[SerializeField] private Button botonPlanta;

    void Start()
    {
        //almanaque.LeerInformacion(NombrePlanta.text, DescripcionTexto.text, AlturaTexto.text, CuidadosText.text, ImgPlanta.sprite);
    }

    void Update()
    {
        
    }

    public void BotonPlanta(string nombre)
    {
        NombrePlanta.text = nombre;
    }


    public void BotonDetalles(string nombre)
    {
        
        NombrePlanta.text = almanaque.nombre;
        DescripcionTexto.text = almanaque.desc;
        AlturaTexto.text = almanaque.altura;
        CuidadosText.text = almanaque.cuidados;
        ImgPlanta.sprite = almanaque.img;
    }
}
