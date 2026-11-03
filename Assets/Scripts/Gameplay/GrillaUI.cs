using System.Collections.Generic;
using UnityEngine;

public class GrillaUI : MonoBehaviour
{
    [Header("Dimensiones de Grilla")]
    [SerializeField] private int columnas = 10;
    [SerializeField] private int filas = 20;

    [Header("Referencias UI")]
    [SerializeField] private GameObject casillaPrefab;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Referencias de Compras y Prefabs")]
    [SerializeField] private Compra_01 compraScript;
    [SerializeField] private GameObject prefabMaceta; // Prefab de la MacetaUI instalable

    private Dictionary<Vector2Int, CasillaUI> diccionarioCasillas = new Dictionary<Vector2Int, CasillaUI>();

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        GenerarGrilla();
    }

    private void Start()
    {
        SetVisibilidadGrilla(true);

        if (compraScript == null)
            compraScript = FindFirstObjectByType<Compra_01>();

        CargarEstadoGrilla();
    }

    private void GenerarGrilla()
    {
        diccionarioCasillas.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        for (int y = 0; y < filas; y++)
        {
            for (int x = 0; x < columnas; x++)
            {
                GameObject nuevaCasilla = Instantiate(casillaPrefab, transform);
                nuevaCasilla.name = $"Casilla_{x}_{y}";

                CasillaUI casillaScript = nuevaCasilla.GetComponent<CasillaUI>();
                if (casillaScript != null)
                {
                    casillaScript.Inicializar(x, y);
                    diccionarioCasillas.Add(new Vector2Int(x, y), casillaScript);
                }
            }
        }
    }

    public CasillaUI GetCasilla(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (diccionarioCasillas.ContainsKey(pos))
        {
            return diccionarioCasillas[pos];
        }
        return null;
    }

    public void GuardarEstadoGrilla()
    {
        ListaMacetasData listaGuardada = new ListaMacetasData();

        foreach (KeyValuePair<Vector2Int, CasillaUI> par in diccionarioCasillas)
        {
            CasillaUI casilla = par.Value;

            // Buscar si la casilla contiene un componente MacetaUI
            MacetaUI maceta = casilla.GetComponentInChildren<MacetaUI>();
            if (maceta != null)
            {
                PlantaData pData = null;

                // Si la maceta tiene una planta colocada
                if (maceta.tienePlanta && maceta.plantaActual != null)
                {
                    PlantaBase pb = maceta.plantaActual;
                    GestorSaludPlanta gsp = pb.GetComponent<GestorSaludPlanta>();

                    // Obtenemos el tipo de flor/planta desde PlantaBase o su enum en int
                    int tipoFlorInt = (int)pb.Tipo + 1; 

                    pData = new PlantaData(tipoFlorInt, pb, gsp);
                }

                listaGuardada.macetas.Add(new MacetaData(casilla.posX, casilla.posY, maceta.tienePlanta, pData));
            }
        }

        string json = JsonUtility.ToJson(listaGuardada);
        PlayerPrefs.SetString("MacetasEnGrilla", json);
        PlayerPrefs.Save();
        Debug.Log("Estado de las macetas y plantas guardado exitosamente.");
    }

    public void CargarEstadoGrilla()
    {
        if (!PlayerPrefs.HasKey("MacetasEnGrilla")) return;

        string json = PlayerPrefs.GetString("MacetasEnGrilla");
        ListaMacetasData listaGuardada = JsonUtility.FromJson<ListaMacetasData>(json);

        if (listaGuardada == null || listaGuardada.macetas == null) return;

        foreach (MacetaData data in listaGuardada.macetas)
        {
            CasillaUI casilla = GetCasilla(data.posX, data.posY);
            if (casilla != null && !casilla.estaOcupada)
            {
                if (prefabMaceta == null)
                {
                    Debug.LogError("¡Asigna el prefabMaceta en el componente GrillaUI!");
                    return;
                }

                // 1. Instanciar la Maceta en la casilla
                GameObject nuevaMacetaObj = Instantiate(prefabMaceta, casilla.transform);
                AjustarTransformUI(nuevaMacetaObj);

                casilla.estaOcupada = true;
                MacetaUI macetaScript = nuevaMacetaObj.GetComponent<MacetaUI>();

                // 2. Si la maceta contenía una planta, instanciar el Prefab de PlantaBase dentro de la maceta
                if (data.tienePlanta && data.plantaData != null)
                {
                    GameObject prefabPlanta = GetPrefabPorTipo(data.plantaData.tipoFlor);
                    if (prefabPlanta != null)
                    {
                        GameObject nuevaPlantaObj = Instantiate(prefabPlanta, nuevaMacetaObj.transform);
                        AjustarTransformPlanta(nuevaPlantaObj);

                        PlantaBase pb = nuevaPlantaObj.GetComponent<PlantaBase>();
                        if (pb != null && macetaScript != null)
                        {
                            pb.enabled = true;
                            macetaScript.tienePlanta = true;
                            macetaScript.plantaActual = pb;

                            // Restaurar estado de crecimiento y vida
                            pb.RestaurarEstado(
                                data.plantaData.tiempoDeVidaActual,
                                data.plantaData.etapaActual,
                                data.plantaData.temporizadorCrecimiento,
                                data.plantaData.estaViva
                            );
                        }

                        // Restaurar estado de salud/riego
                        GestorSaludPlanta gsp = nuevaPlantaObj.GetComponent<GestorSaludPlanta>();
                        if (gsp != null)
                        {
                            gsp.RestaurarEstado(data.plantaData.nivelAgua, data.plantaData.temperaturaAmbiente);
                        }
                    }
                }
            }
        }
    }

    private void AjustarTransformUI(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }
    }

    private void AjustarTransformPlanta(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0f); // Pivot abajo para crecer adecuadamente
            rect.anchoredPosition = Vector2.zero;
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }
    }

    private GameObject GetPrefabPorTipo(int tipoFlor)
    {
        if (compraScript == null) return null;
        switch (tipoFlor)
        {
            case 1: return compraScript.prefabFlor1;
            case 2: return compraScript.prefabFlor2;
            case 3: return compraScript.prefabFlor3;
            case 4: return compraScript.prefabFlor4;
            default: return null;
        }
    }

    public void SetVisibilidadGrilla(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}