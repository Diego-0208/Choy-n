using UnityEngine;
using UnityEngine.UI;

public class Casilla : MonoBehaviour
{
    [Header("Estado")] 
    public bool tienePlanta = false;

    [Header("Referencia")]
    public Image imagenCasilla;
    public Compra inventario; 

    public static Casilla casillaEnMovimiento = null;

    public void PresionarCasilla() 
    {
        if (!tienePlanta)
        {
            if (casillaEnMovimiento != null)
            {
                Plantar();
                casillaEnMovimiento.Vaciar();
                casillaEnMovimiento = null;
                print("La planta se cambio de sitio");
            }
            else if (inventario.Flor1 > 0)
            {
                inventario.Flor1 -= 1;
                PlayerPrefs.SetInt("Flor1", inventario.Flor1);
                PlayerPrefs.Save();
                inventario.ActualizarTextos();
                Plantar();
                print("Nueva flor plantada.");
            }
            else
            {
                print("No tenemos flores para plantar");
            }
        }
        else 
        {
            if (casillaEnMovimiento == null)
            {
                casillaEnMovimiento = this;
                imagenCasilla.color = Color.yellow;
                print("Planta seleccionada. Toca una casilla vacía para moverla.");
            }

            else if(casillaEnMovimiento == this)
            {
                casillaEnMovimiento = null;
                imagenCasilla.color = Color.green; 
                print("Movimiento cancelado.");
            }
        }
    }
    private void Plantar() 
    {
        tienePlanta = true;
        imagenCasilla.color = Color.green; 

    }
    private void Vaciar() 
    {
        tienePlanta = false;
        imagenCasilla.color = Color.white;
    }

}
