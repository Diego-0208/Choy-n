using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class CasillaUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Coordenadas en Grilla")]
    public int posX;
    public int posY;

    [Header("Estado")]
    public bool estaOcupada = false;

    private Image imagenCasilla;

    private void Awake()
    {
        imagenCasilla = GetComponent<Image>();
    }

    public void Inicializar(int x, int y)
    {
        posX = x;
        posY = y;
        estaOcupada = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Tocaste la casilla en posición: [{posX}, {posY}]");
        
    }
}