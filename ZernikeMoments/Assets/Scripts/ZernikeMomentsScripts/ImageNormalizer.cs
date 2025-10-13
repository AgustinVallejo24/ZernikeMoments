// FileName: ImageNormalizer.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procesa una BinaryImage para normalizar su grosor y calcular un histograma angular.
/// Contiene el algoritmo de adelgazamiento de Zhang-Suen.
/// </summary>
public class ImageNormalizer
{
    private float[] _lastAngularHistogram;

    public float[] GetLastHistogram() => _lastAngularHistogram;
    public Texture2D ResizeSelectedTextures(Texture2D originalTexture, int targetSize)
    {
        var selectedTextures = originalTexture;



        // Crea un nuevo RenderTexture y renderiza la textura original en él
        RenderTexture rt = new RenderTexture(targetSize, targetSize, 24);
        Graphics.Blit(selectedTextures, rt);

        // Crea una nueva Texture2D y lee los píxeles del RenderTexture
        Texture2D resizedTexture = new Texture2D(targetSize, targetSize);
        RenderTexture.active = rt;
        resizedTexture.ReadPixels(new Rect(0, 0, targetSize, targetSize), 0, 0);
        resizedTexture.Apply();


        // Guarda la nueva textura redimensionada como PNG
        byte[] bytes = resizedTexture.EncodeToPNG();


        return resizedTexture;

    }

    public BinaryImage NormalizeThicknessAndComputeHistogram(BinaryImage input, int targetThickness, int sectors = 16, int rings = 3)
    {
        _lastAngularHistogram = new float[sectors * rings];
        int imageSize = input.Size;
        if (imageSize <= 0) return input;

        bool[,] skeleton = ZhangSuenThinning(ToBoolMatrix(input.Pixels, imageSize), imageSize);

        if (targetThickness <= 1)
        {
            return BoolToFloat(skeleton, imageSize);
        }

        // --- 1) Centroide del esqueleto ---
        float cx = 0f, cy = 0f;
        int skeletonCount = 0;
        for (int y = 0; y < imageSize; y++)
            for (int x = 0; x < imageSize; x++)
                if (skeleton[x, y]) { cx += x; cy += y; skeletonCount++; }

        if (skeletonCount > 0) { cx /= skeletonCount; cy /= skeletonCount; }
        else { cx = imageSize / 2f; cy = imageSize / 2f; }

        // --- 2) Engrosar esqueleto y calcular histograma ---
        BinaryImage thickenedImage = new BinaryImage(imageSize);
        int radius = Mathf.Max(0, Mathf.RoundToInt((targetThickness - 1) / 2f));
        List<Vector2Int> offsets = new List<Vector2Int>();
        int r2 = radius * radius;
        for (int dy = -radius; dy <= radius; dy++)
            for (int dx = -radius; dx <= radius; dx++)
                if (dx * dx + dy * dy <= r2)
                    offsets.Add(new Vector2Int(dx, dy));

        bool[,] visited = new bool[imageSize, imageSize];
        int totalAdded = 0;
        float maxDist = Mathf.Sqrt(2 * imageSize * imageSize) / 2f;

        for (int y = 0; y < imageSize; y++)
        {
            for (int x = 0; x < imageSize; x++)
            {
                if (!skeleton[x, y]) continue;
                foreach (var o in offsets)
                {
                    int px = x + o.x;
                    int py = y + o.y;
                    if (px < 0 || px >= imageSize || py < 0 || py >= imageSize || visited[px, py]) continue;

                    visited[px, py] = true;
                    thickenedImage.Pixels[px, py] = 1f;
                    totalAdded++;

                    // Histograma
                    float dx_h = px - cx;
                    float dy_h = py - cy;
                    float angle = (Mathf.Atan2(dy_h, dx_h) + Mathf.PI) / (2f * Mathf.PI);
                    float dist = Mathf.Sqrt(dx_h * dx_h + dy_h * dy_h);

                    int sector = Mathf.FloorToInt(angle * sectors) % sectors;
                    int ring = Mathf.Min(rings - 1, Mathf.FloorToInt(dist / maxDist * rings));
                    _lastAngularHistogram[ring * sectors + sector]++;
                }
            }
        }

        // --- 3) Normalizar histograma ---
        if (totalAdded > 0)
        {
            for (int i = 0; i < _lastAngularHistogram.Length; i++)
                _lastAngularHistogram[i] /= totalAdded;
        }

        return thickenedImage;
    }

    // --- Lógica de Zhang-Suen Thinning (privada) ---
    private bool[,] ZhangSuenThinning(bool[,] input, int size)
    {
        bool[,] img = (bool[,])input.Clone();
        List<Vector2Int> toRemove = new List<Vector2Int>();
        bool changed = true;

        while (changed)
        {
            changed = false;
            // Sub-iteración 1
            for (int y = 1; y < size - 1; y++)
            {
                for (int x = 1; x < size - 1; x++)
                {
                    if (img[x, y] && CheckConditions(img, x, y, 1)) toRemove.Add(new Vector2Int(x, y));
                }
            }
            if (toRemove.Count > 0)
            {
                changed = true;
                foreach (var p in toRemove) img[p.x, p.y] = false;
                toRemove.Clear();
            }

            // Sub-iteración 2
            for (int y = 1; y < size - 1; y++)
            {
                for (int x = 1; x < size - 1; x++)
                {
                    if (img[x, y] && CheckConditions(img, x, y, 2)) toRemove.Add(new Vector2Int(x, y));
                }
            }
            if (toRemove.Count > 0)
            {
                changed = true;
                foreach (var p in toRemove) img[p.x, p.y] = false;
                toRemove.Clear();
            }
        }
        return img;
    }

    private bool CheckConditions(bool[,] img, int x, int y, int iter)
    {
        bool p2 = img[x, y - 1], p3 = img[x + 1, y - 1], p4 = img[x + 1, y];
        bool p5 = img[x + 1, y + 1], p6 = img[x, y + 1], p7 = img[x - 1, y + 1];
        bool p8 = img[x - 1, y], p9 = img[x - 1, y - 1];

        int neighbors = (p2 ? 1 : 0) + (p3 ? 1 : 0) + (p4 ? 1 : 0) + (p5 ? 1 : 0) + (p6 ? 1 : 0) + (p7 ? 1 : 0) + (p8 ? 1 : 0) + (p9 ? 1 : 0);
        if (neighbors < 2 || neighbors > 6) return false;

        int transitions = ((!p2 && p3) ? 1 : 0) + ((!p3 && p4) ? 1 : 0) + ((!p4 && p5) ? 1 : 0) + ((!p5 && p6) ? 1 : 0) +
                          ((!p6 && p7) ? 1 : 0) + ((!p7 && p8) ? 1 : 0) + ((!p8 && p9) ? 1 : 0) + ((!p9 && p2) ? 1 : 0);
        if (transitions != 1) return false;

        if (iter == 1) return !p2 || !p4 || !p6;
        if (iter == 2) return !p2 || !p4 || !p8;
        return false;
    }

    // --- Utilidades de conversión (privadas) ---
    private bool[,] ToBoolMatrix(float[,] src, int size)
    {
        bool[,] b = new bool[size, size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                b[x, y] = src[x, y] > 0f;
        return b;
    }

    private BinaryImage BoolToFloat(bool[,] b, int size)
    {
        BinaryImage f = new BinaryImage(size);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                f.Pixels[x, y] = b[x, y] ? 1f : 0f;
        return f;
    }
}