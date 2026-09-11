using UnityEngine;


public class PlantaToxica : PlantaBase
{
    [Header("Configuración Tóxica")]
    [SerializeField] private float radioDeVeneno = 3f;
    [SerializeField] private float danoPorSegundo = 5f;

    protected override void Start()
    {
        base.Start();
        tipoDePlanta = TipoPlanta.Toxica;
    }

    protected override void Update()
    {
        base.Update();
        if (!estaViva) return;

        EmitirToxina();
    }

    private void EmitirToxina()
    {
        // Lógica para detectar jugadores/enemigos en el radioDeVeneno
    }
}