using UnityEngine;

public class PlantaMedicinal : PlantaBase
{
    [Header("Configuración Medicinal")]
    [SerializeField] private float potenciaCurativa = 25f;

    protected override void Start()
    {
        base.Start(); 
        tipoDePlanta = TipoPlanta.Medicinal;
    }

    public void CosecharCuracion()
    {
        if (!estaViva) return;
        Debug.Log($"Cosechada {nombrePlanta}. Cura {potenciaCurativa} de vida.");
    }
}