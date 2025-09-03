using System.Collections.Generic;
using UnityEngine;
using System;
public class ZernikeProcessor
{
    private int _imageSize;
    private float[,] _binaryImage;
    float[] angularHistogram;
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

        float scale = Mathf.Min(
            (_imageSize - 1) / (maxX - minX),
            (_imageSize - 1) / (maxY - minY)
        );

        // 2. Calcular offsets para centrar el dibujo
        float offsetX = (_imageSize - (maxX - minX) * scale) / 2f;
        float offsetY = (_imageSize - (maxY - minY) * scale) / 2f;

        // 3. Dibujar líneas con esa escala y offset
        for (int i = 0; i < points.Count - 1; i++)
        {
            int x0 = Mathf.RoundToInt((points[i].x - minX) * scale + offsetX);
            int y0 = Mathf.RoundToInt((points[i].y - minY) * scale + offsetY);
            int x1 = Mathf.RoundToInt((points[i + 1].x - minX) * scale + offsetX);
            int y1 = Mathf.RoundToInt((points[i + 1].y - minY) * scale + offsetY);

            DrawLine(
                Mathf.Clamp(x0, 0, _imageSize - 1),
                Mathf.Clamp(y0, 0, _imageSize - 1),
                Mathf.Clamp(x1, 0, _imageSize - 1),
                Mathf.Clamp(y1, 0, _imageSize - 1)
            );
        }
        _binaryImage = NormalizeThicknessAndComputeAngularHistogram(_binaryImage, _imageSize, 3);
        ImprimirMimatriz(_binaryImage);
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
        _binaryImage = NormalizeThicknessAndComputeAngularHistogram(_binaryImage, _imageSize, 3);
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

