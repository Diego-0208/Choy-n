using System;
using System.Collections.Generic;

[Serializable]
public class PlantaData
{
    public int tipoFlor;

    // Estado de PlantaBase
    public float tiempoDeVidaActual;
    public int etapaActual;
    public float temporizadorCrecimiento;
    public bool estaViva;

    // Estado de GestorSaludPlanta
    public float nivelAgua;
    public float temperaturaAmbiente;

    public PlantaData(int tipoFlor, PlantaBase pb, GestorSaludPlanta gsp)
    {
        this.tipoFlor = tipoFlor;

        if (pb != null)
        {
            this.tiempoDeVidaActual = pb.TiempoDeVidaActual;
            this.etapaActual = pb.EtapaActual;
            this.temporizadorCrecimiento = pb.TemporizadorCrecimiento;
            this.estaViva = pb.EstaViva;
        }

        if (gsp != null)
        {
            this.nivelAgua = gsp.NivelAgua;
            this.temperaturaAmbiente = gsp.TemperaturaAmbiente;
        }
    }
}

[Serializable]
public class MacetaData
{
    public int posX;
    public int posY;
    public bool tienePlanta;
    public PlantaData plantaData;

    public MacetaData(int posX, int posY, bool tienePlanta, PlantaData plantaData)
    {
        this.posX = posX;
        this.posY = posY;
        this.tienePlanta = tienePlanta;
        this.plantaData = plantaData;
    }
}

[Serializable]
public class ListaMacetasData
{
    public List<MacetaData> macetas = new List<MacetaData>();
}