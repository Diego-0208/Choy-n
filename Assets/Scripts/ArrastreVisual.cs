using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ArrastreVisual
{
    private GameObject fantasma;
    private RectTransform rectFantasma;
    private RectTransform rectRaiz;
    private Camera camara;
    private CanvasGroup grupoOrigen;
    private float alphaOriginal;

 
    public static bool PasarAlScrollSiEsHorizontal(GameObject origen, PointerEventData e)
    {
        ScrollRect scroll = origen.GetComponentInParent<ScrollRect>();
        if (scroll == null || !scroll.horizontal) return false;
        if (Mathf.Abs(e.delta.x) <= Mathf.Abs(e.delta.y)) return false;

        e.pointerDrag = scroll.gameObject; 
        ExecuteEvents.Execute(scroll.gameObject, e, ExecuteEvents.initializePotentialDrag);
        ExecuteEvents.Execute(scroll.gameObject, e, ExecuteEvents.beginDragHandler);
        return true;
    }

    public static ArrastreVisual Iniciar(GameObject origen, PointerEventData e, float alphaOrigenDuranteArrastre = 0.4f)
    {
        Image imagenOrigen = origen.GetComponent<Image>();
        if (imagenOrigen == null || imagenOrigen.canvas == null) return null;

        Canvas raiz = imagenOrigen.canvas.rootCanvas;
        ArrastreVisual a = new ArrastreVisual();
        a.rectRaiz = raiz.transform as RectTransform;
        a.camara = raiz.renderMode == RenderMode.ScreenSpaceOverlay ? null : raiz.worldCamera;

        a.fantasma = new GameObject("Arrastrando_" + origen.name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        a.rectFantasma = (RectTransform)a.fantasma.transform;
        a.rectFantasma.SetParent(raiz.transform, false);
        a.rectFantasma.SetAsLastSibling(); 

        Image img = a.fantasma.GetComponent<Image>();
        img.sprite = imagenOrigen.sprite;
        img.color = imagenOrigen.color;
        img.preserveAspect = imagenOrigen.preserveAspect;
        img.raycastTarget = false; 

        RectTransform rectOrigen = imagenOrigen.rectTransform;
        a.rectFantasma.sizeDelta = rectOrigen.rect.size;
        Vector3 escalaRaiz = raiz.transform.lossyScale;
        a.rectFantasma.localScale = new Vector3(
            rectOrigen.lossyScale.x / escalaRaiz.x,
            rectOrigen.lossyScale.y / escalaRaiz.y,
            1f);

        a.grupoOrigen = origen.GetComponent<CanvasGroup>();
        if (a.grupoOrigen != null)
        {
            a.alphaOriginal = a.grupoOrigen.alpha;
            a.grupoOrigen.alpha = alphaOrigenDuranteArrastre;
            a.grupoOrigen.blocksRaycasts = false;
        }

        a.Mover(e);
        return a;
    }

    public void Mover(PointerEventData e)
    {
        if (rectFantasma == null) return;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rectRaiz, e.position, camara, out Vector3 mundo))
        {
            rectFantasma.position = mundo;
        }
    }

    public void Terminar()
    {
        if (fantasma != null) Object.Destroy(fantasma);
        fantasma = null;

        if (grupoOrigen != null)
        {
            grupoOrigen.alpha = alphaOriginal;
            grupoOrigen.blocksRaycasts = true;
        }
    }
}