using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinijuegoTrasplanteUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Formas de raíces (cada una con sus puntos como hijos, en orden)")]
    [SerializeField] private RectTransform[] formasRaices;

    [Header("Modo de juego")]
    [Tooltip("ON: hay que hacer clic en los puntos en orden y van desapareciendo. OFF: hay que arrastrar siguiendo la línea.")]
    [SerializeField] private bool modoClicPuntos = true;

    [Header("Tolerancia")]
    [SerializeField] private float radioPunto = 35f;
    [SerializeField] private float toleranciaLinea = 30f;
    [SerializeField] private int erroresPermitidos = 3;

    [Header("Feedback visual")]
    [SerializeField] private Image rastroPrefab;
    [SerializeField] private float espaciadoRastro = 8f;
    [SerializeField] private Color colorPendiente = Color.white;
    [SerializeField] private Color colorAlcanzado = Color.green;
    [SerializeField] private Color colorError = Color.red;
    [SerializeField] private bool resaltarSiguiente = true;
    [SerializeField] private Color colorSiguiente = Color.yellow;

    public event Action<int, int> AlCambiarErrores;
    public event Action Terminado;

    private readonly List<RectTransform> puntos = new List<RectTransform>();
    private readonly List<GameObject> rastro = new List<GameObject>();
    private RectTransform formaActual;
    private Action<bool> alTerminar;

    private int indiceSiguiente;
    private int errores;
    private bool dibujando;
    private bool activo;
    private Vector2 ultimoPuntoRastro;

    public void Iniciar(Action<bool> alTerminar)
    {
        this.alTerminar = alTerminar;

        ElegirForma();
        errores = 0;
        AlCambiarErrores?.Invoke(errores, erroresPermitidos);
        ReiniciarProgreso();
        activo = true;
    }

    public void Cancelar()
    {
        if (!activo) return;
        Terminar(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!activo || puntos.Count < 2) return;

        Vector2 local = AlocalDelContenedor(eventData);

        if (modoClicPuntos)
        {
            ManejarClic(local);
            return;
        }

        if (Vector2.Distance(local, PosLocal(puntos[0])) <= radioPunto)
        {
            dibujando = true;
            indiceSiguiente = 1;
            Pintar(puntos[0], colorAlcanzado);
            ultimoPuntoRastro = local;
            AgregarRastro(local);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!activo || !dibujando) return;

        Vector2 local = AlocalDelContenedor(eventData);

        Vector2 a = PosLocal(puntos[indiceSiguiente - 1]);
        Vector2 b = PosLocal(puntos[indiceSiguiente]);

        if (DistanciaASegmento(local, a, b) > toleranciaLinea)
        {
            RegistrarError();
            return;
        }

        if (Vector2.Distance(local, ultimoPuntoRastro) >= espaciadoRastro)
        {
            AgregarRastro(local);
            ultimoPuntoRastro = local;
        }

        if (Vector2.Distance(local, b) <= radioPunto)
        {
            Pintar(puntos[indiceSiguiente], colorAlcanzado);
            indiceSiguiente++;

            if (indiceSiguiente >= puntos.Count)
            {
                Terminar(true);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (activo && dibujando)
        {
            ReiniciarProgreso();
        }
    }

    private void ManejarClic(Vector2 local)
    {
        if (indiceSiguiente >= puntos.Count) return;

        if (Vector2.Distance(local, PosLocal(puntos[indiceSiguiente])) <= radioPunto)
        {
            puntos[indiceSiguiente].gameObject.SetActive(false);
            indiceSiguiente++;
            ResaltarSiguiente();

            if (indiceSiguiente >= puntos.Count) Terminar(true);
            return;
        }

        for (int i = indiceSiguiente + 1; i < puntos.Count; i++)
        {
            if (Vector2.Distance(local, PosLocal(puntos[i])) <= radioPunto)
            {
                errores++;
                AlCambiarErrores?.Invoke(errores, erroresPermitidos);
                StartCoroutine(ParpadeoError(puntos[i]));

                if (errores >= erroresPermitidos) Terminar(false);
                return;
            }
        }
    }

    private void ResaltarSiguiente()
    {
        if (!modoClicPuntos || !resaltarSiguiente) return;
        if (indiceSiguiente >= 0 && indiceSiguiente < puntos.Count)
            Pintar(puntos[indiceSiguiente], colorSiguiente);
    }

    private IEnumerator ParpadeoError(RectTransform punto)
    {
        Pintar(punto, colorError);
        yield return new WaitForSeconds(0.25f);
        if (punto != null) Pintar(punto, colorPendiente);
    }

    private void ElegirForma()
    {
        for (int i = 0; i < formasRaices.Length; i++)
            formasRaices[i].gameObject.SetActive(false);

        formaActual = formasRaices[UnityEngine.Random.Range(0, formasRaices.Length)];
        formaActual.gameObject.SetActive(true);

        puntos.Clear();
        foreach (Transform hijo in formaActual)
        {
            if (hijo.gameObject.activeSelf)
                puntos.Add((RectTransform)hijo);
        }
    }

    private void ReiniciarProgreso()
    {
        dibujando = false;
        indiceSiguiente = 0;

        foreach (RectTransform p in puntos)
        {
            p.gameObject.SetActive(true);
            Pintar(p, colorPendiente);
        }
        ResaltarSiguiente();

        foreach (GameObject r in rastro)
            if (r != null) Destroy(r);
        rastro.Clear();
    }

    private void RegistrarError()
    {
        errores++;
        AlCambiarErrores?.Invoke(errores, erroresPermitidos);

        Pintar(puntos[Mathf.Clamp(indiceSiguiente, 0, puntos.Count - 1)], colorError);

        if (errores >= erroresPermitidos)
        {
            Terminar(false);
        }
        else
        {
            ReiniciarProgreso();
        }
    }

    private void Terminar(bool exito)
    {
        activo = false;
        dibujando = false;
        ReiniciarProgreso();

        Action<bool> cb = alTerminar;
        alTerminar = null;

        Terminado?.Invoke();
        cb?.Invoke(exito);
    }

    private void AgregarRastro(Vector2 posLocal)
    {
        if (rastroPrefab == null) return;

        Image img = Instantiate(rastroPrefab, formaActual);
        img.raycastTarget = false;
        img.rectTransform.localPosition = posLocal;
        rastro.Add(img.gameObject);
    }

    private void Pintar(RectTransform punto, Color color)
    {
        Image img = punto.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    private Vector2 AlocalDelContenedor(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            formaActual, eventData.position, eventData.pressEventCamera, out Vector2 local);
        return local;
    }

    private Vector2 PosLocal(RectTransform punto)
    {
        return formaActual.InverseTransformPoint(punto.position);
    }

    private static float DistanciaASegmento(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float largo2 = ab.sqrMagnitude;
        if (largo2 < 0.0001f) return Vector2.Distance(p, a);

        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / largo2);
        return Vector2.Distance(p, a + ab * t);
    }
}