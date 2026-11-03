using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Minijuego de trasplante: el jugador debe seguir con el dedo/mouse una línea de puntos
/// (la forma de las raíces) sin salirse. Al terminar avisa con un callback (true = éxito).
///
/// Montaje: este script va en un objeto con un Image transparente (Raycast Target activado)
/// que cubra el área del minijuego. Cada "forma de raíz" es un RectTransform hijo cuyos hijos
/// son los puntos, en orden (el primero es el inicio, el último es el final).
/// </summary>
public class MinijuegoTrasplanteUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Panel Minijuegos (opcional, el mismo que usan Plagas y Moho)")]
    [SerializeField] private GameObject panelMinijuegos;

    [Header("Formas de raíces (cada una con sus puntos como hijos, en orden)")]
    [SerializeField] private RectTransform[] formasRaices;

    [Header("Modo de juego")]
    [Tooltip("ON: hay que hacer clic en los puntos en orden y van desapareciendo. OFF: hay que arrastrar siguiendo la línea.")]
    [SerializeField] private bool modoClicPuntos = true;

    [Header("Tolerancia")]
    [SerializeField] private float radioPunto = 35f;       // qué tan cerca hay que estar de un punto para "alcanzarlo"
    [SerializeField] private float toleranciaLinea = 30f;  // qué tan lejos de la línea se puede salir
    [SerializeField] private int erroresPermitidos = 3;    // al llegar a este número, pierdes

    [Header("Feedback visual")]
    [SerializeField] private Image rastroPrefab;           // puntito pequeño que se dibuja al arrastrar
    [SerializeField] private float espaciadoRastro = 8f;
    [SerializeField] private Color colorPendiente = Color.white;
    [SerializeField] private Color colorAlcanzado = Color.green;
    [SerializeField] private Color colorError = Color.red;
    [SerializeField] private bool resaltarSiguiente = true; // modo clic: pinta el punto que toca
    [SerializeField] private Color colorSiguiente = Color.yellow;

    // Eventos opcionales para conectar textos de errores, sonidos, etc.
    public event Action<int, int> AlCambiarErrores; // (erroresActuales, erroresPermitidos)

    private readonly List<RectTransform> puntos = new List<RectTransform>();
    private readonly List<GameObject> rastro = new List<GameObject>();
    private RectTransform formaActual;
    private Action<bool> alTerminar;

    private int indiceSiguiente;   // siguiente punto que hay que alcanzar
    private int errores;
    private bool dibujando;
    private bool activo;
    private Vector2 ultimoPuntoRastro;

    // ---------------------------------------------------------------
    //  API pública
    // ---------------------------------------------------------------

    /// <summary>Abre el minijuego. alTerminar(true) = éxito, alTerminar(false) = fallo.</summary>
    public void Iniciar(Action<bool> alTerminar)
    {
        this.alTerminar = alTerminar;
        if (panelMinijuegos != null) panelMinijuegos.SetActive(true);
        gameObject.SetActive(true);

        ElegirForma();
        errores = 0;
        AlCambiarErrores?.Invoke(errores, erroresPermitidos);
        ReiniciarProgreso();
        activo = true;
    }

    /// <summary>
    /// Para el OnClick de un botón: abre el panel y arranca el minijuego (igual que IniciarMinijuego
    /// de Plagas y Moho). Por ahora solo muestra el resultado en la consola.
    /// </summary>
    public void IniciarMinijuego()
    {
        Iniciar(exito => Debug.Log(exito ? "Trasplante: ¡ganaste!" : "Trasplante: perdiste."));
    }

    /// <summary>Cancela sin éxito ni fallo (por ejemplo, botón "Cancelar").</summary>
    public void Cancelar()
    {
        if (!activo) return;
        Terminar(false);
    }

    // ---------------------------------------------------------------
    //  Input
    // ---------------------------------------------------------------

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!activo || puntos.Count < 2) return;

        Vector2 local = AlocalDelContenedor(eventData);

        if (modoClicPuntos)
        {
            ManejarClic(local);
            return;
        }

        // Solo se empieza si tocas el primer punto
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

        // ¿Te saliste de la línea? -> error (como cuando se rompe la galleta)
        if (DistanciaASegmento(local, a, b) > toleranciaLinea)
        {
            RegistrarError();
            return;
        }

        // Dibujar rastro
        if (Vector2.Distance(local, ultimoPuntoRastro) >= espaciadoRastro)
        {
            AgregarRastro(local);
            ultimoPuntoRastro = local;
        }

        // ¿Llegaste al siguiente punto?
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
        // Soltar antes de terminar: se reinicia el trazo, sin contar error
        if (activo && dibujando)
        {
            ReiniciarProgreso();
        }
    }

    // ---------------------------------------------------------------
    //  Modo clic: tocar los puntos en orden, cada uno desaparece
    // ---------------------------------------------------------------

    private void ManejarClic(Vector2 local)
    {
        if (indiceSiguiente >= puntos.Count) return;

        // ¿Tocó el punto que correspondía?
        if (Vector2.Distance(local, PosLocal(puntos[indiceSiguiente])) <= radioPunto)
        {
            puntos[indiceSiguiente].gameObject.SetActive(false);
            indiceSiguiente++;
            ResaltarSiguiente();

            if (indiceSiguiente >= puntos.Count) Terminar(true);
            return;
        }

        // ¿Tocó un punto que todavía no era su turno? -> error
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

    // ---------------------------------------------------------------
    //  Lógica interna
    // ---------------------------------------------------------------

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

        // Parpadeo rojo en el punto donde ibas
        Pintar(puntos[Mathf.Clamp(indiceSiguiente, 0, puntos.Count - 1)], colorError);

        if (errores >= erroresPermitidos)
        {
            Terminar(false);
        }
        else
        {
            ReiniciarProgreso(); // vuelves a empezar desde el primer punto
        }
    }

    private void Terminar(bool exito)
    {
        activo = false;
        dibujando = false;
        ReiniciarProgreso();
        gameObject.SetActive(false);
        if (panelMinijuegos != null) panelMinijuegos.SetActive(false);

        Action<bool> cb = alTerminar;
        alTerminar = null;
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

    // Convierte la posición del puntero a coordenadas locales de la forma actual
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