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

        // Decidir el tipo de trayectoria simulando tus trazos:
        // -1: Arco hacia la izquierda
        //  0: Caída casi recta al centro
        //  1: Arco hacia la derecha
        int tipoTrayectoria = Random.Range(-1, 2);

        float offsetHorizontalFinal = 0f;
        float offsetCurvaMedio = 0f;

        if (tipoTrayectoria == -1) // Curva hacia la izquierda
        {
            offsetHorizontalFinal = Random.Range(-70f, -25f);
            offsetCurvaMedio = Random.Range(-60f, -90f); // Se abren hacia afuera en el punto medio
        }
        else if (tipoTrayectoria == 1) // Curva hacia la derecha
        {
            offsetHorizontalFinal = Random.Range(25f, 70f);
            offsetCurvaMedio = Random.Range(60f, 90f); // Se abren hacia afuera en el punto medio
        }
        else // Casi recto al centro
        {
            offsetHorizontalFinal = Random.Range(-15f, 15f);
            offsetCurvaMedio = Random.Range(-15f, 15f);
        }

        // Posición de destino final alrededor de la planta
        Vector2 posFinal = posPlanta + new Vector2(offsetHorizontalFinal, Random.Range(-15f, 15f));

        // Punto intermedio desplazado lateralmente para formar el arco parabólico
        Vector2 posMedia = Vector2.Lerp(posInicial, posFinal, 0.5f) + new Vector2(offsetCurvaMedio, 0f);

        float distanciaTotal = Vector2.Distance(posInicial, posMedia) + Vector2.Distance(posMedia, posFinal);
        float duracion = distanciaTotal / vel;

        // 1. Crear una secuencia en arco usando DOAnchorPos (100% seguro para UI)
        Sequence secuenciaMovimiento = DOTween.Sequence().SetTarget(gameObject);

        // Primera mitad hacia el punto medio del arco
        secuenciaMovimiento.Append(rectGota.DOAnchorPos(posMedia, duracion * 0.5f).SetEase(Ease.OutQuad));
        // Segunda mitad desde el punto medio hasta la planta
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

        // 2. Rotación continua de 360 grados en paralelo mientras viaja en arco
        rectGota.DORotate(new Vector3(0, 0, 360f), 0.5f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear)
            .SetTarget(gameObject);
    }

    private void OnDestroy()
    {
        // Limpieza de seguridad para evitar errores de DOTween
        DOTween.Kill(gameObject);
    }
}