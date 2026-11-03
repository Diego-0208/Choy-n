using UnityEngine;
using UnityEngine.EventSystems;

public class ZonaPresionFertilizante : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private ControlFertilizante control;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (control != null) control.OnPointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (control != null) control.OnPointerUp(eventData);
    }
}