using UnityEngine;

public class GotaAgua : MonoBehaviour
{
    private float velocidad;
    private RectTransform rectGota;
    private RectTransform zonaColision;
    private Regadera controladorRegadera;
    private bool haColisionado = false;

    public void Inicializar(Regadera regadera, float vel, RectTransform zona)
    {
        controladorRegadera = regadera;
        velocidad = vel;
        zonaColision = zona;
        rectGota = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (haColisionado) return;

        // La gota cae restando posición en el eje Y
        rectGota.anchoredPosition -= new Vector2(0, velocidad * Time.deltaTime);

        // Detección de colisión precisa mediante coordenadas locales del contenedor UI
        if (zonaColision != null)
        {
            // Comprobamos si las rects se solapan en el mismo espacio del panel
            Rect rectGotaMundo = GetWorldRect(rectGota);
            Rect rectMacetaMundo = GetWorldRect(zonaColision);

            if (rectGotaMundo.Overlaps(rectMacetaMundo))
            {
                haColisionado = true;
                if (controladorRegadera != null)
                {
                    controladorRegadera.RegistrarImpactoGota();
                }
                Destroy(gameObject); // Destruye la gota al hacer contacto
                return;
            }
        }

        // Si la gota cae por debajo de la pantalla, se destruye para limpiar memoria
        if (rectGota.anchoredPosition.y < -900f)
            Destroy(gameObject);
    }

    // Método auxiliar para calcular los límites reales en pantalla de cualquier RectTransform de UI
    Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        // Creamos un Rect basado en la esquina inferior izquierda y el tamaño
        float width = corners[3].x - corners[0].x;
        float height = corners[1].y - corners[0].y;
        return new Rect(corners[0].x, corners[0].y, width, height);
    }
}