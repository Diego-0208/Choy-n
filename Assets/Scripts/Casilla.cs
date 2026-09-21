using UnityEngine;
using UnityEngine.UI;

public class Casilla : MonoBehaviour
{
    [Header("Estado")] 
    public bool tienePlanta = false;
    public int tipoPlantaActual = 0;

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
                Plantar(casillaEnMovimiento.tipoPlantaActual, casillaEnMovimiento.imagenCasilla.color);
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
                Plantar(1,Color.green);
                print("Nueva flor plantada.");
            }
            else if (inventario.Flor2 > 0)
            {
                inventario.Flor2 -= 1;
                PlayerPrefs.SetInt("Flor2", inventario.Flor2);
                PlayerPrefs.Save();
                inventario.ActualizarTextos();
                Plantar(2,Color.orange);
                print("Nueva flor plantada.2");
            }
            else if (inventario.Flor3 > 0)
            {
                inventario.Flor3 -= 1;
                PlayerPrefs.SetInt("Flor3", inventario.Flor3);
                PlayerPrefs.Save();
                inventario.ActualizarTextos();
                Plantar(3,Color.pink);
                print("Nueva flor plantada.3");
            }
            else if (inventario.Flor4 > 0)
            {
                inventario.Flor4 -= 1;
                PlayerPrefs.SetInt("Flor4", inventario.Flor4);
                PlayerPrefs.Save();
                inventario.ActualizarTextos();
                Plantar(4,Color.cyan);
                print("Nueva flor plantada.4");
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
                RestaurarColor(); 
                print("Movimiento cancelado.");
            }
        }
    }
    private void Plantar(int tipo, Color colorPlanta) 
    {
        tienePlanta = true;
        tipoPlantaActual = tipo;
        imagenCasilla.color = colorPlanta;

    }
    private void Vaciar() 
    {
        tienePlanta = false;
        tipoPlantaActual = 0;
        imagenCasilla.color = Color.white;
    }

    private void RestaurarColor() 
    {
        if (tipoPlantaActual == 1) imagenCasilla.color = Color.green;
        else if (tipoPlantaActual == 2) imagenCasilla.color = Color.orange;
        else if (tipoPlantaActual == 3) imagenCasilla.color = Color.pink;
        else if (tipoPlantaActual == 4) imagenCasilla.color = Color.cyan;
    }
}
