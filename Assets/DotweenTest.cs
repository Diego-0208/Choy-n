using DG.Tweening;
using UnityEngine;

public class DOTweenTest : MonoBehaviour
{
    [SerializeField] RectTransform puntoA;
    [SerializeField] RectTransform puntoB;
    [SerializeField] float duracion = 5f;
    [SerializeField] float retraso = 0f;
    [SerializeField] float pausaEntreCiclos = 3f;
    [SerializeField] int repeticiones = -1;
    [SerializeField] Ease ease = Ease.OutCubic;
    [SerializeField] bool alIniciar = true;

    RectTransform rect;
    Tween tween;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Start()
    {
        if (alIniciar) Mover();
    }

    public void Mover()
    {
        tween?.Kill();
        rect.position = puntoA.position;

        Sequence secuencia = DOTween.Sequence();
        secuencia.Append(rect.DOMove(puntoB.position, duracion).SetEase(ease));
        secuencia.AppendInterval(pausaEntreCiclos);
        secuencia.SetLoops(repeticiones, LoopType.Restart);
        secuencia.SetDelay(retraso);
        secuencia.SetLink(gameObject);

        tween = secuencia;
    }

    public void Detener()
    {
        tween?.Kill();
    }
}