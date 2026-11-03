using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class CasillaUI : MonoBehaviour, IPointerClickHandler, IDropHandler
{
    [Header("Coordenadas en Grilla")]
    public int posX;
    public int posY;

    [Header("Estado")]
    public bool estaOcupada = false;

    public void Inicializar(int x, int y)
    {
        posX = x;
        posY = y;
        estaOcupada = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject objetoArrastrado = eventData.pointerDrag;

        if (objetoArrastrado != null && !estaOcupada)
        {
            MacetaUI macetaScript = objetoArrastrado.GetComponent<MacetaUI>();

            if (macetaScript != null)
            {
                macetaScript.SoltarEnCasilla(this);
            }
            else
            {
                Debug.LogWarning("¡Debes colocar una maceta en la casilla antes de plantar!");
            }
        }
    }

    public void VaciarCasilla()
    {
        estaOcupada = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Tocaste la casilla en posición: [{posX}, {posY}]");

        if (MenuCuidadosUI.Instance != null)
        {
            MacetaUI maceta = GetComponentInChildren<MacetaUI>();
            bool hayPlanta = maceta != null && maceta.tienePlanta && maceta.plantaActual != null;
            if (!hayPlanta)
            {
                if (MenuCuidadosUI.Instance != null) MenuCuidadosUI.Instance.OcultarPanel();
                return;
            }
            MenuCuidadosUI.Instance.MostrarPanel(this);
        }
    }
}