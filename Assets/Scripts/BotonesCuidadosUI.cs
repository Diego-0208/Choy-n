using UnityEngine;

public class BotonesCuidadosUI : MonoBehaviour
{
    [Header("Botones (arrastra el GameObject de cada botón)")]
    [SerializeField] private GameObject botonRiego;
    [SerializeField] private GameObject botonFertilizante;
    [SerializeField] private GameObject botonCosecha;
    [SerializeField] private GameObject botonTrasplante;
    [SerializeField] private GameObject botonMoho;
    [SerializeField] private GameObject botonPlagas;
    [SerializeField] private GameObject botonBasura;
    [Tooltip("El botón de vender (ZonaVenta). Solo aparece si la planta es adulta y está sana.")]
    [SerializeField] private GameObject botonVenta;

    [Tooltip("Cada cuántos segundos se revisan los botones mientras el panel está abierto.")]
    [SerializeField] private float intervaloRefresco = 0.25f;

    private float temporizador;

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
        GestorSaludPlanta salud = planta != null ? planta.GetComponent<GestorSaludPlanta>() : null;

        bool hayPlanta = planta != null;
        bool viva = hayPlanta && planta.EstaViva;

        Mostrar(botonTrasplante, viva && planta.NecesitaTrasplante && !maceta.PuedeTrasplantar);

        Mostrar(botonRiego, viva && planta.Permite(PlantaBase.Minijuego.Riego)
                            && salud != null && !salud.TieneAfeccion(TipoAfeccion.ExcesoDeAgua));

        Mostrar(botonFertilizante, viva && planta.Permite(PlantaBase.Minijuego.Fertilizante) && !planta.EsAdulta);

        Mostrar(botonCosecha, viva && planta.Permite(PlantaBase.Minijuego.Cosecha) && planta.PuedeCosechar);

        Mostrar(botonMoho, viva && planta.Permite(PlantaBase.Minijuego.Moho)
                           && salud != null && salud.CantidadMoho > 0);

        Mostrar(botonPlagas, viva && planta.Permite(PlantaBase.Minijuego.Plagas)
                             && salud != null && salud.CantidadPlagas > 0);

        Mostrar(botonBasura, hayPlanta && !viva);
        Mostrar(botonVenta, ZonaVentaUI.SePuedeVender(planta)); 
    }

    private static void Mostrar(GameObject boton, bool visible)
    {
        if (boton != null && boton.activeSelf != visible) boton.SetActive(visible);
    }
}