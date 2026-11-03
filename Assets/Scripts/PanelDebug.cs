using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PanelDebug : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El panel con los botones de debug (se abre y cierra con AlternarPanel).")]
    [SerializeField] private GameObject panel;
    [Tooltip("El botón que abre el panel. Se oculta en builds que no son de desarrollo.")]
    [SerializeField] private GameObject botonAbrir;
    [SerializeField] private TextMeshProUGUI textoEstado;
    [SerializeField] private TextMeshProUGUI textoVelocidad;

    [Header("Configuración")]
    [Tooltip("Actívalo para mostrar el debug también en una build normal (ej. para presentar).")]
    [SerializeField] private bool mostrarSiempre = true;
    [Tooltip("Duraciones de un día (en segundos reales) que recorre el botón de velocidad.")]
    [SerializeField] private float[] duracionesDia = { 86400f, 3600f, 300f, 60f };

    private Compra_01 compra;
    private int indiceVelocidad;
    private float temporizadorTexto;

    private void Awake()
    {
        bool habilitado = mostrarSiempre || Debug.isDebugBuild; 
        if (!habilitado)
        {
            if (botonAbrir != null) botonAbrir.SetActive(false);
            if (panel != null) panel.SetActive(false);
            enabled = false;
            return;
        }

        if (panel != null) panel.SetActive(false);
    }

    private void Start()
    {
        compra = FindFirstObjectByType<Compra_01>(FindObjectsInactive.Include);

        if (TimeManager.Instance != null)
        {
            for (int i = 0; i < duracionesDia.Length; i++)
            {
                if (Mathf.Approximately(duracionesDia[i], TimeManager.Instance.DuracionDiaSegundos)) indiceVelocidad = i;
            }
        }
        ActualizarTextos();
    }

    private void Update()
    {
        if (panel == null || !panel.activeSelf) return;

        temporizadorTexto += Time.unscaledDeltaTime;
        if (temporizadorTexto >= 0.25f)
        {
            temporizadorTexto = 0f;
            ActualizarTextos();
        }
    }


    public void AlternarPanel()
    {
        if (panel == null) return;
        panel.SetActive(!panel.activeSelf);
        ActualizarTextos();
    }

    public void SiguienteVelocidad()
    {
        if (TimeManager.Instance == null || duracionesDia.Length == 0) return;

        indiceVelocidad = (indiceVelocidad + 1) % duracionesDia.Length;
        TimeManager.Instance.CambiarDuracionDia(duracionesDia[indiceVelocidad]);
        ActualizarTextos();
    }

    public void CambiarMonedas(int cantidad)
    {
        if (compra == null) compra = FindFirstObjectByType<Compra_01>(FindObjectsInactive.Include);
        if (compra == null)
        {
            Debug.LogWarning("PanelDebug: no se encontró Compra_01 en la escena.");
            return;
        }

        compra.Moneda = Mathf.Max(0, compra.Moneda + cantidad);
        PlayerPrefs.SetInt("Moneda", compra.Moneda);
        PlayerPrefs.Save();
        compra.ActualizarTextos();
    }

    public void SaltarHoras(float horas)
    {
        if (TimeManager.Instance != null) TimeManager.Instance.SaltarHoras(horas);
        ActualizarTextos();
    }

    public void SiguienteFase()
    {
        if (TimeManager.Instance != null) TimeManager.Instance.SiguienteFaseForzada();
        ActualizarTextos();
    }

    public void SiguienteClima()
    {
        if (TimeManager.Instance != null) TimeManager.Instance.SiguienteClimaForzado();
        ActualizarTextos();
    }

    public void InfestarPlantaSeleccionada()
    {
        CasillaUI casilla = MenuCuidadosUI.Instance != null ? MenuCuidadosUI.Instance.CasillaSeleccionada : null;
        GestorSaludPlanta salud = casilla != null ? casilla.GetComponentInChildren<GestorSaludPlanta>() : null;

        GestorSaludPlanta[] objetivos = salud != null
            ? new[] { salud }
            : FindObjectsByType<GestorSaludPlanta>(FindObjectsSortMode.None);

        foreach (GestorSaludPlanta s in objetivos)
        {
            s.AgregarMoho(3);
            s.AgregarPlagas(3);
        }
    }

    public void ProbarNotificacion()
    {
        NotificacionesPlantas notif = FindFirstObjectByType<NotificacionesPlantas>();
        if (notif != null) notif.EnviarPrueba();
    }

    public void BorrarPartida()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    private void ActualizarTextos()
    {
        TimeManager tm = TimeManager.Instance;
        if (tm == null) return;

        if (textoVelocidad != null)
        {
            textoVelocidad.text = "1 día = " + FormatearDuracion(tm.DuracionDiaSegundos);
        }

        if (textoEstado != null)
        {
            float h = tm.HoraDelDia;
            int horas = Mathf.FloorToInt(h);
            int minutos = Mathf.FloorToInt((h - horas) * 60f);
            string fase = tm.FaseHoraria + (tm.FaseEstaForzada ? " (forzada)" : "");
            textoEstado.text = $"{horas:00}:{minutos:00}  ·  {fase}  ·  {tm.NombreClima()}\nx{tm.Escala:0.#}";
        }
    }

    private static string FormatearDuracion(float segundos)
    {
        if (segundos >= 86399f) return "24 h (real)";
        if (segundos >= 3600f) return (segundos / 3600f).ToString("0.#") + " h";
        if (segundos >= 60f) return (segundos / 60f).ToString("0.#") + " min";
        return segundos.ToString("0") + " s";
    }
}