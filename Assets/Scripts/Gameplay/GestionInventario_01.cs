using UnityEngine;
using TMPro;

public class GestionInventario_01 : MonoBehaviour
{
    [Header("Contadores de Texto de la UI")]
    public TextMeshProUGUI textCantidadFlor1;
    public TextMeshProUGUI textCantidadFlor2;
    public TextMeshProUGUI textCantidadFlor3;
    public TextMeshProUGUI textCantidadFlor4;
    public TextMeshProUGUI textCantidadMacetas; // <- NUEVO

    [Header("Referencia al Sistema de Compras")]
    public Compra_01 inventario;

    private void Update()
    {
        if (inventario == null) return;

        if (textCantidadFlor1 != null) textCantidadFlor1.text = inventario.Flor1.ToString();
        if (textCantidadFlor2 != null) textCantidadFlor2.text = inventario.Flor2.ToString();
        if (textCantidadFlor3 != null) textCantidadFlor3.text = inventario.Flor3.ToString();
        if (textCantidadFlor4 != null) textCantidadFlor4.text = inventario.Flor4.ToString();
        if (textCantidadMacetas != null) textCantidadMacetas.text = inventario.Macetas.ToString(); 
    }
}