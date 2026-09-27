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

    private Compra_01 inventario;

    private void Awake()
    {
        inventario = FindFirstObjectByType<Compra_01>();
    }

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
            FlorItem_01 florScript = objetoArrastrado.GetComponent<FlorItem_01>();

            if (florScript != null)
            {
                objetoArrastrado.transform.SetParent(this.transform, false);

                RectTransform rectObjeto = objetoArrastrado.GetComponent<RectTransform>();
                rectObjeto.anchorMin = new Vector2(0.5f, 0.5f);
                rectObjeto.anchorMax = new Vector2(0.5f, 0.5f);
                rectObjeto.pivot = new Vector2(0.5f, 0.5f);
                rectObjeto.anchoredPosition = Vector2.zero;
                rectObjeto.localPosition = Vector3.zero;
                rectObjeto.localRotation = Quaternion.identity;
                rectObjeto.localScale = Vector3.one;

                estaOcupada = true;

                if (inventario != null)
                {
                    inventario.ConsumirFlor(florScript.tipoFlor);
                }

                florScript.ActivarPlantaEnGrilla();

                Debug.Log($"Planta acoplada e iniciada en casilla [{posX}, {posY}]");
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
    }
}