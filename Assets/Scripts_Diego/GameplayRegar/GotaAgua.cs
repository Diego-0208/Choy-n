using UnityEngine;
using DG.Tweening;

public class GotaAgua : MonoBehaviour
{
    private RectTransform rectGota;
    private Regadera controladorRegadera;
    private bool haColisionado = false;

    public void Inicializar(Regadera regadera, float vel, RectTransform zona)
    {
        controladorRegadera = regadera;
        rectGota = GetComponent<RectTransform>();

        if (zona == null) return;

        Vector2 posInicial = rectGota.anchoredPosition;
        Vector2 posZona = zona.anchoredPosition;

        int tipoTrayectoria = Random.Range(-1, 2);

        float offsetHorizontalFinal = 0f;
        float offsetCurvaMedio = 0f;

        if (tipoTrayectoria == -1) 
        {
            offsetHorizontalFinal = Random.Range(-60f, -20f);
            offsetCurvaMedio = Random.Range(-50f, -80f);
        }
        else if (tipoTrayectoria == 1) 
        {
            offsetHorizontalFinal = Random.Range(20f, 60f);
            offsetCurvaMedio = Random.Range(50f, 80f);
        }
        else 
        {
            offsetHorizontalFinal = Random.Range(-10f, 10f);
            offsetCurvaMedio = Random.Range(-10f, 10f);
        }

        Vector2 posFinal = posZona + new Vector2(offsetHorizontalFinal, Random.Range(-10f, 10f));
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
                if (controladorRegadera != null)
                {
                    controladorRegadera.RegistrarImpactoGota(posFinal);
                }

               
                rectGota.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InBack).OnComplete(() => {
                    DOTween.Kill(gameObject);
                    Destroy(gameObject);
                });
            }
        });
    }

    private void OnDestroy()
    {
        DOTween.Kill(gameObject);
    }
}