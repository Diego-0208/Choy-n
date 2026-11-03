using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ZonaInventarioUI : MonoBehaviour, IDropHandler
{
    [Header("Resaltado opcional mientras se arrastra una maceta vacía")]
    [SerializeField] private Image imagenResaltado;
    [SerializeField] private Color colorResaltado = new Color(0.6f, 1f, 0.6f, 1f);

    private Color colorOriginal;
    private bool resaltado;

    private void Awake()
    {
        if (imagenResaltado != null) colorOriginal = imagenResaltado.color;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject arrastrado = eventData.pointerDrag;
        if (arrastrado == null) return;

        MacetaUI maceta = arrastrado.GetComponent<MacetaUI>();
        if (maceta != null) maceta.GuardarEnInventario();
    }

    private void Update()
    {
        MacetaUI m = MacetaUI.Arrastrada;
        Resaltar(m != null && !m.tienePlanta);
    }

    private void Resaltar(bool activo)
    {
        if (imagenResaltado == null || activo == resaltado) return;
        resaltado = activo;
        imagenResaltado.color = activo ? colorResaltado : colorOriginal;
    }
}