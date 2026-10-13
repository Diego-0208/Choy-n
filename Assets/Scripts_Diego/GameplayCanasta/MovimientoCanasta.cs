using UnityEngine;
using UnityEngine.EventSystems;
public class MovimientoCanasta : MonoBehaviour,IDragHandler
{
    [Header("Refencias dentro del juego")]
    public RectTransform panelJuego; 
    public RectTransform CanastaRect;

    [Header("Limites del movimiento")]
    public float limiteIzquierdo = -430f;
    public float limiteDerecho = 430f;     

    public void OnDrag(PointerEventData eventData) 
    {   
        // convierte la posición del mouse o del dedo en la pantalla a coordenadas dentro del canvas. 
        Vector2 posicionLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelJuego, eventData.position, null, out posicionLocal);

        float posXLimitada = Mathf.Clamp(posicionLocal.x, limiteIzquierdo, limiteDerecho);

        //La canasta se mueve solo en el eje x manteniendo la posición fija en el y 
        CanastaRect.anchoredPosition = new Vector2(posXLimitada,CanastaRect.anchoredPosition.y); 
    }

}
