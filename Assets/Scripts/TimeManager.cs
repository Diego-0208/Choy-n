using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("Configuración de Tiempo")]
    [Tooltip("Factor de velocidad: 1 = tiempo real, 288 = 24h pasan en 5 minutos")]
    public float timeScale = 288f; 
    
    [Header("Debug")]
    public bool isDebugMode = true;
    [Range(0, 24)] public float debugHour = 12f; // Hora actual simulada (0 a 24)

    private DateTime gameDateTime;
    private const string LastSaveTimeKey = "LastSaveDateTime";

    public event Action<float> OnHourChanged; // Evento para notificar a las plantas/UI

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadTime();
    }

    private void Update()
    {
        if (isDebugMode)
        {
            // En modo debug podemos mover la hora manualmente o dejarla correr acelerada
            debugHour += (Time.deltaTime * timeScale) / 3600f;
            if (debugHour >= 24f) debugHour = 0f;
        }
        else
        {
            // Tiempo real basado en la hora del sistema o guardada
            // Aquí puedes calcular la diferencia desde el último cierre
        }

        OnHourChanged?.Invoke(GetCurrentHour());
    }

    public float GetCurrentHour()
    {
        return isDebugMode ? debugHour : (float)DateTime.Now.TimeOfDay.TotalHours;
    }

    public void AddDebugHours(float hours)
    {
        debugHour = (debugHour + hours) % 24f;
    }

    private void OnApplicationQuit()
    {
        SaveTime();
    }

    public void SaveTime()
    {
        PlayerPrefs.SetString(LastSaveTimeKey, DateTime.Now.ToString());
        PlayerPrefs.Save();
    }

    public void LoadTime()
    {
        if (PlayerPrefs.HasKey(LastSaveTimeKey))
        {
            DateTime lastSave = DateTime.Parse(PlayerPrefs.GetString(LastSaveTimeKey));
            TimeSpan timePassed = DateTime.Now - lastSave;
            // Aquí puedes calcular cuánto crecieron las plantas while la app estuvo cerrada
            Debug.Log("Tiempo transcurrido fuera de la app: " + timePassed.TotalHours + " horas.");
        }
    }
}