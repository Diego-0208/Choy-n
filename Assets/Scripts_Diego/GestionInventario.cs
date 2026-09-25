using UnityEngine;
using TMPro; 

public class GestionInventario : MonoBehaviour
{
    public static int florSeleccionado = 0;

    public TextMeshProUGUI textCantidadFlor1;
    public TextMeshProUGUI textCantidadFlor2;
    public TextMeshProUGUI textCantidadFlor3;
    public TextMeshProUGUI textCantidadFlor4;

    public Compra inventario;

    private void Update()
    {
        if (textCantidadFlor1!= null) textCantidadFlor1.text = inventario.Flor1.ToString();
        if (textCantidadFlor2 != null) textCantidadFlor2.text = inventario.Flor2.ToString();
        if (textCantidadFlor3 != null) textCantidadFlor3.text = inventario.Flor3.ToString();
        if (textCantidadFlor4 != null) textCantidadFlor4.text = inventario.Flor4.ToString();
    }

    public void SeleccionarFlor1() { florSeleccionado = 1; print("Flor 1 seleccionado para plantar"); }
    public void SeleccionarFlor2() { florSeleccionado = 2; print("Flor 2 seleccionado para plantar"); }
    public void SeleccionarFlor3() { florSeleccionado = 3; print("Flor 3 seleccionado para plantar"); }
    public void SeleccionarFlor4() { florSeleccionado = 4; print("Flor 4 seleccionado para plantar"); }
    public void Deseleccionar() { florSeleccionado = 0; }

}
