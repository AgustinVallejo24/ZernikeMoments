using System.Collections.Generic;
using UnityEngine;
using System;
public class ZernikeProcessor
{
    private int _imageSize;
    private float[,] _binaryImage;

    // Inicializa el procesador con el tamaño de la imagen.
    public ZernikeProcessor(int imageSize)
    {
        _imageSize = imageSize;
        _binaryImage = new float[imageSize, imageSize];
    }

    // Dibuja el trazo del jugador en la imagen binaria.
    private void DrawLine(int x0, int y0, int x1, int y1)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = (x0 < x1) ? 1 : -1;
        int sy = (y0 < y1) ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            _binaryImage[x0, y0] = 1;

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    public void DrawStroke(List<Vector2> points)
    {
        // Limpiar
        for (int y = 0; y < _imageSize; y++)
            for (int x = 0; x < _imageSize; x++)
                _binaryImage[x, y] = 0;

        if (points.Count == 0) return;

        // Normalizar a 0–imageSize
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        foreach (var p in points)
        {
            if (p.x < minX) minX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.x > maxX) maxX = p.x;
            if (p.y > maxY) maxY = p.y;
        }
        minX -= .6f;
        minY -= .6f;
        maxX += .6f;
        maxY += .6f;

        float scaleX = (_imageSize - 1) / (maxX - minX);
        float scaleY = ((_imageSize - 1) / (maxY - minY));

