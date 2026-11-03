using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecompensaDiaria : MonoBehaviour
{
    private const string KEY_ULTIMO_DIA = "Recompensa_UltimoDia";
    private const string KEY_RACHA = "Recompensa_Racha";
    private const string KEY_MONEDA = "Moneda";
    private const string FORMATO = "yyyy-MM-dd";

    [Header("Recompensas")]
    [SerializeField] private int[] monedasPorDia = { 100, 150, 200, 250, 300, 400, 600 };
    [SerializeField] private int monedasMinimasGarantizadas = 200;
    [SerializeField] private int monedasIniciales = 2000;

    [Header("UI")]
    [SerializeField] private GameObject panelRecompensa;
    [SerializeField] private TextMeshProUGUI textoTitulo;
    [SerializeField] private TextMeshProUGUI textoMonedas;
    [SerializeField] private Button botonReclamar;
    [SerializeField] private TextMeshProUGUI textoMonedasTotales;

    private Compra_01 compra;
    private int recompensaPendiente;
    private int rachaPendiente;
    private bool hayPendiente;

    private void Start()
    {
        compra = FindFirstObjectByType<Compra_01>(FindObjectsInactive.Include);
        if (panelRecompensa != null) panelRecompensa.SetActive(false);
        if (botonReclamar != null) botonReclamar.onClick.AddListener(Reclamar);
        ActualizarTextoTotal();
        Comprobar();
    }

    private void OnApplicationPause(bool pausada)
    {
        if (!pausada) Comprobar();
    }

    private int MonedasActuales()
    {
        if (compra != null) return compra.Moneda;
        return PlayerPrefs.GetInt(KEY_MONEDA, monedasIniciales);
    }

    private void SumarMonedas(int cantidad)
    {
        if (compra != null)
        {
            compra.Moneda += cantidad;
            PlayerPrefs.SetInt(KEY_MONEDA, compra.Moneda);
            compra.ActualizarTextos();
        }
        else
        {
            PlayerPrefs.SetInt(KEY_MONEDA, MonedasActuales() + cantidad);
        }
    }

    public void Comprobar()
    {
        if (hayPendiente) return;

        DateTime hoy = DateTime.Now.Date;
        int racha = PlayerPrefs.GetInt(KEY_RACHA, 0);
        string guardado = PlayerPrefs.GetString(KEY_ULTIMO_DIA, "");

        if (DateTime.TryParseExact(guardado, FORMATO, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ultimo))
        {
            int dias = (hoy - ultimo).Days;
            if (dias <= 0) return;
            racha = dias == 1 ? racha + 1 : 1;
        }
        else
        {
            racha = 1;
        }

        rachaPendiente = racha;
        recompensaPendiente = monedasPorDia.Length > 0 ? monedasPorDia[(racha - 1) % monedasPorDia.Length] : 0;
        hayPendiente = true;
        Mostrar();
    }

    private void Mostrar()
    {
        if (textoTitulo != null) textoTitulo.text = "¡Regalo del día " + rachaPendiente + "!";
        if (textoMonedas != null) textoMonedas.text = "+" + CalcularTotal() + " monedas";
        if (panelRecompensa != null) panelRecompensa.SetActive(true);
    }

    private int CalcularTotal()
    {
        int total = recompensaPendiente;
        int actuales = MonedasActuales();
        if (actuales + total < monedasMinimasGarantizadas)
            total = monedasMinimasGarantizadas - actuales;
        return total;
    }

    public void Reclamar()
    {
         if (!hayPendiente) return;
        GestorSonidos.Reproducir(Sonido.RecompensaDiaria);

        SumarMonedas(CalcularTotal());

        PlayerPrefs.SetString(KEY_ULTIMO_DIA, DateTime.Now.Date.ToString(FORMATO, CultureInfo.InvariantCulture));
        PlayerPrefs.SetInt(KEY_RACHA, rachaPendiente);
        PlayerPrefs.Save();

        hayPendiente = false;
        if (panelRecompensa != null) panelRecompensa.SetActive(false);
        ActualizarTextoTotal();
    }

    private void ActualizarTextoTotal()
    {
        if (textoMonedasTotales != null) textoMonedasTotales.text = "Monedas: " + MonedasActuales();
    }

    public void DebugReiniciar()
    {
        PlayerPrefs.DeleteKey(KEY_ULTIMO_DIA);
        PlayerPrefs.DeleteKey(KEY_RACHA);
        hayPendiente = false;
        Comprobar();
    }

    public void DebugSimularDiaSiguiente()
    {
        PlayerPrefs.SetString(KEY_ULTIMO_DIA, DateTime.Now.Date.AddDays(-1).ToString(FORMATO, CultureInfo.InvariantCulture));
        hayPendiente = false;
        Comprobar();
    }
}