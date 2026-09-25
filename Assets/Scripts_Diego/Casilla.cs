using UnityEngine;
using UnityEngine.UI;

public class Casilla : MonoBehaviour
{
    public int idCasilla; 

    [Header("Estado")] 
    public bool tienePlanta = false;
    public int tipoPlantaActual = 0;

    [Header("Referencia")]
    public Image imagenCasilla;
    public Compra inventario; 

    public static Casilla casillaEnMovimiento = null;

    public void Start()
    {
        tipoPlantaActual = PlayerPrefs.GetInt("Casilla_"+ idCasilla, 0) ;

        if (tipoPlantaActual > 0)
        {
            tienePlanta = true;
            RestaurarColor();
        }
        else 
        {
            VaciarSinGuardar();
        }
    }
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
            else if (GestionInventario.florSeleccionado > 0)
            {
                TryPlantarFlorSeleccionada(GestionInventario.florSeleccionado);
            }
            else
            {
                print("Selecciona una flor de tu inventario");
            }

        }
        else 
        {
            if (casillaEnMovimiento == null)
            {
                casillaEnMovimiento = this;
                imagenCasilla.color = Color.yellow;
                print("Planta seleccionada para mover.Toca una casilla para cambiarla");
            }
            else if (casillaEnMovimiento == this) 
            {
                casillaEnMovimiento = null;
                RestaurarColor();
                print("Movimiento cancelado");
            } 
                
        }
      
    }
    private void TryPlantarFlorSeleccionada(int tipoFlor) 
    {
        if (tipoFlor == 1 && inventario.Flor1 > 0)
        {
            inventario.Flor1 -= 1;
            PlayerPrefs.SetInt("Flor1", inventario.Flor1);
            Plantar(1, Color.green);
        }
        else if (tipoFlor == 2 && inventario.Flor2 > 0)
        {
            inventario.Flor2 -= 1;
            PlayerPrefs.SetInt("Flor2", inventario.Flor2);
            Plantar(2, Color.orange);
        }
        else if (tipoFlor == 3 && inventario.Flor3 > 0)
        {
            inventario.Flor3 -= 1;
            PlayerPrefs.SetInt("Flor3", inventario.Flor3);
            Plantar(3, Color.pink);
        }
        else if (tipoFlor == 4 && inventario.Flor4 > 0)
        {
            inventario.Flor4 -= 1;
            PlayerPrefs.SetInt("Flor4", inventario.Flor4);
            Plantar(4, Color.cyan);
        }
        else 
        {
            print("No te quedan flores para plantar"); 
            return;
        }
        PlayerPrefs.Save(); 
        inventario.ActualizarTextos();
    }
    private void Plantar(int tipo, Color colorPlanta) 
    {
        tienePlanta = true;
        tipoPlantaActual = tipo;
        imagenCasilla.color = colorPlanta;

        PlayerPrefs.SetInt("Casilla_" + idCasilla, tipoPlantaActual);
        PlayerPrefs.Save(); 
    }
    private void Vaciar() 
    {
        tienePlanta = false;
        tipoPlantaActual = 0;
        imagenCasilla.color = Color.white;

        PlayerPrefs.SetInt("Casilla_" + idCasilla, 0); 
        PlayerPrefs.Save();
    }

    private void VaciarSinGuardar() 
    {
        tienePlanta = false;
        tipoPlantaActual = 0; 
        imagenCasilla.color= Color.white;
    }

    private void RestaurarColor() 
    {
        if (tipoPlantaActual == 1) imagenCasilla.color = Color.green;
        else if (tipoPlantaActual == 2) imagenCasilla.color = Color.orange;
        else if (tipoPlantaActual == 3) imagenCasilla.color = Color.pink;
        else if (tipoPlantaActual == 4) imagenCasilla.color = Color.cyan;
    }
}
