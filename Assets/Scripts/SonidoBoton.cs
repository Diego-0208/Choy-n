using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SonidoBoton : MonoBehaviour
{
    [SerializeField] private Sonido sonido = Sonido.Click;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Reproducir);
    }

    private void Reproducir()
    {
        GestorSonidos.Reproducir(sonido);
    }
}