    private bool[,] ToBoolMatrix(float[,] src, int size)
    {
        bool[,] b = new bool[size, size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                b[x, y] = src[x, y] > 0f;
        return b;
    }

    // Convierte bool[,] a float[,]
    private float[,] BoolToFloat(bool[,] b, int size)
    {
        float[,] f = new float[size, size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                f[x, y] = b[x, y] ? 1f : 0f;
        return f;
    }

    // Función principal: devuelve una matriz con grosor uniforme (targetThickness en píxeles)
    public float[,] NormalizeThicknessAndComputeAngularHistogram(float[,] binaryMatrix, int imageSize, int targetThickness, int sectors = 16, int rings = 3)
    {
        angularHistogram = new float[sectors * rings];

        if (imageSize <= 0) return binaryMatrix;
        if (targetThickness <= 1)
        {
            bool[,] thin = ZhangSuenThinning(ToBoolMatrix(binaryMatrix, imageSize), imageSize);
            return BoolToFloat(thin, imageSize);
        }

        bool[,] bin = ToBoolMatrix(binaryMatrix, imageSize);
        bool[,] skeleton = ZhangSuenThinning(bin, imageSize);

        // --- 1) centroide ---
        float cx = 0f, cy = 0f;
        int skeletonCount = 0;
        for (int y = 0; y < imageSize; y++)
            for (int x = 0; x < imageSize; x++)
                if (skeleton[x, y]) { cx += x; cy += y; skeletonCount++; }

        if (skeletonCount > 0)
        {
            cx /= skeletonCount;
            cy /= skeletonCount;
        }
        else { cx = imageSize / 2f; cy = imageSize / 2f; }

        // --- 2) offsets de disco ---
        int radius = Mathf.Max(0, Mathf.RoundToInt((targetThickness - 1) / 2f));
        List<Vector2Int> offsets = new List<Vector2Int>();
        int r2 = radius * radius;
        for (int dy = -radius; dy <= radius; dy++)
            for (int dx = -radius; dx <= radius; dx++)
                if (dx * dx + dy * dy <= r2)
                    offsets.Add(new Vector2Int(dx, dy));

        float[,] thickened = new float[imageSize, imageSize];
        bool[,] visited = new bool[imageSize, imageSize];
        int totalAdded = 0;

        // --- 3) expandir skeleton y llenar histograma polar ---
        float maxDist = Mathf.Sqrt(imageSize * imageSize + imageSize * imageSize) / 2f;

        for (int y = 0; y < imageSize; y++)
        {
            for (int x = 0; x < imageSize; x++)
            {
                if (!skeleton[x, y]) continue;

                foreach (var o in offsets)
                {
                    int px = x + o.x;
                    int py = y + o.y;
                    if (px < 0 || px >= imageSize || py < 0 || py >= imageSize) continue;

                    if (!visited[px, py])
                    {
                        visited[px, py] = true;
                        thickened[px, py] = 1f;
                        totalAdded++;

                        // ---- Histograma polar ----
                        float dx = px - cx;
                        float dy = py - cy;
                        float angle = Mathf.Atan2(dy, dx); // -pi..pi
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);

                        int sector = (int)((angle + Mathf.PI) / (2f * Mathf.PI) * sectors);
                        sector = (sector % sectors + sectors) % sectors;

                        int ring = Mathf.Min(rings - 1, (int)(dist / maxDist * rings));

                        int index = ring * sectors + sector;
                        angularHistogram[index] += 1f;
                    }
                }
            }
        }

        // --- 4) normalizar histograma ---
        if (totalAdded > 0)
        {
            for (int i = 0; i < angularHistogram.Length; i++)
                angularHistogram[i] /= totalAdded;
        }

        return thickened;
    }

    public float[] GetSymbolDistribution()
    {
        return angularHistogram;
    }

    public  float CompareAngularHistograms(float[] histA, float[] histB, bool rotationInvariant = false)
    {
        int sectors = 16;
        if (histA.Length != histB.Length) throw new ArgumentException("Length mismatch");
        int rings = histA.Length / sectors;

        float best = float.MaxValue;

        if (!rotationInvariant)
        {
            float sum = 0f;
            for (int i = 0; i < histA.Length; i++)
            {
                float d = histA[i] - histB[i];
                sum += d * d;
            }
            return Mathf.Sqrt(sum);
        }
        else
        {
            // probamos todos los shifts angulares, pero mantenemos los anillos fijos
            for (int shift = 0; shift < sectors; shift++)
            {
                float sum = 0f;
                for (int r = 0; r < rings; r++)
                {
                    for (int s = 0; s < sectors; s++)
                    {
                        int idxA = r * sectors + s;
                        int idxB = r * sectors + ((s + shift) % sectors);
                        float d = histA[idxA] - histB[idxB];
                        sum += d * d;
                    }
                }
                best = Mathf.Min(best, sum);
            }
            return Mathf.Sqrt(best);
        }
    }

    // --- Zhang-Suen thinning (devuelve skeleton como bool[,]) ---
    private bool[,] ZhangSuenThinning(bool[,] input, int size)
    {
        int w = size, h = size;
        bool[,] img = new bool[w, h];
        // copia
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                img[x, y] = input[x, y];

        bool changed = true;
        List<Vector2Int> toRemove = new List<Vector2Int>();

        while (changed)
        {
            changed = false;
            toRemove.Clear();

            // Primera sub-iteración
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    if (!img[x, y]) continue;

                    int neighbors = CountNeighbors(img, x, y);
                    int transitions = CountTransitions(img, x, y);
                    bool p2 = img[x, y - 1];
                    bool p4 = img[x + 1, y];
                    bool p6 = img[x, y + 1];
                    bool p8 = img[x - 1, y];

                    if (neighbors >= 2 && neighbors <= 6 &&
                        transitions == 1 &&
                        (!p2 || !p4 || !p6) &&
                        (!p4 || !p6 || !p8))
                    {
                        toRemove.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (toRemove.Count > 0)
            {
                changed = true;
                foreach (var v in toRemove) img[v.x, v.y] = false;
                toRemove.Clear();
            }

            // Segunda sub-iteración
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    if (!img[x, y]) continue;

                    int neighbors = CountNeighbors(img, x, y);
                    int transitions = CountTransitions(img, x, y);
                    bool p2 = img[x, y - 1];
                    bool p4 = img[x + 1, y];
                    bool p6 = img[x, y + 1];
                    bool p8 = img[x - 1, y];

                    if (neighbors >= 2 && neighbors <= 6 &&
                        transitions == 1 &&
                        (!p2 || !p4 || !p8) &&
                        (!p2 || !p6 || !p8))
                    {
                        toRemove.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (toRemove.Count > 0)
            {
                changed = true;
                foreach (var v in toRemove) img[v.x, v.y] = false;
                toRemove.Clear();
            }
        }

        return img;
    }

    // Cuenta vecinos 8-conn (sin contar el centro)
    private int CountNeighbors(bool[,] img, int x, int y)
    {
        int c = 0;
        for (int j = -1; j <= 1; j++)
            for (int i = -1; i <= 1; i++)
                if (!(i == 0 && j == 0) && img[x + i, y + j]) c++;
        return c;
    }

    // Cuenta transiciones 0->1 en la secuencia p2..p9..p2
    private int CountTransitions(bool[,] img, int x, int y)
    {
        bool[] p = new bool[8];
        // p2..p9: N, NE, E, SE, S, SW, W, NW
        p[0] = img[x, y - 1];     // N
        p[1] = img[x + 1, y - 1]; // NE
        p[2] = img[x + 1, y];     // E
        p[3] = img[x + 1, y + 1]; // SE
        p[4] = img[x, y + 1];     // S
        p[5] = img[x - 1, y + 1]; // SW
        p[6] = img[x - 1, y];     // W
        p[7] = img[x - 1, y - 1]; // NW

        int transitions = 0;
        for (int k = 0; k < 8; k++)
        {
            bool cur = p[k];
            bool next = p[(k + 1) % 8];
            if (!cur && next) transitions++;
        }
        return transitions;
    }



}
