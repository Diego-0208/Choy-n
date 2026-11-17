using UnityEngine;
using DG.Tweening;

public class GotaFertilizante : MonoBehaviour
{
    private RectTransform rectGota;
    private ControlFertilizante controlador;
    private bool haColisionado = false;

    public void Inicializar(ControlFertilizante ctrl, float vel, RectTransform planta, bool acierta)
    {
        controlador = ctrl;
        rectGota = GetComponent<RectTransform>();

        if (planta == null) return;

        Vector2 posInicial = rectGota.anchoredPosition;
        Vector2 posPlanta = planta.anchoredPosition;

       
        int tipoTrayectoria = Random.Range(-1, 2);

        float offsetHorizontalFinal = 0f;
        float offsetCurvaMedio = 0f;

        if (tipoTrayectoria == -1) 
        {
            offsetHorizontalFinal = Random.Range(-70f, -25f);
            offsetCurvaMedio = Random.Range(-60f, -90f); 
        }
        else if (tipoTrayectoria == 1) 
        {
            offsetHorizontalFinal = Random.Range(25f, 70f);
            offsetCurvaMedio = Random.Range(60f, 90f); 
        }
        else // Casi recto al centro
        {
            offsetHorizontalFinal = Random.Range(-15f, 15f);
            offsetCurvaMedio = Random.Range(-15f, 15f);
        }

        
        Vector2 posFinal = posPlanta + new Vector2(offsetHorizontalFinal, Random.Range(-15f, 15f));

        
        Vector2 posMedia = Vector2.Lerp(posInicial, posFinal, 0.5f) + new Vector2(offsetCurvaMedio, 0f);

        float distanciaTotal = Vector2.Distance(posInicial, posMedia) + Vector2.Distance(posMedia, posFinal);
        float duracion = distanciaTotal / vel;

        
        Sequence secuenciaMovimiento = DOTween.Sequence().SetTarget(gameObject);

        
        secuenciaMovimiento.Append(rectGota.DOAnchorPos(posMedia, duracion * 0.5f).SetEase(Ease.OutQuad));
        
        secuenciaMovimiento.Append(rectGota.DOAnchorPos(posFinal, duracion * 0.5f).SetEase(Ease.InQuad));

        secuenciaMovimiento.OnComplete(() => {
            if (!haColisionado)
            {
                haColisionado = true;
                if (controlador != null)
                {
                    controlador.RegistrarImpactoExitoso();
                }

                DOTween.Kill(gameObject);
                Destroy(gameObject);
            }
        });

        
        rectGota.DORotate(new Vector3(0, 0, 360f), 0.5f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear)
            .SetTarget(gameObject);
    }

    private void OnDestroy()
    {
       
        DOTween.Kill(gameObject);
    }
}