using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GestorMinijuegosPrincipal : MonoBehaviour
{
    [SerializeField] private GameObject panelMinijuegos;
    [SerializeField] private GestorMoho gestorMoho;
    [SerializeField] private GestorPlagas gestorPlagas;
    [SerializeField] private MinijuegoTrasplanteUI gestorTrasplante;
    [SerializeField] private float retrasoCierre = 0.7f;

    private Coroutine cierreProgramado;

    private void Awake()
    {
        gestorMoho.Terminado += TerminarConRetraso;
        gestorPlagas.Terminado += TerminarConRetraso;
        gestorTrasplante.Terminado += Cerrar;

        Cerrar();
    }

    private void OnDestroy()
    {
        if (gestorMoho != null) gestorMoho.Terminado -= TerminarConRetraso;
        if (gestorPlagas != null) gestorPlagas.Terminado -= TerminarConRetraso;
        if (gestorTrasplante != null) gestorTrasplante.Terminado -= Cerrar;
    }

    public void AbrirMoho()
    {
        GestorSaludPlanta planta = ObtenerPlantaSeleccionada();

        if (planta == null)
        {
            Debug.Log("No hay una planta en la casilla seleccionada.");
            return;
        }

        if (planta.CantidadMoho <= 0)
        {
            Debug.Log("Esta planta no tiene moho.");
            return;
        }

        OcultarMenuCuidados();
        Preparar(gestorMoho.gameObject);
        AplicarImagenPlanta(gestorMoho.gameObject, planta.gameObject);
        gestorMoho.IniciarMinijuego(planta);
    }

    public void AbrirPlagas()
    {
        GestorSaludPlanta planta = ObtenerPlantaSeleccionada();

        if (planta == null)
        {
            Debug.Log("No hay una planta en la casilla seleccionada.");
            return;
        }

        if (planta.CantidadPlagas <= 0)
        {
            Debug.Log("Esta planta no tiene plagas.");
            return;
        }

        OcultarMenuCuidados();
        Preparar(gestorPlagas.gameObject);
        AplicarImagenPlanta(gestorPlagas.gameObject, planta.gameObject);
        gestorPlagas.IniciarMinijuego(planta);
    }

    public void AbrirTrasplante()
    {
        CasillaUI casilla = ObtenerCasillaSeleccionada();
        MacetaUI maceta = casilla != null ? casilla.GetComponentInChildren<MacetaUI>() : null;

        if (maceta == null || !maceta.tienePlanta)
        {
            Debug.Log("No hay una planta para trasplantar en la casilla seleccionada.");
            return;
        }

        OcultarMenuCuidados();
        Preparar(gestorTrasplante.gameObject);

        if (maceta.plantaActual != null)
        {
            AplicarImagenPlanta(gestorTrasplante.gameObject, maceta.plantaActual.gameObject);
        }

        gestorTrasplante.Iniciar(exito =>
        {
            if (exito && maceta != null) maceta.HabilitarTrasplante();
        });
    }

    public void Cerrar()
    {
        CancelarCierreProgramado();
        DesactivarGestores();
        panelMinijuegos.SetActive(false);
    }

    private void AplicarImagenPlanta(GameObject raizMinijuego, GameObject planta)
    {
        Image imagenPlanta = planta != null ? planta.GetComponent<Image>() : null;
        if (imagenPlanta == null || imagenPlanta.sprite == null) return;

        ImagenPlantaMinijuego[] destinos = raizMinijuego.GetComponentsInChildren<ImagenPlantaMinijuego>(true);
        foreach (ImagenPlantaMinijuego destino in destinos)
        {
            destino.Aplicar(imagenPlanta.sprite);
        }
    }

    private void TerminarConRetraso()
    {
        CancelarCierreProgramado();
        cierreProgramado = StartCoroutine(CerrarTrasRetraso());
    }

    private IEnumerator CerrarTrasRetraso()
    {
        yield return new WaitForSeconds(retrasoCierre);

        cierreProgramado = null;
        Cerrar();

        GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
        if (grilla != null) grilla.GuardarEstadoGrilla();
    }

    private void CancelarCierreProgramado()
    {
        if (cierreProgramado != null)
        {
            StopCoroutine(cierreProgramado);
            cierreProgramado = null;
        }
    }

    private void Preparar(GameObject gestorAActivar)
    {
        CancelarCierreProgramado();
        DesactivarGestores();
        panelMinijuegos.SetActive(true);
        gestorAActivar.SetActive(true);
    }

    private void DesactivarGestores()
    {
        gestorMoho.gameObject.SetActive(false);
        gestorPlagas.gameObject.SetActive(false);
        gestorTrasplante.gameObject.SetActive(false);
    }

    private CasillaUI ObtenerCasillaSeleccionada()
    {
        return MenuCuidadosUI.Instance != null ? MenuCuidadosUI.Instance.CasillaSeleccionada : null;
    }

    private GestorSaludPlanta ObtenerPlantaSeleccionada()
    {
        CasillaUI casilla = ObtenerCasillaSeleccionada();
        return casilla != null ? casilla.GetComponentInChildren<GestorSaludPlanta>() : null;
    }

    private void OcultarMenuCuidados()
    {
        if (MenuCuidadosUI.Instance != null) MenuCuidadosUI.Instance.OcultarPanel();
    }
}