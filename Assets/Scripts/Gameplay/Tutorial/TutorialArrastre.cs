using TMPro;
using UnityEngine;

public class TutorialArrastre : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoTutorial;

    [Header("Paso 1: envase de yogurt")]
    [SerializeField, TextArea] private string mensajeEnvase = "Arrastra el envase de yogurt hasta una casilla vacía";
    [SerializeField] private GameObject manoEnvase;

    [Header("Paso 2: tienda")]
    [SerializeField, TextArea] private string mensajeTienda = "Presiona la tienda para comprar semillas y plantar";
    [SerializeField] private GameObject manoTienda;

    [Header("Guardado")]
    [SerializeField] private bool mostrarSoloUnaVez = false;

    private const string ClavePaso = "TutorialPaso";
    private int paso;

    private void Start()
    {
        paso = mostrarSoloUnaVez ? PlayerPrefs.GetInt(ClavePaso, 0) : 0;
        MostrarPaso();
    }

    private void OnEnable()
    {
        MacetaItemUI.AlColocar += AlColocarMaceta;
        Shop_01.AlAbrirTienda += AlAbrirTienda;
    }

    private void OnDisable()
    {
        MacetaItemUI.AlColocar -= AlColocarMaceta;
        Shop_01.AlAbrirTienda -= AlAbrirTienda;
    }

    private void AlColocarMaceta(TamanoMaceta tamano)
    {
        if (paso != 0 || tamano != TamanoMaceta.Pequena) return;
        CambiarPaso(1);
    }

    private void AlAbrirTienda()
    {
        if (paso != 1) return;
        CambiarPaso(2);
    }

    private void CambiarPaso(int nuevoPaso)
    {
        paso = nuevoPaso;
        if (mostrarSoloUnaVez)
        {
            PlayerPrefs.SetInt(ClavePaso, paso);
            PlayerPrefs.Save();
        }
        MostrarPaso();
    }

    private void MostrarPaso()
    {
        if (manoEnvase != null) manoEnvase.SetActive(paso == 0);
        if (manoTienda != null) manoTienda.SetActive(paso == 1);

        if (textoTutorial == null) return;

        if (paso == 0) textoTutorial.text = mensajeEnvase;
        else if (paso == 1) textoTutorial.text = mensajeTienda;

        textoTutorial.gameObject.SetActive(paso < 2);
    }
}