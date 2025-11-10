using System.Collections.Generic;
using UnityEngine;

public static class SymbolRasterizer
{
    // Clears the image by setting all pixels to zero
    public static void ClearImage(float[,] image, int imageSize)
    {
        for (int y = 0; y < imageSize; y++)
            for (int x = 0; x < imageSize; x++)
                image[x, y] = 0;
    }

    // Draws a list of strokes into the image
    public static void DrawStrokes(float[,] image, int imageSize, List<List<Vector2>> allStrokes)
    {
        if (allStrokes.Count == 0) return;

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        // Find stroke boundaries
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

        minX -= .6f; minY -= .6f;
        maxX += .6f; maxY += .6f;

        // Compute scale and offsets
        float scale = Mathf.Min((imageSize - 1) / (maxX - minX), (imageSize - 1) / (maxY - minY));
        float offsetX = (imageSize - (maxX - minX) * scale) / 2f;
        float offsetY = (imageSize - (maxY - minY) * scale) / 2f;

        // Draw each stroke
        foreach (var stroke in allStrokes)
        {
            for (int i = 0; i < stroke.Count - 1; i++)
            {
                int x0 = Mathf.RoundToInt((stroke[i].x - minX) * scale + offsetX);
                int y0 = Mathf.RoundToInt((stroke[i].y - minY) * scale + offsetY);
                int x1 = Mathf.RoundToInt((stroke[i + 1].x - minX) * scale + offsetX);
                int y1 = Mathf.RoundToInt((stroke[i + 1].y - minY) * scale + offsetY);
                DrawAALine(image, imageSize, x0, y0, x1, y1);
            }
        }
    }

    // Draws a texture into the image
    public static void DrawTexture(float[,] image, int imageSize, Texture2D texture)
    {
        if (!texture.isReadable)
        {
            Debug.LogError("Texture is not readable.");
            return;
        }

        // Find bounding box
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

        if (width <= 0 || height <= 0)
        {
            Debug.LogWarning("No symbol found in texture.");
            return;
        }

        // Compute scale and offsets
        float scale = Mathf.Min((imageSize * 0.9f) / width, (imageSize * 0.9f) / height);
        float offsetX = (imageSize - width * scale) / 2f;
        float offsetY = (imageSize - height * scale) / 2f;

        // Map texture pixels to the target image
        for (int y = 0; y < imageSize; y++)
        {
            for (int x = 0; x < imageSize; x++)
            {
                float texX = ((x - offsetX) / scale) + minX;
                float texY = ((y - offsetY) / scale) + minY;

                if (texX < minX || texX > maxX || texY < minY || texY > maxY)
                {
                    image[x, y] = 0;
                }
                else
                {
                    Color c = texture.GetPixel(
                        Mathf.Clamp(Mathf.FloorToInt(texX), 0, texture.width - 1),
                        Mathf.Clamp(Mathf.FloorToInt(texY), 0, texture.height - 1)
                    );
                    image[x, y] = (c.a > 0.1f) ? 1f : 0f;
                }
            }
        }
    }

    // Draws an anti-aliased line using Xiaolin Wu’s algorithm
    private static void DrawAALine(float[,] image, int imageSize, int x0, int y0, int x1, int y1)
    {
        x0 = Mathf.Clamp(x0, 0, imageSize - 1);
        y0 = Mathf.Clamp(y0, 0, imageSize - 1);
        x1 = Mathf.Clamp(x1, 0, imageSize - 1);
        y1 = Mathf.Clamp(y1, 0, imageSize - 1);

        bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
        if (steep)
        {
            int temp = x0; x0 = y0; y0 = temp;
            temp = x1; x1 = y1; y1 = temp;
        }
        if (x0 > x1)
        {
            int temp = x0; x0 = x1; x1 = temp;
            temp = y0; y0 = y1; y1 = temp;
        }

        int dx = x1 - x0;
        int dy = y1 - y0;
        float gradient = dx == 0 ? 1f : (float)dy / dx;

        float xEnd = x0;
        float yEnd = y0;
        float xGap = 1f - (x0 + 0.5f - Mathf.Floor(x0 + 0.5f));
        int xPx1 = x0;
        int yPx1 = Mathf.FloorToInt(yEnd);
        float intensity1 = (1f - Mathf.Abs(yEnd - yPx1)) * xGap;
        float intensity2 = Mathf.Abs(yEnd - yPx1) * xGap;

        if (steep)
        {
            SetPixelSafe(image, imageSize, yPx1, xPx1, intensity1);
            SetPixelSafe(image, imageSize, yPx1 + (yEnd > yPx1 ? 1 : -1), xPx1, intensity2);
        }
        else
        {
            SetPixelSafe(image, imageSize, xPx1, yPx1, intensity1);
            SetPixelSafe(image, imageSize, xPx1, yPx1 + (yEnd > yPx1 ? 1 : -1), intensity2);
        }

        float intery = yEnd + gradient;
        for (int x = x0 + 1; x < x1; x++)
        {
            int y = Mathf.FloorToInt(intery);
            float intensity = 1f - Mathf.Abs(intery - y);
            if (steep)
            {
                SetPixelSafe(image, imageSize, y, x, intensity);
                SetPixelSafe(image, imageSize, y + (intery > y ? 1 : -1), x, 1f - intensity);
            }
            else
            {
                SetPixelSafe(image, imageSize, x, y, intensity);
                SetPixelSafe(image, imageSize, x, y + (intery > y ? 1 : -1), 1f - intensity);
            }
            intery += gradient;
        }

        xEnd = x1;
        yEnd = y1;
        xGap = x1 + 0.5f - Mathf.Floor(x1 + 0.5f);
        int xPx2 = x1;
        int yPx2 = Mathf.FloorToInt(yEnd);
        intensity1 = (1f - Mathf.Abs(yEnd - yPx2)) * xGap;
        intensity2 = Mathf.Abs(yEnd - yPx2) * xGap;

        if (steep)
        {
            SetPixelSafe(image, imageSize, yPx2, xPx2, intensity1);
            SetPixelSafe(image, imageSize, yPx2 + (yEnd > yPx2 ? 1 : -1), xPx2, intensity2);
        }
        else
        {
            SetPixelSafe(image, imageSize, xPx2, yPx2, intensity1);
            SetPixelSafe(image, imageSize, xPx2, yPx2 + (yEnd > yPx2 ? 1 : -1), intensity2);
        }
    }

    // Safely sets a pixel value with intensity clamped between 0 and 1
    private static void SetPixelSafe(float[,] image, int imageSize, int x, int y, float intensity)
    {
        if (x >= 0 && x < imageSize && y >= 0 && y < imageSize)
            image[x, y] = Mathf.Min(image[x, y] + intensity, 1f);
    }
}
