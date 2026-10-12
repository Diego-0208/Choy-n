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

    public int Macetas = 5;

    [Header("Referencias de UI (Monedas)")]
    public TextMeshProUGUI monedas_text_Gameplay;
    public TextMeshProUGUI monedas_text;

    [Header("Prefabs de Sobres de Semillas (UI Inventario)")]
    public GameObject prefabSobre1;
    public GameObject prefabSobre2;
    public GameObject prefabSobre3;
    public GameObject prefabSobre4;

    [Header("Prefabs de Flores (Para plantar en Maceta)")]
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

        Macetas = PlayerPrefs.GetInt("Macetas", 5);

        ActualizarTextos();
        CargarInventarioInicial();
    }

    private void CargarInventarioInicial()
    {
        // Instancia los SOBRES de semillas en el inventario al cargar la partida
        if (Flor1 > 0) CrearObjetoEnInventario(prefabSobre1, spawnFlor1);
        if (Flor2 > 0) CrearObjetoEnInventario(prefabSobre2, spawnFlor2);
        if (Flor3 > 0) CrearObjetoEnInventario(prefabSobre3, spawnFlor3);
        if (Flor4 > 0) CrearObjetoEnInventario(prefabSobre4, spawnFlor4);
    }

    public void ComprarFlor1() { EjecutarCompra(ref Flor1, "Flor1", "1", 200, prefabSobre1, spawnFlor1); }
    public void ComprarFlor2() { EjecutarCompra(ref Flor2, "Flor2", "2", 400, prefabSobre2, spawnFlor2); }
    public void ComprarFlor3() { EjecutarCompra(ref Flor3, "Flor3", "3", 150, prefabSobre3, spawnFlor3); }
    public void ComprarFlor4() { EjecutarCompra(ref Flor4, "Flor4", "4", 700, prefabSobre4, spawnFlor4); }

    private void EjecutarCompra(ref int contadorFlor, string claveSave, string idPlanta, int costo, GameObject prefabSobre, Transform puntoSpawn)
    {
        if (Moneda >= costo)
        {
            Moneda -= costo;
            contadorFlor += 1;

            PlayerPrefs.SetInt("Moneda", Moneda);
            PlayerPrefs.SetInt(claveSave, contadorFlor);

            PlayerPrefs.SetInt("Desbloqueada_" + idPlanta, 1);
            PlayerPrefs.Save();

            Debug.Log($"[Compra] ¡Comprada exitosamente! Se guardó 'Desbloqueada_{idPlanta}' = 1");

            ActualizarTextos();

            // Si es la primera semilla que se compra de este tipo y el contenedor está vacío, crea el sobre UI
            if (puntoSpawn != null && puntoSpawn.childCount == 0)
            {
                CrearObjetoEnInventario(prefabSobre, puntoSpawn);
            }

            AlmanaqueManager managerAlmanaque = FindFirstObjectByType<AlmanaqueManager>();
            if (managerAlmanaque != null)
            {
                managerAlmanaque.AbrirAlmanaque();
            }
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
            // Limpia instancias previas duplicadas en el slot antes de instanciar el sobre
            foreach (Transform child in puntoSpawn)
            {
                Destroy(child.gameObject);
            }
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
            case 1: if (Flor1 > 0) Flor1--; PlayerPrefs.SetInt("Flor1", Flor1); break;
            case 2: if (Flor2 > 0) Flor2--; PlayerPrefs.SetInt("Flor2", Flor2); break;
            case 3: if (Flor3 > 0) Flor3--; PlayerPrefs.SetInt("Flor3", Flor3); break;
            case 4: if (Flor4 > 0) Flor4--; PlayerPrefs.SetInt("Flor4", Flor4); break;
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

    public void ConsumirMaceta()
    {
        if (Macetas > 0)
        {
            Macetas--;
            PlayerPrefs.SetInt("Macetas", Macetas);
            PlayerPrefs.Save();
            ActualizarTextos();
        }
    }

    public void DevolverMaceta()
    {
        Macetas++;
        PlayerPrefs.SetInt("Macetas", Macetas);
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