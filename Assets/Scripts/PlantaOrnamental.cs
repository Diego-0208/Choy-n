using UnityEngine;

public class PlantaOrnamental : PlantaBase
{
    [Header("Configuración Ornamental")]
    [SerializeField] private float bonoDeBelleza = 15f;
    [SerializeField] private float radioAtraccionPolinizadores = 5f;

    protected override void Start()
    {
        base.Start(); 
        tipoDePlanta = TipoPlanta.Ornamental;
    }

    protected override void Update()
    {
        base.Update();
        if (!estaViva) return;

        AtraerPolinizadores();
    }

    private void AtraerPolinizadores()
    {
        // podria servir para poner abejas o bichos
    }

    public float ObtenerBonoBelleza()
    {
        return estaViva ? bonoDeBelleza : 0f;
    }
}