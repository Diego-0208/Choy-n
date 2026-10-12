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
            // Solo se permite soltar objetos de tipo MacetaUI
            MacetaUI macetaScript = objetoArrastrado.GetComponent<MacetaUI>();

            if (macetaScript != null)
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

                GrillaUI grilla = FindFirstObjectByType<GrillaUI>();
                if (grilla != null)
                {
                    grilla.GuardarEstadoGrilla();
                }

                Debug.Log($"Maceta colocada en casilla [{posX}, {posY}]");
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

        // Pasa 'this' para enviarle esta casilla específica al menú
        if (MenuCuidadosUI.Instance != null)
        {
            MenuCuidadosUI.Instance.MostrarPanel(this);
        }
    }
}