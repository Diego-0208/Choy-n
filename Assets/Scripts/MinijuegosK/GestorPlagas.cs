using System.Collections;
using UnityEngine;

public class GestorPlagas : MonoBehaviour
{
    [SerializeField] private GameObject prefabPlaga;
    
    [SerializeField] private RectTransform zonaSpawns;

    [SerializeField] private GameObject panelMinijuegos;

    [SerializeField] private float tiempoEntreSpawns = 2f;
    [SerializeField] private int maxPlagasSimultaneas = 5;

    [SerializeField] private int totalBichosAEliminar = 10;
    
    private int plagasActuales = 0;
    private int bichosGeneradosTotales = 0;
    private int bichosEliminadosTotales = 0;
    private bool juegoActivo = false;
    private Coroutine rutinaSpawns;

    public void IniciarMinijuego()
    {
        plagasActuales = 0;
        bichosGeneradosTotales = 0;
        bichosEliminadosTotales = 0;
        juegoActivo = true;

        foreach (Transform hijo in zonaSpawns)
        {
            Destroy(hijo.gameObject);
        }

        if (panelMinijuegos != null)
        {
            panelMinijuegos.SetActive(true);
        }

        if (rutinaSpawns != null)
        {
            StopCoroutine(rutinaSpawns);
        }

        rutinaSpawns = StartCoroutine(RutinaGenerarPlagas());
    }

    private IEnumerator RutinaGenerarPlagas()
    {
        while (juegoActivo)
        {
            yield return new WaitForSeconds(tiempoEntreSpawns);

            if (juegoActivo && bichosGeneradosTotales < totalBichosAEliminar && plagasActuales < maxPlagasSimultaneas)
            {
                GenerarPlaga();
            }
        }
    }

    private void GenerarPlaga()
    {
        if (prefabPlaga == null || zonaSpawns == null) return;

        GameObject nuevoBicho = Instantiate(prefabPlaga, zonaSpawns);
        plagasActuales++;
        bichosGeneradosTotales++;

        RectTransform rectBicho = nuevoBicho.GetComponent<RectTransform>();
        
        float anchoPanel = zonaSpawns.rect.width;
        float altoPanel = zonaSpawns.rect.height;

        float posX = Random.Range(-anchoPanel / 2f, anchoPanel / 2f);
        float posY = Random.Range(-altoPanel / 2f, altoPanel / 2f);

        rectBicho.anchoredPosition = new Vector2(posX, posY);

        PlagaItem itemPlaga = nuevoBicho.GetComponent<PlagaItem>();
        if (itemPlaga != null)
        {
            itemPlaga.Inicializar(this);
        }
    }

    public void RegistrarPlagaEliminada()
    {
        plagasActuales = Mathf.Max(0, plagasActuales - 1);
        bichosEliminadosTotales++;

        if (bichosEliminadosTotales >= totalBichosAEliminar)
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

        foreach (Transform hijo in zonaSpawns)
        {
            Destroy(hijo.gameObject);
        }

        if (panelMinijuegos != null)
        {
            panelMinijuegos.SetActive(false);
        }
    }
}

