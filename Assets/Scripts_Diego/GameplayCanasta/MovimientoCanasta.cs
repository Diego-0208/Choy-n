using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MovimientoCanasta : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler, IEndDragHandler
{
    [Header("Referencias dentro del juego")]
    public RectTransform panelJuego; 
    public RectTransform CanastaRect;

    [Header("Limites del movimiento")]
    public float limiteIzquierdo = -430f;
    public float limiteDerecho = 430f;

    public ParticleSystem particulasCanasta; 
    public bool estaPresionadoCanasta = false; 

    public void OnPointerDown(PointerEventData eventData)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(CanastaRect, eventData.position, eventData.pressEventCamera))
        {
            estaPresionadoCanasta = true; 
        }
        else
        {
            estaPresionadoCanasta = false; 
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        estaPresionadoCanasta = false;
    }

    public void OnDrag(PointerEventData eventData) 
    {   
        if (!estaPresionadoCanasta) return; 

        Vector2 posicionLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelJuego, eventData.position, eventData.pressEventCamera, out posicionLocal);

        float posXLimitada = Mathf.Clamp(posicionLocal.x, limiteIzquierdo, limiteDerecho);

        float deltaX = posXLimitada - CanastaRect.anchoredPosition.x;

        CanastaRect.anchoredPosition = new Vector2(posXLimitada, CanastaRect.anchoredPosition.y);

        float inclinacionMeta = Mathf.Clamp(-deltaX * 1.5f, -20f, 20f);

        CanastaRect.DORotate(new Vector3(0, 0, inclinacionMeta), 0.12f).SetEase(Ease.OutBack);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        estaPresionadoCanasta = false; 
        CanastaRect.DORotate(Vector3.zero, 0.25f).SetEase(Ease.OutElastic);    
    } 

    public void ReproducirEfectoCaptura() 
    {
       
        GestorSonidos.Reproducir(Sonido.FrutaAtrapada);

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