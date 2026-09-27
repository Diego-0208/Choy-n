using UnityEngine;
using TMPro;

public class Compra_01 : MonoBehaviour
{
    [Header("Economía y Recursos")]
    public int Moneda;
    public int Flor1;
    public int Flor2;
    public int Flor3;
    public int Flor4;

    [Header("Referencias de UI (Monedas)")]
    public TextMeshProUGUI monedas_text_Gameplay;
    public TextMeshProUGUI monedas_text;

    [Header("Prefabs de Flores Arrastrables")]
    public GameObject prefabFlor1;
    public GameObject prefabFlor2;
    public GameObject prefabFlor3;
    public GameObject prefabFlor4;

    [Header("Contenedores de Spawn por Flor (Inventario)")]
    public Transform spawnFlor1;
    public Transform spawnFlor2;
    public Transform spawnFlor3;
    public Transform spawnFlor4;

    private void Start()
    {
        Moneda = PlayerPrefs.GetInt("Moneda", 2000);
        Flor1 = PlayerPrefs.GetInt("Flor1", 0);
        Flor2 = PlayerPrefs.GetInt("Flor2", 0);
        Flor3 = PlayerPrefs.GetInt("Flor3", 0);
        Flor4 = PlayerPrefs.GetInt("Flor4", 0);

        ActualizarTextos();
        CargarInventarioInicial();
    }

    private void CargarInventarioInicial()
    {
        for (int i = 0; i < Flor1; i++) CrearObjetoEnInventario(prefabFlor1, spawnFlor1);
        for (int i = 0; i < Flor2; i++) CrearObjetoEnInventario(prefabFlor2, spawnFlor2);
        for (int i = 0; i < Flor3; i++) CrearObjetoEnInventario(prefabFlor3, spawnFlor3);
        for (int i = 0; i < Flor4; i++) CrearObjetoEnInventario(prefabFlor4, spawnFlor4);
    }

    public void ComprarFlor1() { EjecutarCompra(ref Flor1, "Flor1", 200, prefabFlor1, spawnFlor1); }
    public void ComprarFlor2() { EjecutarCompra(ref Flor2, "Flor2", 400, prefabFlor2, spawnFlor2); }
    public void ComprarFlor3() { EjecutarCompra(ref Flor3, "Flor3", 150, prefabFlor3, spawnFlor3); }
    public void ComprarFlor4() { EjecutarCompra(ref Flor4, "Flor4", 700, prefabFlor4, spawnFlor4); }

    private void EjecutarCompra(ref int contadorFlor, string claveSave, int costo, GameObject prefabItem, Transform puntoSpawn)
    {
        if (Moneda >= costo)
        {
            Moneda -= costo;
            contadorFlor += 1;

            PlayerPrefs.SetInt("Moneda", Moneda);
            PlayerPrefs.SetInt(claveSave, contadorFlor);
            PlayerPrefs.Save();

            ActualizarTextos();
            CrearObjetoEnInventario(prefabItem, puntoSpawn);
        }
        else
        {
            print($"No tienes suficientes monedas para realizar la compra (Costo: {costo})");
        }
    }

    private void CrearObjetoEnInventario(GameObject prefab, Transform puntoSpawn)
    {
        if (prefab != null && puntoSpawn != null)
        {
            Instantiate(prefab, puntoSpawn);
        }
    }

    public void ActualizarTextos()
    {
        if (monedas_text_Gameplay != null)
            monedas_text_Gameplay.text = "Monedas: " + Moneda.ToString();

        if (monedas_text != null)
            monedas_text.text = "Monedas: " + Moneda.ToString();
    }

    public void ConsumirFlor(int tipoFlor)
    {
        switch (tipoFlor)
        {
            case 1: Flor1--; PlayerPrefs.SetInt("Flor1", Flor1); break;
            case 2: Flor2--; PlayerPrefs.SetInt("Flor2", Flor2); break;
            case 3: Flor3--; PlayerPrefs.SetInt("Flor3", Flor3); break;
            case 4: Flor4--; PlayerPrefs.SetInt("Flor4", Flor4); break;
        }
        PlayerPrefs.Save();
        ActualizarTextos();
    }

    public void DevolverFlor(int tipoFlor)
    {
        switch (tipoFlor)
        {
            case 1: Flor1++; PlayerPrefs.SetInt("Flor1", Flor1); break;
            case 2: Flor2++; PlayerPrefs.SetInt("Flor2", Flor2); break;
            case 3: Flor3++; PlayerPrefs.SetInt("Flor3", Flor3); break;
            case 4: Flor4++; PlayerPrefs.SetInt("Flor4", Flor4); break;
        }
        PlayerPrefs.Save();
        ActualizarTextos();
    }

    public Transform GetSpawnPorTipo(int tipoFlor)
    {
        switch (tipoFlor)
        {
            case 1: return spawnFlor1;
            case 2: return spawnFlor2;
            case 3: return spawnFlor3;
            case 4: return spawnFlor4;
            default: return null;
        }
    }
}