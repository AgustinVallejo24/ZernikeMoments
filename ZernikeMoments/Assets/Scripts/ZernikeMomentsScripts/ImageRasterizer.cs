// FileName: ImageRasterizer.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase de utilidad estática para dibujar trazos y texturas en un objeto BinaryImage.
/// </summary>
public static class ImageRasterizer
{
    /// <summary>
    /// Dibuja una serie de trazos en la imagen, normalizando su tamaño y posición.
    /// </summary>
    public static void DrawStrokes(BinaryImage image, List<List<Vector2>> allStrokes)
    {
        image.Clear();
        if (allStrokes == null || allStrokes.Count == 0) return;

        // Encontrar límites para la normalización
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        foreach (var stroke in allStrokes)
        {
            foreach (var p in stroke)
            {
                if (p.x < minX) minX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.x > maxX) maxX = p.x;
                if (p.y > maxY) maxY = p.y;
            }
        }
        if (minX > maxX) return;

        // Añadir margen
        minX -= 0.6f; minY -= 0.6f;
        maxX += 0.6f; maxY += 0.6f;

        // Calcular escala y offset
        float scale = Mathf.Min((image.Size - 1) / (maxX - minX), (image.Size - 1) / (maxY - minY));
        float offsetX = (image.Size - (maxX - minX) * scale) / 2f;
        float offsetY = (image.Size - (maxY - minY) * scale) / 2f;

        // Dibujar cada segmento de cada trazo
        foreach (var stroke in allStrokes)
        {
            for (int i = 0; i < stroke.Count - 1; i++)
            {
                int x0 = Mathf.RoundToInt((stroke[i].x - minX) * scale + offsetX);
                int y0 = Mathf.RoundToInt((stroke[i].y - minY) * scale + offsetY);
                int x1 = Mathf.RoundToInt((stroke[i + 1].x - minX) * scale + offsetX);
                int y1 = Mathf.RoundToInt((stroke[i + 1].y - minY) * scale + offsetY);
                DrawAALine(image, x0, y0, x1, y1);
            }
        }
    }

    /// <summary>
    /// Dibuja una textura en la imagen, ajustándola al centro y escalándola.
    /// </summary>
    public static void DrawTexture(BinaryImage image, Texture2D texture)
    {
        image.Clear();
        if (!texture.isReadable)
        {
            Debug.LogError("La textura no es legible. Activa 'Read/Write Enabled' en la importación.");
            return;
        }

        int minX = texture.width, maxX = 0, minY = texture.height, maxY = 0;
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                if (texture.GetPixel(x, y).a > 0.1f)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        if (width <= 0 || height <= 0) return;

        float scale = Mathf.Min((image.Size * 0.9f) / width, (image.Size * 0.9f) / height);
        float offsetX = (image.Size - width * scale) / 2f;
        float offsetY = (image.Size - height * scale) / 2f;

        for (int y = 0; y < image.Size; y++)
        {
            for (int x = 0; x < image.Size; x++)
            {
                float texX = ((x - offsetX) / scale) + minX;
                float texY = ((y - offsetY) / scale) + minY;

                if (texX >= minX && texX <= maxX && texY >= minY && texY <= maxY)
                {
                    Color c = texture.GetPixel(Mathf.FloorToInt(texX), Mathf.FloorToInt(texY));
                    if (c.a > 0.1f)
                    {
                        image.Pixels[x, y] = 1f;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Dibuja una línea con antialiasing (Algoritmo de Xiaolin Wu).
    /// </summary>
    private static void DrawAALine(BinaryImage image, int x0, int y0, int x1, int y1)
    {
        x0 = Mathf.Clamp(x0, 0, image.Size - 1);
        y0 = Mathf.Clamp(y0, 0, image.Size - 1);
        x1 = Mathf.Clamp(x1, 0, image.Size - 1);
        y1 = Mathf.Clamp(y1, 0, image.Size - 1);

        bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
        if (steep) { (x0, y0) = (y0, x0); (x1, y1) = (y1, x1); }
        if (x0 > x1) { (x0, x1) = (x1, x0); (y0, y1) = (y1, y0); }

        int dx = x1 - x0;
        int dy = y1 - y0;
        float gradient = (dx == 0) ? 1.0f : (float)dy / dx;

        float y = y0;
        for (int x = x0; x <= x1; x++)
        {
            int iy = Mathf.FloorToInt(y);
            float fpart = y - iy;

            if (steep)
            {
                image.SetPixelSafe(iy, x, 1 - fpart);
                image.SetPixelSafe(iy + 1, x, fpart);
            }
            else
            {
                image.SetPixelSafe(x, iy, 1 - fpart);
                image.SetPixelSafe(x, iy + 1, fpart);
            }
            y += gradient;
        }
    }
}