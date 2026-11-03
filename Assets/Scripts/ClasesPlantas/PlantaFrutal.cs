using UnityEngine;

public class PlantaFrutal : PlantaBase
{
    [Header("Configuración Frutal")]
    [Tooltip("Horas de juego entre frutos.")]
    [SerializeField] private float tiempoEntreFrutos = 12f;
    private float temporizadorFruto;

    protected override void Start()
    {
        base.Start();
        tipoDePlanta = TipoPlanta.Frutal;
        temporizadorFruto = tiempoEntreFrutos;
    }

    public override void Avanzar(float horas)
    {
        base.Avanzar(horas);
        if (!estaViva) return;

        if (EsAdulta)
        {
            ProducirFruto(horas);
        }
    }

    private void ProducirFruto(float horas)
    {
        temporizadorFruto -= horas;
        if (temporizadorFruto <= 0)
        {
            Debug.Log($"{nombrePlanta} ha producido un fruto.");
            temporizadorFruto = tiempoEntreFrutos;
        }
    }
}