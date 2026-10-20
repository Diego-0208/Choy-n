using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestorMoho : MonoBehaviour
{
    [SerializeField] private GameObject prefabMoho;
    [SerializeField] private Transform contenedorNodosPlanta;
    [SerializeField] private GameObject panelMinijuegos;

    [SerializeField] private float tiempoEntreSpawns = 2.5f;
    [SerializeField] private int totalMohoAEliminar = 8;
    
    private List<Transform> nodosDisponibles = new List<Transform>();
    private List<Transform> nodosOcupados = new List<Transform>();
    
    private int mohoGeneradoTotales = 0;
    private int mohoEliminadosTotales = 0;
    private bool juegoActivo = false;
    private Coroutine rutinaSpawns;

    private void Awake()
    {
        // Cargar automáticamente los puntos hijos en la lista de disponibles
        nodosDisponibles.Clear();
        foreach (Transform hijo in contenedorNodosPlanta)
        {
            nodosDisponibles.Add(hijo);
        }
    }

    public void IniciarMinijuego()
    {
        mohoGeneradoTotales = 0;
        mohoEliminadosTotales = 0;
        juegoActivo = true;
        nodosOcupados.Clear();

        // Limpiar restos visuales previos
        foreach (Transform nodo in contenedorNodosPlanta)
        {
            foreach (Transform hijo in nodo)
            {
                Destroy(hijo.gameObject);
            }
        }

        if (panelMinijuegos != null)
        {
            panelMinijuegos.SetActive(true);
        }

        if (rutinaSpawns != null)
        {
            StopCoroutine(rutinaSpawns);
        }

        rutinaSpawns = StartCoroutine(RutinaGenerarMoho());
    }

    private IEnumerator RutinaGenerarMoho()
    {
        yield return new WaitForSeconds(1f);

        while (juegoActivo)
        {
            yield return new WaitForSeconds(tiempoEntreSpawns);

            if (juegoActivo && mohoGeneradoTotales < totalMohoAEliminar && nodosDisponibles.Count > 0)
            {
                GenerarMohoEnNodoAleatorio();
            }
        }
    }

    private void GenerarMohoEnNodoAleatorio()
    {
        if (prefabMoho == null || nodosDisponibles.Count == 0) return;

        int indiceAleatorio = Random.Range(0, nodosDisponibles.Count);
        Transform nodoElegido = nodosDisponibles[indiceAleatorio];

        GameObject nuevoMoho = Instantiate(prefabMoho, nodoElegido);
        
        RectTransform rectMoho = nuevoMoho.GetComponent<RectTransform>();
        rectMoho.anchoredPosition = Vector2.zero;

        nodosDisponibles.RemoveAt(indiceAleatorio);
        nodosOcupados.Add(nodoElegido);

        mohoGeneradoTotales++;

        MohoItem itemMoho = nuevoMoho.GetComponent<MohoItem>();
        if (itemMoho != null)
        {
            itemMoho.Inicializar(this, nodoElegido);
        }
    }

    public void RegistrarMohoEliminado(MohoItem itemMoho)
    {
        Transform nodoDelMoho = itemMoho.ObtenerNodo();

        if (nodosOcupados.Contains(nodoDelMoho))
        {
            nodosOcupados.Remove(nodoDelMoho);
            nodosDisponibles.Add(nodoDelMoho);
        }

        mohoEliminadosTotales++;

        if (mohoEliminadosTotales >= totalMohoAEliminar)
        {
            TerminarMinijuego();
        }
    }

    private void TerminarMinijuego()
    {
        juegoActivo = false;

        if (rutinaSpawns != null)
        {
            StopCoroutine(rutinaSpawns);
        }

        foreach (Transform nodo in contenedorNodosPlanta)
        {
            foreach (Transform hijo in nodo)
            {
                Destroy(hijo.gameObject);
            }
        }

        if (panelMinijuegos != null)
        {
            panelMinijuegos.SetActive(false);
        }
    }
}