        // Dibujar líneas entre puntos consecutivos
        for (int i = 0; i < points.Count - 1; i++)
        {
            int x0 = Mathf.RoundToInt((points[i].x - minX) * scaleX);
            int y0 = Mathf.RoundToInt((points[i].y - minY) * scaleY);
            int x1 = Mathf.RoundToInt((points[i + 1].x - minX) * scaleX);
            int y1 = Mathf.RoundToInt((points[i + 1].y - minY) * scaleY);

            DrawLine(
                Mathf.Clamp(x0, 0, _imageSize - 1),
                Mathf.Clamp(y0, 0, _imageSize - 1),
                Mathf.Clamp(x1, 0, _imageSize - 1),
                Mathf.Clamp(y1, 0, _imageSize - 1)
            );
        }
        ImprimirMimatriz(GetContourAndThicken(_binaryImage,_imageSize));
    }
    public float GetActivePixelCount()
    {
        float count = 0;
        for (int y = 0; y < _imageSize; y++)
        {
            for (int x = 0; x < _imageSize; x++)
            {
                count += _binaryImage[x, y];
            }
        }
        return count;
    }
    public float[,] GetContourAndThicken(float[,] binaryMatrix, int imageSize, int radius = 2)
    {
        float[,] thickened = new float[imageSize, imageSize];

        for (int y = 1; y < imageSize - 1; y++)
        {
            for (int x = 1; x < imageSize - 1; x++)
            {
                if (binaryMatrix[x, y] > 0)
                {
                    // Si tiene al menos un vecino con 0 => es contorno
                    bool isContour = false;
                    for (int ny = -1; ny <= 1; ny++)
                    {
                        for (int nx = -1; nx <= 1; nx++)
                        {
                            if (binaryMatrix[x + nx, y + ny] == 0)
                                isContour = true;
                        }
                    }

                    if (isContour)
                    {
                        // Dibujar círculo en thickened
                        for (int dy = -radius; dy <= radius; dy++)
                        {
                            for (int dx = -radius; dx <= radius; dx++)
                            {
                                int px = x + dx;
                                int py = y + dy;
                                if (px >= 0 && px < imageSize && py >= 0 && py < imageSize)
                                {
                                    if (dx * dx + dy * dy <= radius * radius)
                                    {
                                        thickened[px, py] = 1f;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return thickened;
    }
    public void DrawTexture(Texture2D texture)
    {
        if (!texture.isReadable)
        {
            Debug.LogError("La textura no es legible. Activa 'Read/Write Enabled' en la importación.");
            return;
        }

        // Limpiar matriz
        for (int y = 0; y < _imageSize; y++)
            for (int x = 0; x < _imageSize; x++)
                _binaryImage[x, y] = 0;

        // 1. Encontrar bounding box del símbolo en la textura
        int minX = texture.width, maxX = 0, minY = texture.height, maxY = 0;
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                if (texture.GetPixel(x, y).a > 0.1f) // o usar grayscale > umbral
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
            Debug.LogWarning("No se encontró símbolo en la textura");
            return;
        }

        // 2. Calcular escala para que quepa en matriz, con margen (por ejemplo 90%)
        float scale = Mathf.Min((_imageSize * 0.9f) / width, (_imageSize * 0.9f) / height);

        // 3. Calcular offsets para centrar el dibujo escalado
        float offsetX = (_imageSize - width * scale) / 2f;
        float offsetY = (_imageSize - height * scale) / 2f;

        // 4. Mapear cada pixel de la matriz a un pixel en la textura escalada
        for (int y = 0; y < _imageSize; y++)
        {
            for (int x = 0; x < _imageSize; x++)
            {
                // Mapeamos x,y a coordenadas en el bounding box original (escala inversa)
                float texX = ((x - offsetX) / scale) + minX;
                float texY = ((y - offsetY) / scale) + minY;

                // Si está fuera del bounding box, píxel negro (0)
                if (texX < minX || texX > maxX || texY < minY || texY > maxY)
                {
                    _binaryImage[x, y] = 0;
                }
                else
                {
                    // Tomamos el pixel interpolado (bilineal) para suavizar, o nearest neighbor si querés línea de 1px exacta
                    Color c = texture.GetPixel(Mathf.Clamp(Mathf.FloorToInt(texX), 0, texture.width - 1),
                                               Mathf.Clamp(Mathf.FloorToInt(texY), 0, texture.height - 1));

                    _binaryImage[x, y] = (c.a > 0.1f) ? 1f : 0f;
                }
            }
        }
        ImprimirMimatriz(_binaryImage);
    }


    void ImprimirMimatriz(float[,] Mimatriz)
    {
        string texto = "";

        for (int row = 0; row < Mimatriz.GetLength(0); row++)
        {
            for (int col = 0; col < Mimatriz.GetLength(1); col++)
            {
                texto = texto + Mimatriz[row, col] + ", ";
            }
            texto = texto + "\n";
        }

        Debug.Log(texto);

    }
    // Calcula los momentos de Zernike hasta un orden máximo `maxN`.
    public ZernikeMoment[] ComputeZernikeMoments(int maxN)
    {
        List<ZernikeMoment> moments = new List<ZernikeMoment>();
        float radius = _imageSize / 2.0f;
        int cantidad = 0;
        for (int n = 0; n <= maxN; n++)
        {
            for (int m = -n; m <= n; m++)
            {
                if ((n - Mathf.Abs(m)) % 2 == 0) // Condición de paridad para los polinomios
                {
                    float realMoment = 0f;
                    float imagMoment = 0f;

                    for (int y = 0; y < _imageSize; y++)
                    {
                        for (int x = 0; x < _imageSize; x++)
                        {
                            cantidad++;
                            if (_binaryImage[x, y] > 0)
                            {
                                // Conversión de coordenadas cartesianas a polares normalizadas
                                float xNorm = (x - radius) / radius;
                                float yNorm = (y - radius) / radius;
                                float rho = Mathf.Sqrt(xNorm * xNorm + yNorm * yNorm);

                                if (rho <= 1.0f)
                                {
                                    float theta = Mathf.Atan2(yNorm, xNorm);

                                    // Cálculo del polinomio radial
                                    float radialPoly = RadialPolynomial(n, Mathf.Abs(m), rho);

                                    // Sumatoria
                                    realMoment += radialPoly * Mathf.Cos(m * theta);
                                    imagMoment -= radialPoly * Mathf.Sin(m * theta);
                                }
                            }
                        }
                    }

                    // Cálculo de la magnitud y la fase
                    float magnitude = Mathf.Sqrt(realMoment * realMoment + imagMoment * imagMoment);
                    float phase = Mathf.Atan2(imagMoment, realMoment);
                    moments.Add(new ZernikeMoment(magnitude, phase));
                }
            }
        }
        Debug.Log(cantidad);
        return moments.ToArray();
    }

    // Implementación de los polinomios radiales R_n^m(rho)
    private float RadialPolynomial(int n, int m, float rho)
    {
        float sum = 0f;
        int s_limit = (n - m) / 2;

        for (int s = 0; s <= s_limit; s++)
        {
            float term = Mathf.Pow(-1, s) *
                         Factorial(n - s) /
                         (Factorial(s) * Factorial((n + m) / 2 - s) * Factorial((n - m) / 2 - s)) *
                         Mathf.Pow(rho, n - 2 * s);
            sum += term;
        }
        return sum;
    }

    // Función de factorial para el cálculo de los polinomios radiales
    private long Factorial(int n)
    {
        if (n < 0) return 0;
        if (n == 0) return 1;
        long result = 1;
        for (int i = 1; i <= n; i++)
        {
            result *= i;
        }
        return result;
    }
}
