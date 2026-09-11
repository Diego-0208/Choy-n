using UnityEngine;

public class PlantaVegetal : PlantaBase
{
    [Header("Configuración Vegetal")]
    [SerializeField] private int cantidadDeComida = 3;
    [SerializeField] private string nombreDelRecurso = "Verdura Fresca";

    protected override void Start()
    {
        base.Start();
        tipoDePlanta = TipoPlanta.Comun;
    }

    public void CosecharVegetal()
    {
        if (!estaViva) return;

        Debug.Log($"Cosechaste {cantidadDeComida}x {nombreDelRecurso} de la planta {nombrePlanta}.");
        
        
        Morir();
    }
}