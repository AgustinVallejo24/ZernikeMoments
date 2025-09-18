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
    private void DrawAALine(int x0, int y0, int x1, int y1)
    {
        // Asegurar que los puntos estén dentro de los límites de la imagen
        x0 = Mathf.Clamp(x0, 0, _imageSize - 1);
        y0 = Mathf.Clamp(y0, 0, _imageSize - 1);
        x1 = Mathf.Clamp(x1, 0, _imageSize - 1);
        y1 = Mathf.Clamp(y1, 0, _imageSize - 1);

        bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
        if (steep)
        {
            // Intercambiar x e y para manejar líneas con pendiente > 1
            int temp = x0; x0 = y0; y0 = temp;
            temp = x1; x1 = y1; y1 = temp;
        }

        if (x0 > x1)
        {
            // Asegurar que x0 <= x1
            int temp = x0; x0 = x1; x1 = temp;
            temp = y0; y0 = y1; y1 = temp;
        }

        int dx = x1 - x0;
        int dy = y1 - y0;
        float gradient = dx == 0 ? 1f : (float)dy / dx;

        // Primer píxel
        float xEnd = x0;
        float yEnd = y0;
        float xGap = 1f - (x0 + 0.5f - Mathf.Floor(x0 + 0.5f));
        int xPx1 = x0;
        int yPx1 = Mathf.FloorToInt(yEnd);
        float intensity1 = (1f - Mathf.Abs(yEnd - yPx1)) * xGap;
        float intensity2 = Mathf.Abs(yEnd - yPx1) * xGap;

        if (steep)
        {
            SetPixelSafe(yPx1, xPx1, intensity1);
            SetPixelSafe(yPx1 + (yEnd > yPx1 ? 1 : -1), xPx1, intensity2);
        }
        else
        {
            SetPixelSafe(xPx1, yPx1, intensity1);
            SetPixelSafe(xPx1, yPx1 + (yEnd > yPx1 ? 1 : -1), intensity2);
        }

        // Bucle principal
        float intery = yEnd + gradient;
        for (int x = x0 + 1; x < x1; x++)
        {
            int y = Mathf.FloorToInt(intery);
            float intensity = 1f - Mathf.Abs(intery - y);
            if (steep)
            {
                SetPixelSafe(y, x, intensity);
                SetPixelSafe(y + (intery > y ? 1 : -1), x, 1f - intensity);
            }
            else
            {
                SetPixelSafe(x, y, intensity);
                SetPixelSafe(x, y + (intery > y ? 1 : -1), 1f - intensity);
            }
            intery += gradient;
        }

        // Último píxel
        xEnd = x1;
        yEnd = y1;
        xGap = x1 + 0.5f - Mathf.Floor(x1 + 0.5f);
        int xPx2 = x1;
        int yPx2 = Mathf.FloorToInt(yEnd);
        intensity1 = (1f - Mathf.Abs(yEnd - yPx2)) * xGap;
        intensity2 = Mathf.Abs(yEnd - yPx2) * xGap;

        if (steep)
        {
            SetPixelSafe(yPx2, xPx2, intensity1);
            SetPixelSafe(yPx2 + (yEnd > yPx2 ? 1 : -1), xPx2, intensity2);
        }
        else
        {
            SetPixelSafe(xPx2, yPx2, intensity1);
            SetPixelSafe(xPx2, yPx2 + (yEnd > yPx2 ? 1 : -1), intensity2);
        }
    }

    // Función auxiliar para asignar intensidad a un píxel con seguridad
    private void SetPixelSafe(int x, int y, float intensity)
    {
        if (x >= 0 && x < _imageSize && y >= 0 && y < _imageSize)
        {
            // Acumular intensidad (suma en lugar de sobrescribir, útil para líneas que se cruzan)
            _binaryImage[x, y] = Mathf.Min(_binaryImage[x, y] + intensity, 1f);
        }
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
    public void ClearImage()
    {
        for (int y = 0; y < _imageSize; y++)
        {
            for (int x = 0; x < _imageSize; x++)
            {
                _binaryImage[x, y] = 0;
            }
        }
    }
    public void DrawStrokes(List<List<Vector2>> allStrokes)
    {
        // 1. Limpia la imagen al principio
        ClearImage();

        if (allStrokes.Count == 0) return;

        // 2. Encuentra los límites de TODOS los puntos para una normalización correcta
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

        // Margen
        minX -= .6f; minY -= .6f;
        maxX += .6f; maxY += .6f;

        // 3. Calcula escala y offsets UNA SOLA VEZ para el conjunto completo
        float scale = Mathf.Min((_imageSize - 1) / (maxX - minX), (_imageSize - 1) / (maxY - minY));
        float offsetX = (_imageSize - (maxX - minX) * scale) / 2f;
        float offsetY = (_imageSize - (maxY - minY) * scale) / 2f;

        // 4. Dibuja cada trazo por separado
        foreach (var stroke in allStrokes) // <--- BUCLE EXTERNO (POR CADA TRAZO)
        {
            // BUCLE INTERNO (DENTRO DE UN TRAZO)
            for (int i = 0; i < stroke.Count - 1; i++)
            {
                int x0 = Mathf.RoundToInt((stroke[i].x - minX) * scale + offsetX);
                int y0 = Mathf.RoundToInt((stroke[i].y - minY) * scale + offsetY);
                int x1 = Mathf.RoundToInt((stroke[i + 1].x - minX) * scale + offsetX);
                int y1 = Mathf.RoundToInt((stroke[i + 1].y - minY) * scale + offsetY);

                DrawAALine(x0, y0, x1, y1); // No une el final de este trazo con el inicio del siguiente
            }
        }

        // 5. Procesa la imagen final con todos los trazos ya dibujados
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
        int size = _imageSize;

        // 0) centroid (peso por intensidad)
        double cx = 0.0, cy = 0.0, mass = 0.0;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                double v = _binaryImage[x, y];
                mass += v;
                cx += x * v;
                cy += y * v;
            }
        }
        if (mass > 0.0) { cx /= mass; cy /= mass; }
        else { cx = size / 2.0; cy = size / 2.0; } // fallback

        // 1) radio max relativo al centroide (para normalizar a disco unidad)
        double maxR = 0.0;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (_binaryImage[x, y] > 0.0)
                {
                    double dx = x - cx;
                    double dy = y - cy;
                    double d = Math.Sqrt(dx * dx + dy * dy);
                    if (d > maxR) maxR = d;
                }
        if (maxR < 1e-6) maxR = Math.Max(size / 2.0, 1.0);

        // 2) contar (suma de intensidades) dentro del disco unidad para normaliza
        double massInDisk = 0.0;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                double xN = (x - cx) / maxR;
                double yN = (y - cy) / maxR;
                double rho = Math.Sqrt(xN * xN + yN * yN);
                if (rho <= 1.0) massInDisk += _binaryImage[x, y];
            }
        }
        double deltaA = massInDisk > 0.0 ? (1.0 / massInDisk) : (1.0 / (size * size));

        // 3) cálculo de momentos (usando double para más precisión)
        for (int n = 0; n <= maxN; n++)
        {
            for (int m = -n; m <= n; m++)
            {
                if ((n - Math.Abs(m)) % 2 != 0) continue;

                double realAcc = 0.0;
                double imagAcc = 0.0;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        double val = _binaryImage[x, y];
                        if (val <= 0.0) continue;

                        double xN = (x - cx) / maxR;
                        double yN = (y - cy) / maxR;
                        double rho = Math.Sqrt(xN * xN + yN * yN);
                        if (rho > 1.0) continue;

                        double theta = Math.Atan2(yN, xN);
                        double radial = RadialPolynomialDouble(n, Math.Abs(m), rho);

                        
                        realAcc += val * radial * Math.Cos(m * theta);
                        imagAcc -= val * radial * Math.Sin(m * theta); // nota el signo para conj
                    }
                }

                double prefactor = (n + 1.0) / Math.PI;
                realAcc *= prefactor * deltaA;
                imagAcc *= prefactor * deltaA;

                double magnitude = Math.Sqrt(realAcc * realAcc + imagAcc * imagAcc);
                double phase = Math.Atan2(imagAcc, realAcc);

                moments.Add(new ZernikeMoment((float)magnitude, (float)phase));
            }
        }

        return moments.ToArray();
    }

    // Radial polynomial en double
    private double RadialPolynomialDouble(int n, int m, double rho)
    {
        double sum = 0.0;
        int sLimit = (n - m) / 2;
        for (int s = 0; s <= sLimit; s++)
        {
            double sign = (s % 2 == 0) ? 1.0 : -1.0;
            double num = FactorialDouble(n - s);
            double denom = FactorialDouble(s) * FactorialDouble((n + m) / 2 - s) * FactorialDouble((n - m) / 2 - s);
            double term = sign * (num / denom) * Math.Pow(rho, n - 2 * s);
            sum += term;
        }
        return sum;
    }

    private double FactorialDouble(int n)
    {
        if (n < 0) return 1.0;
        double r = 1.0;
        for (int i = 2; i <= n; i++) r *= i;
        return r;
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
