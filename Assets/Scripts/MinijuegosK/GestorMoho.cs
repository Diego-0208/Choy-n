using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestorMoho : MonoBehaviour
{
    [SerializeField] private GameObject prefabMoho;
    [SerializeField] private GameObject prefabEfectoLimpieza;
    [SerializeField] private Transform contenedorNodosPlanta;

    [SerializeField] private float tiempoEntreSpawns = 1.5f;

    public event System.Action Terminado;

    private List<Transform> nodosDisponibles = new List<Transform>();
    private List<Transform> nodosOcupados = new List<Transform>();

    private GestorSaludPlanta plantaObjetivo;
    private int totalMohoAEliminar;
    private int mohoGeneradoTotales = 0;
    private int mohoEliminadosTotales = 0;
    private bool juegoActivo = false;
    private Coroutine rutinaSpawns;

    public void IniciarMinijuego(GestorSaludPlanta planta)
    {
        plantaObjetivo = planta;
        mohoGeneradoTotales = 0;
        mohoEliminadosTotales = 0;
        juegoActivo = true;

        nodosDisponibles.Clear();
        nodosOcupados.Clear();

        foreach (Transform nodo in contenedorNodosPlanta)
        {
            nodosDisponibles.Add(nodo);

            foreach (Transform hijo in nodo)
            {
                Destroy(hijo.gameObject);
            }
        }

        totalMohoAEliminar = Mathf.Min(planta.CantidadMoho, nodosDisponibles.Count);

        if (rutinaSpawns != null)
        {
            StopCoroutine(rutinaSpawns);
        }

        if (totalMohoAEliminar <= 0)
        {
            TerminarMinijuego();
            return;
        }

        rutinaSpawns = StartCoroutine(RutinaGenerarMoho());
    }

    private IEnumerator RutinaGenerarMoho()
    {
        yield return new WaitForSeconds(0.5f);

        while (juegoActivo)
        {
            if (mohoGeneradoTotales < totalMohoAEliminar && nodosDisponibles.Count > 0)
            {
                GenerarMohoEnNodoAleatorio();
            }

            yield return new WaitForSeconds(tiempoEntreSpawns);
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

        if (plantaObjetivo != null)
        {
            plantaObjetivo.QuitarMoho(1);
        }

        ReproducirEfecto(itemMoho.transform.position);

        mohoEliminadosTotales++;

        if (mohoEliminadosTotales >= totalMohoAEliminar)
        {
            TerminarMinijuego();
        }
    }

    private void ReproducirEfecto(Vector3 posicion)
    {
        if (prefabEfectoLimpieza == null) return;

        GameObject efecto = Instantiate(prefabEfectoLimpieza, transform);
        efecto.transform.position = posicion;
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

        Terminado?.Invoke();
    }
}