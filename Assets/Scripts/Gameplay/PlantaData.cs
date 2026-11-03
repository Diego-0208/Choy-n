using System;
using System.Collections.Generic;

[Serializable]
public class PlantaData
{
    public int tipoFlor;
    public int cantidadMoho;
    public int cantidadPlagas;
    public float tiempoDeVidaActual;
    public int etapaActual;
    public float temporizadorCrecimiento;
    public bool estaViva;
    public float temporizadorCosecha;
    public bool yaTrasplantada;

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
            this.temporizadorCosecha = pb.TemporizadorCosecha;
            this.yaTrasplantada = pb.YaTrasplantada;
        }

        if (gsp != null)
        {
            this.nivelAgua = gsp.NivelAgua;
            this.temperaturaAmbiente = gsp.TemperaturaAmbiente;
            this.cantidadMoho = gsp.CantidadMoho;
            this.cantidadPlagas = gsp.CantidadPlagas;
        }
    }
}

[Serializable]
public class MacetaData
{
    public int posX;
    public int posY;
    public bool tienePlanta;
    public int tamano; 
    public PlantaData plantaData;

    public MacetaData(int posX, int posY, bool tienePlanta, PlantaData plantaData, int tamano = 0)
    {
        this.posX = posX;
        this.posY = posY;
        this.tienePlanta = tienePlanta;
        this.plantaData = plantaData;
        this.tamano = tamano;
    }
}

[Serializable]
public class ListaMacetasData
{
    public long guardadoUtcTicks; 
    public List<MacetaData> macetas = new List<MacetaData>();
}