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

       
        rectGota.anchoredPosition -= new Vector2(0, velocidad * Time.deltaTime);

   
        if (zonaColision != null)
        {
            Rect rectGotaMundo = GetWorldRect(rectGota);
            Rect rectMacetaMundo = GetWorldRect(zonaColision);

            if (rectGotaMundo.Overlaps(rectMacetaMundo))
            {
                haColisionado = true;
                if (controladorRegadera != null)
                {
                  
                    controladorRegadera.RegistrarImpactoGota(rectGota.anchoredPosition);
                }
                Destroy(gameObject); 
                return;
            }
        }

       
        if (rectGota.anchoredPosition.y < -900f)
            Destroy(gameObject);
    }

  
    Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        float width = corners[3].x - corners[0].x;
        float height = corners[1].y - corners[0].y;
        return new Rect(corners[0].x, corners[0].y, width, height);
    }
}