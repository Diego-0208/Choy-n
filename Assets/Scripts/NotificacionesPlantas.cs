using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif


public class NotificacionesPlantas : MonoBehaviour
{
    private const string CANAL = "cuidados_plantas";

    [Tooltip("Máximo de notificaciones programadas a la vez, para no llenar la barra del teléfono.")]
    [SerializeField] private int maximoNotificaciones = 4;
    [Tooltip("Si una planta ya necesita cuidado al salir, avisar después de estos segundos reales.")]
    [SerializeField] private float segundosMinimos = 60f;

    private struct Aviso
    {
        public double segundosReales;
        public string titulo;
        public string texto;
    }

    private IEnumerator Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var canal = new AndroidNotificationChannel()
        {
            Id = CANAL,
            Name = "Cuidado de plantas",
            Importance = Importance.Default,
            Description = "Avisos cuando una planta necesita agua, cosecha o trasplante",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(canal);

        var permiso = new PermissionRequest();
        while (permiso.Status == PermissionStatus.RequestPending)
            yield return null;

        AndroidNotificationCenter.CancelAllNotifications();
#endif
        yield break;
    }

    private void OnApplicationPause(bool pausada)
    {
        if (pausada) ProgramarAvisos();
        else CancelarAvisos();
    }

    private void OnApplicationQuit()
    {
        ProgramarAvisos();
    }


    public void ProgramarAvisos()
    {
        CancelarAvisos();

        if (pruebaPendiente)
        {
            pruebaPendiente = false;
            Enviar("Choyün - Brotes", "¡Ven a mirar tus plantas!", 10);
        }

        TimeManager tm = TimeManager.Instance;
        if (tm == null) return;

        List<Aviso> avisos = new List<Aviso>();

        foreach (PlantaBase planta in FindObjectsByType<PlantaBase>(FindObjectsSortMode.None))
        {
            if (!planta.EstaViva) continue;

            GestorSaludPlanta salud = planta.GetComponent<GestorSaludPlanta>();
            string nombre = planta.Nombre;

            if (salud != null && planta.Permite(PlantaBase.Minijuego.Riego))
            {
                float horas = salud.HorasHastaSequia;
                if (horas >= 0f)
                    avisos.Add(CrearAviso(tm, horas, "¡Tu " + nombre + " tiene sed!", "Riégala antes de que se seque."));
            }

            float horasCosecha = planta.HorasHastaCosecha;
            if (horasCosecha >= 0f)
                avisos.Add(CrearAviso(tm, horasCosecha, nombre + " está lista", "Ya puedes cosecharla."));

            float horasTrasplante = planta.HorasHastaTrasplante;
            if (horasTrasplante >= 0f)
                avisos.Add(CrearAviso(tm, horasTrasplante, nombre + " creció", "Es momento de trasplantarla."));
        }

        avisos.Sort((a, b) => a.segundosReales.CompareTo(b.segundosReales));

        for (int i = 0; i < avisos.Count && i < maximoNotificaciones; i++)
        {
            Enviar(avisos[i].titulo, avisos[i].texto, avisos[i].segundosReales);
        }
    }

    private Aviso CrearAviso(TimeManager tm, float horasJuego, string titulo, string texto)
    {
        double segundos = Math.Max(segundosMinimos, tm.HorasJuegoASegundosReales(horasJuego));
        return new Aviso { segundosReales = segundos, titulo = titulo, texto = texto };
    }

    private bool pruebaPendiente;

    public void EnviarPrueba()
    {
        pruebaPendiente = true;
        Debug.Log("Prueba de notificación lista: sal de la app y espera 10 segundos.");
    }

    private void Enviar(string titulo, string texto, double segundos)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var notificacion = new AndroidNotification();
        notificacion.Title = titulo;
        notificacion.Text = texto;
        notificacion.FireTime = DateTime.Now.AddSeconds(segundos);
        AndroidNotificationCenter.SendNotification(notificacion, CANAL);
#else
        Debug.Log($"[Notificación simulada en {segundos:F0} s] {titulo} — {texto}");
#endif
    }

    private void CancelarAvisos()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidNotificationCenter.CancelAllNotifications();
#endif
    }
}