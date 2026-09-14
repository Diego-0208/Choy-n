using UnityEngine;


public class PlantaFrutal : PlantaBase
{
    [Header("Configuración Frutal")]
    [SerializeField] private float tiempoEntreFrutos = 10f;
    private float temporizadorFruto;

    protected override void Start()
    {
        base.Start();
        tipoDePlanta = TipoPlanta.Frutal;
        temporizadorFruto = tiempoEntreFrutos;
    }

    protected override void Update()
    {
        base.Update(); 
        if (!estaViva) return;

        ProducirFruto();
    }

    private void ProducirFruto()
    {
        temporizadorFruto -= Time.deltaTime;
        if (temporizadorFruto <= 0)
        {
            Debug.Log($"{nombrePlanta} ha producido un fruto.");
            temporizadorFruto = tiempoEntreFrutos;
        }
    }
}