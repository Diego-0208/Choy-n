using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class AlmanaqueManager : MonoBehaviour
{
    [SerializeField] private List<Almanaque> plantasAlmanaque;
    [SerializeField] private List<TarjetaAlmanaque> tarjetasUI; // Lista de las 11 tarjetas en escena

    [Header("Paneles")]
    [SerializeField] private GameObject panelAlmanaquePrincipal;
    [SerializeField] private GameObject panelDetalles;
    [SerializeField] private GameObject LayoutDetalles;
    [SerializeField] private ScrollRect scrollRectDetalles;

    [Header("Detalles UI")]
    [SerializeField] private Image ImgPlanta;
    [SerializeField] private TextMeshProUGUI NombrePlanta;
    [SerializeField] private TextMeshProUGUI DescripcionTexto;
    [SerializeField] private TextMeshProUGUI AlturaTexto;
    [SerializeField] private TextMeshProUGUI CuidadosText;

    private Almanaque plantaSeleccionada;
    private RectTransform layoutRectCacheado;

    void Start()
    {
        if (panelDetalles != null)
            panelDetalles.SetActive(false);

        InicializarTarjetas();
    }

    private void InicializarTarjetas()
    {
        foreach (var tarjeta in tarjetasUI)
        {
            if (tarjeta != null)
            {
                tarjeta.Inicializar(this);
            }
        }
    }

    public void AbrirAlmanaque()
    {
        if (panelAlmanaquePrincipal != null)
            panelAlmanaquePrincipal.SetActive(true);

        // Refrescar el estado de desbloqueo de todas las tarjetas
        foreach (var tarjeta in tarjetasUI)
        {
            if (tarjeta != null) tarjeta.ActualizarEstado();
        }

        Time.timeScale = 0f; // Pausa el juego
    }

    public void CerrarAlmanaque()
    {
        if (panelDetalles != null)
            panelDetalles.SetActive(false);

        if (panelAlmanaquePrincipal != null)
            panelAlmanaquePrincipal.SetActive(false);

        Time.timeScale = 1f; // Reanuda el juego
    }

    public bool EstaPlantaDesbloqueada(Almanaque planta)
    {
        if (planta == null) return false;

        string clave = "Desbloqueada_" + planta.idPlanta.Trim();
        int estado = PlayerPrefs.GetInt(clave, 0);

        Debug.Log($"Buscando clave PlayerPrefs: '{clave}' | Valor encontrado: {estado}");

        return estado == 1;
    }

    public void BotonPlanta(Almanaque almanaque)
    {
        plantaSeleccionada = almanaque;
    }

    public void BotonDetalles()
    {
        if (plantaSeleccionada == null) return;
        if (!EstaPlantaDesbloqueada(plantaSeleccionada)) return;

        if (NombrePlanta != null) NombrePlanta.text = plantaSeleccionada.nombre;
        if (DescripcionTexto != null) DescripcionTexto.text = plantaSeleccionada.desc;
        if (AlturaTexto != null) AlturaTexto.text = plantaSeleccionada.altura;
        if (CuidadosText != null) CuidadosText.text = plantaSeleccionada.cuidados;
        if (ImgPlanta != null) ImgPlanta.sprite = plantaSeleccionada.img;

        if (panelDetalles != null)
            panelDetalles.SetActive(true);

        StartCoroutine(ReajustarLayout());
    }

    public void BotonVolver()
    {
        if (scrollRectDetalles != null)
            scrollRectDetalles.verticalNormalizedPosition = 1f;

        if (panelDetalles != null)
            panelDetalles.SetActive(false);
    }

    private RectTransform ObtenerLayout()
    {
        if (layoutRectCacheado != null)
            return layoutRectCacheado;

        if (LayoutDetalles != null)
        {
            layoutRectCacheado = LayoutDetalles.GetComponent<RectTransform>();
            if (layoutRectCacheado != null)
                return layoutRectCacheado;
        }

        if (panelDetalles != null)
        {
            Transform t = panelDetalles.transform.Find("RectMask/Layout");
            if (t != null)
                layoutRectCacheado = t.GetComponent<RectTransform>();
        }

        return layoutRectCacheado;
    }

    private System.Collections.IEnumerator ReajustarLayout()
    {
        yield return null;
        yield return null;

        Canvas.ForceUpdateCanvases();

        RectTransform layout = ObtenerLayout();
        if (layout == null) yield break;

        LayoutRebuilder.ForceRebuildLayoutImmediate(layout);

        foreach (Transform hijo in layout)
        {
            RectTransform rt = hijo.GetComponent<RectTransform>();
            if (rt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

            foreach (Transform nieto in hijo)
            {
                RectTransform rt2 = nieto.GetComponent<RectTransform>();
                if (rt2 != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rt2);
            }
        }

        Canvas.ForceUpdateCanvases();

        if (scrollRectDetalles != null)
            scrollRectDetalles.verticalNormalizedPosition = 1f;
    }
    private void OnEnable()
    {
        if (tarjetasUI == null) return;

        foreach (var tarjeta in tarjetasUI)
        {
            if (tarjeta != null)
            {
                tarjeta.ActualizarEstado();
            }
        }
    }

    public void RefrescarEstadoTarjetas()
    {
        if (tarjetasUI == null) return;

        foreach (var tarjeta in tarjetasUI)
        {
            if (tarjeta != null)
            {
                tarjeta.ActualizarEstado();
            }
        }
    }
}