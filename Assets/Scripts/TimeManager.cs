using System;
using UnityEngine;
public enum FaseDia { Dia, Tarde, Noche, Lluvia }

[DefaultExecutionOrder(-100)] 
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    public const float SEGUNDOS_DIA_REAL = 86400f;

    private const string KEY_DURACION_DIA = "Reloj_DuracionDia";
    private const string KEY_HORAS_VIRTUALES = "Reloj_HorasVirtuales";
    private const string KEY_ULTIMO_UTC = "Reloj_UltimoUtcTicks";

    [Header("Velocidad")]
    [Tooltip("Segundos reales que dura un día de juego. 86400 = 24 h reales, 3600 = 1 h, 300 = 5 min.")]
    [SerializeField] private float duracionDiaSegundos = SEGUNDOS_DIA_REAL;

    [Header("Fases del día (hora en que empieza cada una, 0-24)")]
    [SerializeField] private float inicioDia = 7f;
    [SerializeField] private float inicioTarde = 14f;
    [SerializeField] private float inicioNoche = 20f;

    [Header("Lluvia")]
    [Tooltip("Probabilidad de que un día de juego tenga lluvia.")]
    [SerializeField, Range(0f, 1f)] private float probabilidadLluviaPorDia = 0.3f;
    [Tooltip("Cuántas horas de juego dura la lluvia cuando ocurre.")]
    [SerializeField] private float duracionLluviaHoras = 3f;
    [Tooltip("Cambia este número para obtener otra secuencia de días lluviosos.")]
    [SerializeField] private int semillaLluvia = 1234;

    [Header("Guardado automático")]
    [Tooltip("Cada cuántos segundos reales se guarda la grilla por seguridad (0 = nunca).")]
    [SerializeField] private float segundosEntreAutoguardados = 60f;

    private double horasJuegoTotales;  
    private double desfaseHorasDebug;  
    private int faseForzada = -1;     
    private FaseDia faseActual;
    private bool faseInicializada;
    private DateTime? inicioPausaUtc;
    private float temporizadorAutoguardado;

    private int climaForzado = -1;    
    private bool lloviendo;
    private FaseDia faseHoraria = FaseDia.Dia; 

   
    public float Escala => SEGUNDOS_DIA_REAL / Mathf.Max(1f, duracionDiaSegundos);

    public float DeltaHoras { get; private set; }

    public bool ModoAcelerado => duracionDiaSegundos < SEGUNDOS_DIA_REAL - 1f;
    public float DuracionDiaSegundos => duracionDiaSegundos;
    public float HoraDelDia => (float)(((horasJuegoTotales % 24.0) + 24.0) % 24.0);
    public int DiaActual => (int)Math.Floor(horasJuegoTotales / 24.0);
    public FaseDia FaseActual => faseActual;
    public FaseDia FaseHoraria => faseHoraria;
    public bool Lloviendo => lloviendo;
    public bool FaseEstaForzada => faseForzada >= 0;
    public bool ClimaEstaForzado => climaForzado >= 0;

    public event Action<FaseDia> OnFaseCambiada;         
    public event Action<FaseDia> OnFaseHorariaCambiada;   
    public event Action<bool> OnLluviaCambiada;         
    public event Action<float> OnHourChanged; 

    public float GetCurrentHour() => HoraDelDia;

    public float RealAHorasJuego(TimeSpan tiempoReal)
    {
        if (tiempoReal.TotalSeconds <= 0) return 0f;
        return (float)(tiempoReal.TotalHours * Escala);
    }

    public double HorasJuegoASegundosReales(float horasJuego)
    {
        return horasJuego * 3600.0 / Escala;
    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        duracionDiaSegundos = PlayerPrefs.GetFloat(KEY_DURACION_DIA, duracionDiaSegundos);
        CargarReloj();
        ActualizarFase(true);
    }

    private void Update()
    {
        DeltaHoras = Time.deltaTime * Escala / 3600f;

        if (ModoAcelerado) horasJuegoTotales += DeltaHoras;
        else horasJuegoTotales = HorasReales() + desfaseHorasDebug;

        ActualizarFase(false);
        OnHourChanged?.Invoke(HoraDelDia);

        if (segundosEntreAutoguardados > 0f)
        {
            temporizadorAutoguardado += Time.unscaledDeltaTime;
            if (temporizadorAutoguardado >= segundosEntreAutoguardados)
            {
                temporizadorAutoguardado = 0f;
                GuardarTodo();
            }
        }
    }

    private void OnApplicationPause(bool pausada)
    {
        if (pausada)
        {
            inicioPausaUtc = DateTime.UtcNow;
            GuardarTodo();
        }
        else if (inicioPausaUtc.HasValue)
        {
            float horas = RealAHorasJuego(DateTime.UtcNow - inicioPausaUtc.Value);
            inicioPausaUtc = null;

            if (ModoAcelerado) horasJuegoTotales += horas;
            AvanzarPlantas(horas);
            GuardarTodo();
        }
    }

    private void OnApplicationQuit()
    {
        GuardarTodo();
    }


    public void GuardarTodo()
    {
        GuardarReloj();

        GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
        if (grilla != null) grilla.GuardarEstadoGrilla();
    }

    private void GuardarReloj()
    {
        PlayerPrefs.SetFloat(KEY_DURACION_DIA, duracionDiaSegundos);
        PlayerPrefs.SetString(KEY_HORAS_VIRTUALES, horasJuegoTotales.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
        PlayerPrefs.SetString(KEY_ULTIMO_UTC, DateTime.UtcNow.Ticks.ToString());
        PlayerPrefs.Save();
    }

    private void CargarReloj()
    {
        if (!ModoAcelerado)
        {
            horasJuegoTotales = HorasReales();
            return;
        }

        if (double.TryParse(PlayerPrefs.GetString(KEY_HORAS_VIRTUALES, ""),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double guardadas)
            && long.TryParse(PlayerPrefs.GetString(KEY_ULTIMO_UTC, ""), out long ticks))
        {
            TimeSpan pasado = DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc);
            horasJuegoTotales = guardadas + RealAHorasJuego(pasado);
        }
        else
        {
            horasJuegoTotales = HorasReales();
        }
    }

    private static double HorasReales()
    {
        return DateTime.Now.Ticks / (double)TimeSpan.TicksPerHour;
    }

    
    public void CambiarDuracionDia(float segundos)
    {
        bool eraAcelerado = ModoAcelerado;
        duracionDiaSegundos = Mathf.Max(1f, segundos);

        if (eraAcelerado && !ModoAcelerado) desfaseHorasDebug = 0;

        GuardarReloj();
    }

    public void SaltarHoras(float horas)
    {
        if (horas <= 0f) return;

        if (ModoAcelerado) horasJuegoTotales += horas;
        else
        {
            desfaseHorasDebug += horas;
            horasJuegoTotales = HorasReales() + desfaseHorasDebug;
        }

        AvanzarPlantas(horas);
        ActualizarFase(false);
        GuardarTodo();
    }

    public void ForzarFase(int indice)
    {
        faseForzada = Mathf.Clamp(indice, -1, 2);
        ActualizarFase(false);
    }

    public void SiguienteFaseForzada()
    {
        ForzarFase(faseForzada >= 2 ? -1 : faseForzada + 1);
    }

    public void ForzarClima(int indice)
    {
        climaForzado = Mathf.Clamp(indice, -1, 1);
        ActualizarFase(false);
    }

    public void SiguienteClimaForzado()
    {
        ForzarClima(climaForzado >= 1 ? -1 : climaForzado + 1);
    }

    public string NombreClima()
    {
        string actual = lloviendo ? "Lluvia" : "Despejado";
        return ClimaEstaForzado ? actual + " (forzado)" : actual + " (auto)";
    }

    public void AvanzarPlantas(float horas)
    {
        if (horas <= 0f) return;

        PlantaBase[] plantas = FindObjectsByType<PlantaBase>(FindObjectsSortMode.None);
        foreach (PlantaBase planta in plantas)
        {
            planta.SimularTiempo(horas);
        }
    }


    private void ActualizarFase(bool forzarEvento)
    {
        bool primeraVez = forzarEvento || !faseInicializada;

        FaseDia nuevaHoraria = faseForzada >= 0 ? (FaseDia)faseForzada : CalcularFaseHoraria();
        if (primeraVez || nuevaHoraria != faseHoraria)
        {
            faseHoraria = nuevaHoraria;
            OnFaseHorariaCambiada?.Invoke(faseHoraria);
        }

        bool nuevaLluvia = climaForzado >= 0 ? climaForzado == 1 : CalcularLluviaAutomatica();
        if (primeraVez || nuevaLluvia != lloviendo)
        {
            lloviendo = nuevaLluvia;
            OnLluviaCambiada?.Invoke(lloviendo);
        }

        FaseDia nueva = lloviendo ? FaseDia.Lluvia : faseHoraria;
        if (primeraVez || nueva != faseActual)
        {
            faseActual = nueva;
            OnFaseCambiada?.Invoke(faseActual);
        }

        faseInicializada = true;
    }

    private FaseDia CalcularFaseHoraria()
    {
        float h = HoraDelDia;
        if (h >= inicioDia && h < inicioTarde) return FaseDia.Dia;
        if (h >= inicioTarde && h < inicioNoche) return FaseDia.Tarde;
        return FaseDia.Noche;
    }

    public bool EstaLloviendo() => lloviendo;

        private bool CalcularLluviaAutomatica()
    {
        System.Random rng = new System.Random(DiaActual * 7919 + semillaLluvia);
        if (rng.NextDouble() > probabilidadLluviaPorDia) return false;

        float inicio = (float)(rng.NextDouble() * Mathf.Max(0f, 24f - duracionLluviaHoras));
        float h = HoraDelDia;
        return h >= inicio && h < inicio + duracionLluviaHoras;
    }
}