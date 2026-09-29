using UnityEngine;
using UnityEngine.UI;

public class TarjetaAlmanaque : MonoBehaviour
{
    [Header("Configuración")]
    public Almanaque datosAlmanaque;

    [Header("Referencias UI")]
    [SerializeField] private Button botonTarjeta;
    [SerializeField] private Image imagenPlanta;

    private AlmanaqueManager manager;

    private void Awake()
    {
        if (manager == null)
        {
            manager = FindFirstObjectByType<AlmanaqueManager>();
        }
    }

    public void Inicializar(AlmanaqueManager almanaqueManager)
    {
        manager = almanaqueManager;

        if (botonTarjeta == null)
            botonTarjeta = GetComponent<Button>();

        if (imagenPlanta == null)
            imagenPlanta = GetComponent<Image>();

        ActualizarEstado();
    }

    public void ActualizarEstado()
    {
        if (manager == null)
            manager = FindFirstObjectByType<AlmanaqueManager>();

        if (datosAlmanaque == null || manager == null) return;

        bool desbloqueada = manager.EstaPlantaDesbloqueada(datosAlmanaque);

        Debug.Log($"Planta: {datosAlmanaque.nombre} | ID: '{datosAlmanaque.idPlanta}' | Desbloqueada: {desbloqueada}");

        // Activa o desactiva el GameObject entero de la tarjeta según su estado de desbloqueo
        gameObject.SetActive(desbloqueada);

        if (desbloqueada)
        {
            if (imagenPlanta != null && datosAlmanaque.img != null)
            {
                imagenPlanta.sprite = datosAlmanaque.img;
            }

            if (botonTarjeta != null)
            {
                botonTarjeta.interactable = true;
            }
        }
    }

    public void OnClickTarjeta()
    {
        // Solo permite la selección si la tarjeta está activa y asignada
        if (gameObject.activeSelf && manager != null && datosAlmanaque != null)
        {
            manager.BotonPlanta(datosAlmanaque);
        }
    }
}