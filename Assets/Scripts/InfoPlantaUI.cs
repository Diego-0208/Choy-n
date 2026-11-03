using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class InfoPlantaUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoNombre;
    [SerializeField] private TextMeshProUGUI textoEtapa;
    [SerializeField] private TextMeshProUGUI textoSalud;
    [SerializeField] private string[] nombresEtapasPorDefecto = { "Retoño", "Joven", "Adulta" };
    [SerializeField] private float intervaloRefresco = 0.25f;

    private float temporizador;
    private readonly StringBuilder sb = new StringBuilder();

    private void OnEnable()
    {
        Refrescar();
    }

    private void Update()
    {
        temporizador += Time.unscaledDeltaTime;
        if (temporizador < intervaloRefresco) return;
        temporizador = 0f;
        Refrescar();
    }

    public void Refrescar()
    {
        CasillaUI casilla = MenuCuidadosUI.Instance != null ? MenuCuidadosUI.Instance.CasillaSeleccionada : null;
        MacetaUI maceta = casilla != null ? casilla.GetComponentInChildren<MacetaUI>() : null;
        PlantaBase planta = maceta != null && maceta.tienePlanta ? maceta.plantaActual : null;
        if (planta == null) return;

        GestorSaludPlanta salud = planta.GetComponent<GestorSaludPlanta>();

        if (textoNombre != null) textoNombre.text = planta.Nombre;
        if (textoEtapa != null) textoEtapa.text = TextoEtapa(planta);
        if (textoSalud != null) textoSalud.text = TextoSalud(planta, salud);
    }

    private string TextoEtapa(PlantaBase planta)
    {
        string etapa = planta.NombreEtapa(nombresEtapasPorDefecto);

        if (!planta.EstaViva) return etapa;
        if (planta.EsperandoTrasplante) return etapa + " · Necesita trasplante";
        if (planta.EsAdulta && planta.Permite(PlantaBase.Minijuego.Cosecha) && planta.PuedeCosechar)
            return etapa + " · Lista para cosechar";

        return etapa;
    }

    private string TextoSalud(PlantaBase planta, GestorSaludPlanta salud)
    {
        if (!planta.EstaViva) return "Muerta";
        if (salud == null || !salud.TieneAfecciones) return "Sana";

        sb.Clear();
        IReadOnlyList<TipoAfeccion> afecciones = salud.Afecciones;
        for (int i = 0; i < afecciones.Count; i++)
        {
            string nombre = NombreAfeccion(afecciones[i]);
            if (string.IsNullOrEmpty(nombre)) continue;
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(nombre);
        }
        return sb.Length > 0 ? sb.ToString() : "Sana";
    }

    private static string NombreAfeccion(TipoAfeccion afeccion)
    {
        switch (afeccion)
        {
            case TipoAfeccion.Sequia: return "Sequía";
            case TipoAfeccion.ExcesoDeAgua: return "Exceso de agua";
            case TipoAfeccion.Hongos: return "Hongos";
            case TipoAfeccion.PlagaMoscas: return "Plagas";
            case TipoAfeccion.Frio: return "Frío";
            case TipoAfeccion.HojasQuemadas: return "Hojas quemadas";
            default: return null;
        }
    }
}