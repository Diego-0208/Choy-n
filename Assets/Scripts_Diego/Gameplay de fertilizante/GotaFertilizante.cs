using UnityEngine;

public class GotaFertilizante : MonoBehaviour
{
    private float velocidad;
    private RectTransform rectGota;
    private RectTransform zonaPlanta;
    private ControlFertilizante controlador;
    private bool aciertaEnPlanta; 
    private bool haColisionado = false;

    public void Inicializar(ControlFertilizante ctrl, float vel, RectTransform planta, bool acierta) 
    {
        controlador = ctrl;
        velocidad = vel;
        zonaPlanta = planta;
        aciertaEnPlanta = acierta;
        rectGota = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if(haColisionado)return;

        rectGota.anchoredPosition -= new Vector2(0, velocidad * Time.deltaTime);

        if (zonaPlanta != null) 
        {
            Rect rectGotaMundo = GetWorldRect(rectGota);
            Rect rectPlantaMundo = GetWorldRect(zonaPlanta);

            if (rectGotaMundo.Overlaps(rectPlantaMundo)) 
            {
                haColisionado = true;
                if (controlador != null)
                {
                    controlador.RegistrarImpactoExitoso();
                }
            }
        }
        if (rectGota.anchoredPosition.y < -900f) 
        {
            Destroy(gameObject); 
        }
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
