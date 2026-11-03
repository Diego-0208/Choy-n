using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EfectoMinijuegoUI : MonoBehaviour
{
    [SerializeField] private float duracionGolpe = 0.18f;
    [SerializeField] private float duracionPermanencia = 0.15f;
    [SerializeField] private float duracionDesvanecer = 0.2f;

    [SerializeField] private float anguloInicial = -70f;
    [SerializeField] private float anguloFinal = 0f;
    [SerializeField] private float escalaInicial = 0.8f;
    [SerializeField] private float escalaFinal = 1.1f;

    private IEnumerator Start()
    {
        RectTransform rect = (RectTransform)transform;
        Image imagen = GetComponent<Image>();

        if (imagen != null) imagen.raycastTarget = false;

        rect.localRotation = Quaternion.Euler(0f, 0f, anguloInicial);
        rect.localScale = Vector3.one * escalaInicial;

        float t = 0f;
        while (t < duracionGolpe)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duracionGolpe);
            k = k * k;

            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(anguloInicial, anguloFinal, k));
            rect.localScale = Vector3.one * Mathf.Lerp(escalaInicial, escalaFinal, k);
            yield return null;
        }

        yield return new WaitForSeconds(duracionPermanencia);

        t = 0f;
        while (t < duracionDesvanecer)
        {
            t += Time.deltaTime;

            if (imagen != null)
            {
                Color color = imagen.color;
                color.a = 1f - Mathf.Clamp01(t / duracionDesvanecer);
                imagen.color = color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}