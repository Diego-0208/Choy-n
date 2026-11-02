using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImagenPlantaMinijuego : MonoBehaviour
{
    [SerializeField] private bool recortarAlContenido = true;
    [SerializeField] private int margenPixeles = 6;
    [SerializeField] private int umbralAlpha = 10;

    private static readonly Dictionary<Sprite, Sprite> cacheRecortes = new Dictionary<Sprite, Sprite>();

    private Image imagen;

    public void Aplicar(Sprite sprite)
    {
        if (sprite == null) return;
        if (imagen == null) imagen = GetComponent<Image>();

        imagen.sprite = recortarAlContenido ? ObtenerRecorte(sprite) : sprite;
        imagen.preserveAspect = true;
    }

    private Sprite ObtenerRecorte(Sprite original)
    {
        if (cacheRecortes.TryGetValue(original, out Sprite guardado) && guardado != null)
        {
            return guardado;
        }

        Sprite recorte = CrearRecorte(original);
        cacheRecortes[original] = recorte;
        return recorte;
    }

    private Sprite CrearRecorte(Sprite original)
    {
        Texture2D textura = original.texture;

        if (!textura.isReadable)
        {
            Debug.LogWarning($"La textura {textura.name} no tiene Read/Write activado, se usa la imagen completa.");
            return original;
        }

        Rect zona = original.textureRect;
        int x0 = Mathf.RoundToInt(zona.x);
        int y0 = Mathf.RoundToInt(zona.y);
        int ancho = Mathf.RoundToInt(zona.width);
        int alto = Mathf.RoundToInt(zona.height);
        int anchoTextura = textura.width;

        Color32[] pixeles = textura.GetPixels32();

        int minX = ancho;
        int minY = alto;
        int maxX = -1;
        int maxY = -1;

        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                if (pixeles[(y0 + y) * anchoTextura + (x0 + x)].a > umbralAlpha)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (maxX < 0) return original;

        minX = Mathf.Max(0, minX - margenPixeles);
        minY = Mathf.Max(0, minY - margenPixeles);
        maxX = Mathf.Min(ancho - 1, maxX + margenPixeles);
        maxY = Mathf.Min(alto - 1, maxY + margenPixeles);

        Rect rectRecorte = new Rect(x0 + minX, y0 + minY, maxX - minX + 1, maxY - minY + 1);

        return Sprite.Create(textura, rectRecorte, new Vector2(0.5f, 0.5f), original.pixelsPerUnit, 0, SpriteMeshType.FullRect);
    }
}