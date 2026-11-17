using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
public class MovimientoCanasta : MonoBehaviour,IDragHandler
{
    [Header("Refencias dentro del juego")]
    public RectTransform panelJuego; 
    public RectTransform CanastaRect;


    [Header("Limites del movimiento")]
    public float limiteIzquierdo = -430f;
    public float limiteDerecho = 430f;

    public ParticleSystem particulasCanasta; 

    public void OnDrag(PointerEventData eventData) 
    {   
        // convierte la posición del mouse o del dedo en la pantalla a coordenadas dentro del canvas. 
        Vector2 posicionLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelJuego, eventData.position, null, out posicionLocal);

        float posXLimitada = Mathf.Clamp(posicionLocal.x, limiteIzquierdo, limiteDerecho);

        float deltaX = posXLimitada - CanastaRect.anchoredPosition.x;

        //La canasta se mueve solo en el eje x manteniendo la posición fija en el y 
        CanastaRect.anchoredPosition = new Vector2(posXLimitada,CanastaRect.anchoredPosition.y);

        float inclinacionMeta = Mathf.Clamp(-deltaX * 1.5f, -20f, 20f);

        CanastaRect.DORotate(new Vector3(0, 0, inclinacionMeta), 0.12f).SetEase(Ease.OutBack);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CanastaRect.DORotate(Vector3.zero, 0.25f).SetEase(Ease.OutElastic);
    } 

    public void ReproducirEfectoCaptura() 
    {
        if (particulasCanasta != null) 
        {
            particulasCanasta.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particulasCanasta.Play(); 
        }

        if (CanastaRect != null) 
        {
            CanastaRect.DOKill(true);

            CanastaRect.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.25f, 8, 1f);

          
            CanastaRect.DOPunchAnchorPos(new Vector2(0f, 30f), 0.25f, 6, 0.5f);

         
            CanastaRect.DOPunchRotation(new Vector3(0, 0, 12f), 0.25f, 5, 1f);
        }
    }
}
