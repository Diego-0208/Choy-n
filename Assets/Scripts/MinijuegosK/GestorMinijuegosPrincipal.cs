using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GestorMinijuegosPrincipal : MonoBehaviour
{
    [SerializeField] private GameObject panelMinijuegos;
    [SerializeField] private GestorMoho gestorMoho;
    [SerializeField] private GestorPlagas gestorPlagas;
    [SerializeField] private MinijuegoTrasplanteUI gestorTrasplante;
    [SerializeField] private Regadera gestorRiego;
    [SerializeField] private ControlFertilizante gestorFertilizante;
    [SerializeField] private GeneradorFruta gestorCosecha;
    [SerializeField] private float retrasoCierre = 0.7f;

    [Header("Riego")]
    [SerializeField] private float nivelAguaRiegoCorrecto = 70f;
    [SerializeField] private float aguaRiegoInsuficiente = 10f;
    [SerializeField] private float nivelAguaRiegoExceso = 95f;

    [Header("Fertilizante")]
    [SerializeField] private float segundosCrecimientoFertilizante = 10f;

    [Header("Cosecha")]
    [SerializeField] private int monedasPorFruta = 10;

    private Coroutine cierreProgramado;

    private GameObject raizRiego;
    private GameObject raizFertilizante;
    private GameObject raizCosecha;

    public bool TieneMinijuegoRiego => gestorRiego != null;
    public bool TieneMinijuegoFertilizante => gestorFertilizante != null;
    public bool TieneMinijuegoCosecha => gestorCosecha != null;

    private void Awake()
    {
        gestorMoho.Terminado += TerminarConRetraso;
        gestorPlagas.Terminado += TerminarConRetraso;
        gestorTrasplante.Terminado += Cerrar;

        if (gestorRiego != null) gestorRiego.Terminado += TerminarConRetraso;
        if (gestorFertilizante != null) gestorFertilizante.Terminado += TerminarConRetraso;
        if (gestorCosecha != null) gestorCosecha.Terminado += TerminarConRetraso;

        if (gestorRiego != null) raizRiego = ObtenerRaiz(gestorRiego, gestorRiego.Raiz, "riego");
        if (gestorFertilizante != null) raizFertilizante = ObtenerRaiz(gestorFertilizante, gestorFertilizante.Raiz, "fertilizante");
        if (gestorCosecha != null) raizCosecha = ObtenerRaiz(gestorCosecha, gestorCosecha.Raiz, "cosecha");

        AvisarGestoresSinAsignar();

        Cerrar();
    }

    private void OnDestroy()
    {
        if (gestorMoho != null) gestorMoho.Terminado -= TerminarConRetraso;
        if (gestorPlagas != null) gestorPlagas.Terminado -= TerminarConRetraso;
        if (gestorTrasplante != null) gestorTrasplante.Terminado -= Cerrar;

        if (gestorRiego != null) gestorRiego.Terminado -= TerminarConRetraso;
        if (gestorFertilizante != null) gestorFertilizante.Terminado -= TerminarConRetraso;
        if (gestorCosecha != null) gestorCosecha.Terminado -= TerminarConRetraso;
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

    public void AbrirRiego()
    {
        if (gestorRiego == null)
        {
            Debug.LogWarning("Falta asignar el minijuego de riego (Regadera) en GestorMinijuegosPrincipal.");
            return;
        }

        GestorSaludPlanta planta = ObtenerPlantaSeleccionada();

        if (planta == null)
        {
            Debug.Log("No hay una planta en la casilla seleccionada.");
            return;
        }

        PlantaBase plantaBase = planta.GetComponent<PlantaBase>();
        if (plantaBase != null && !plantaBase.EstaViva)
        {
            Debug.Log("Esta planta está muerta, no se puede regar.");
            return;
        }

        if (planta.TieneAfeccion(TipoAfeccion.ExcesoDeAgua))
        {
            Debug.Log("Esta planta ya tiene demasiada agua.");
            return;
        }

        OcultarMenuCuidados();
        Preparar(raizRiego);
        AplicarImagenPlanta(raizRiego, planta.gameObject);

        gestorRiego.Iniciar(resultado => AplicarRiego(planta, resultado));
    }

    public void AbrirFertilizante()
    {
        if (gestorFertilizante == null)
        {
            Debug.LogWarning("Falta asignar el minijuego de fertilizante (ControlFertilizante) en GestorMinijuegosPrincipal.");
            return;
        }

        GestorSaludPlanta planta = ObtenerPlantaSeleccionada();

        if (planta == null)
        {
            Debug.Log("No hay una planta en la casilla seleccionada.");
            return;
        }

        PlantaBase plantaBase = planta.GetComponent<PlantaBase>();

        if (plantaBase == null || !plantaBase.EstaViva)
        {
            Debug.Log("Esta planta no está viva, no se puede fertilizar.");
            return;
        }

        if (plantaBase.EsAdulta)
        {
            Debug.Log(plantaBase.Nombre + " ya está completamente adulta.");
            return;
        }

        OcultarMenuCuidados();
        Preparar(raizFertilizante);
        AplicarImagenPlanta(raizFertilizante, planta.gameObject);

        gestorFertilizante.Iniciar(() => AplicarFertilizante(planta));
    }

    public void AbrirCosecha()
    {
        if (gestorCosecha == null)
        {
            Debug.LogWarning("Falta asignar el minijuego de cosecha (GeneradorFruta) en GestorMinijuegosPrincipal.");
            return;
        }

        GestorSaludPlanta planta = ObtenerPlantaSeleccionada();

        if (planta == null)
        {
            Debug.Log("No hay una planta en la casilla seleccionada.");
            return;
        }

        PlantaBase plantaBase = planta.GetComponent<PlantaBase>();

        if (plantaBase == null || !plantaBase.EstaViva)
        {
            Debug.Log("Esta planta no está viva, no se puede cosechar.");
            return;
        }

        if (!plantaBase.EsAdulta)
        {
            Debug.Log("Esta planta todavía no está lista para cosechar.");
            return;
        }

        if (!plantaBase.PuedeCosechar)
        {
            Debug.Log("Esta planta ya fue cosechada. Vuelve en " + Mathf.CeilToInt(plantaBase.TemporizadorCosecha) + " s.");
            return;
        }

        OcultarMenuCuidados();
        Preparar(raizCosecha);
        AplicarImagenPlanta(raizCosecha, planta.gameObject);

        gestorCosecha.Iniciar(frutas => AplicarCosecha(planta, frutas));
    }

    public void Cerrar()
    {
        CancelarCierreProgramado();
        DesactivarGestores();
        panelMinijuegos.SetActive(false);
    }


    private void AplicarRiego(GestorSaludPlanta planta, ResultadoRiego resultado)
    {
        if (planta == null) return;

        switch (resultado)
        {
            case ResultadoRiego.Correcto:
                SubirAguaHasta(planta, nivelAguaRiegoCorrecto);
                break;
            case ResultadoRiego.Insuficiente:
                planta.Regar(aguaRiegoInsuficiente);
                break;
            case ResultadoRiego.Exceso:
                SubirAguaHasta(planta, nivelAguaRiegoExceso);
                break;
        }
    }

    private void SubirAguaHasta(GestorSaludPlanta planta, float nivelObjetivo)
    {
        float falta = nivelObjetivo - planta.NivelAgua;
        if (falta > 0f) planta.Regar(falta);
    }

    private void AplicarFertilizante(GestorSaludPlanta planta)
    {
        if (planta == null) return;

        PlantaBase plantaBase = planta.GetComponent<PlantaBase>();
        if (plantaBase != null)
        {
            plantaBase.AcelerarCrecimiento(segundosCrecimientoFertilizante);
        }
    }

    private void AplicarCosecha(GestorSaludPlanta planta, int frutasRecolectadas)
    {
        if (planta != null)
        {
            PlantaBase plantaBase = planta.GetComponent<PlantaBase>();
            if (plantaBase != null) plantaBase.RegistrarCosecha();
        }

        DarMonedas(frutasRecolectadas * monedasPorFruta);
    }

    private void DarMonedas(int cantidad)
    {
        if (cantidad <= 0) return;

        Compra_01 inventario = FindFirstObjectByType<Compra_01>(FindObjectsInactive.Include);
        if (inventario == null)
        {
            Debug.LogWarning("No se encontró Compra_01: no se pudieron entregar las monedas de la cosecha.");
            return;
        }

        inventario.Moneda += cantidad;
        PlayerPrefs.SetInt("Moneda", inventario.Moneda);
        PlayerPrefs.Save();
        inventario.ActualizarTextos();

        Debug.Log("¡Cosecha terminada! Monedas ganadas: +" + cantidad);
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

        if (raizRiego != null) raizRiego.SetActive(false);
        if (raizFertilizante != null) raizFertilizante.SetActive(false);
        if (raizCosecha != null) raizCosecha.SetActive(false);
    }

    private GameObject ObtenerRaiz(Component gestor, GameObject raizConfigurada, string nombre)
    {
        Transform panel = panelMinijuegos.transform;

        if (raizConfigurada != null && raizConfigurada.transform.parent == panel)
        {
            return raizConfigurada;
        }

        Transform actual = gestor.transform;
        if (actual != panel && actual.IsChildOf(panel))
        {
            while (actual.parent != panel)
            {
                actual = actual.parent;
            }

            return actual.gameObject;
        }

        Debug.LogWarning("El minijuego de " + nombre + " no está dentro de PanelMinijuegos. Muévelo dentro del panel para que se muestre y se oculte correctamente.");
        return gestor.gameObject;
    }

    private void AvisarGestoresSinAsignar()
    {
        string faltan = "";
        if (gestorRiego == null) faltan += " gestorRiego";
        if (gestorFertilizante == null) faltan += " gestorFertilizante";
        if (gestorCosecha == null) faltan += " gestorCosecha";

        if (faltan.Length > 0)
        {
            Debug.LogWarning("GestorMinijuegosPrincipal: faltan por asignar en el Inspector:" + faltan + ". Sus botones no abrirán ningún minijuego.");
        }
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