using System;
using System.Collections.Generic;
using UnityEngine;

public static class ImageRasterizer
{
    // Normalizes stroke thickness and computes the angular histogram
    public static float[,] NormalizeThicknessAndComputeAngularHistogram(
        float[,] binaryMatrix, int imageSize, int targetThickness,
        out float[] angularHistogram, int sectors = 16, int rings = 3)
    {
        angularHistogram = new float[sectors * rings];
        if (imageSize <= 0) return binaryMatrix;

        bool[,] bin = ToBoolMatrix(binaryMatrix, imageSize);
        bool[,] skeleton = ZhangSuenThinning(bin, imageSize);

        if (targetThickness <= 1)
            return BoolToFloat(skeleton, imageSize);

        float cx = 0f, cy = 0f;
        int skeletonCount = 0;

        // Compute centroid
        for (int y = 0; y < imageSize; y++)
            for (int x = 0; x < imageSize; x++)
                if (skeleton[x, y]) { cx += x; cy += y; skeletonCount++; }

        if (skeletonCount > 0)
        {
            cx /= skeletonCount;
            cy /= skeletonCount;
        }
        else
        {
            cx = imageSize / 2f;
            cy = imageSize / 2f;
        }

        // Compute disk offsets for thickening
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

        float maxDist = Mathf.Sqrt(imageSize * imageSize + imageSize * imageSize) / 2f;

        // Expand skeleton and fill polar histogram
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

                        float dx = px - cx;
                        float dy = py - cy;
                        float angle = Mathf.Atan2(dy, dx);
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

        // Normalize histogram
        if (totalAdded > 0)
        {
            for (int i = 0; i < angularHistogram.Length; i++)
                angularHistogram[i] /= totalAdded;
        }

        return thickened;
    }

    // Compares two angular histograms (optionally rotation-invariant)
    public static float CompareAngularHistograms(float[] histA, float[] histB, bool rotationInvariant = false)
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

    // Converts a float matrix to a boolean matrix
    private static bool[,] ToBoolMatrix(float[,] src, int size)
    {
        bool[,] b = new bool[size, size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                b[x, y] = src[x, y] > 0f;
        return b;
    }

    // Converts a boolean matrix to a float matrix
    private static float[,] BoolToFloat(bool[,] b, int size)
    {
        float[,] f = new float[size, size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                f[x, y] = b[x, y] ? 1f : 0f;
        return f;
    }

    // Applies the Zhang-Suen thinning algorithm
    private static bool[,] ZhangSuenThinning(bool[,] input, int size)
    {
        int w = size, h = size;
        bool[,] img = new bool[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                img[x, y] = input[x, y];

        bool changed = true;
        List<Vector2Int> toRemove = new List<Vector2Int>();

        while (changed)
        {
            changed = false;
            toRemove.Clear();

            // Sub-iteration 1
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    if (!img[x, y]) continue;
                    int neighbors = CountNeighbors(img, x, y);
                    int transitions = CountTransitions(img, x, y);
                    bool p2 = img[x, y - 1]; bool p4 = img[x + 1, y];
                    bool p6 = img[x, y + 1]; bool p8 = img[x - 1, y];
                    if (neighbors >= 2 && neighbors <= 6 && transitions == 1 && (!p2 || !p4 || !p6) && (!p4 || !p6 || !p8))
                        toRemove.Add(new Vector2Int(x, y));
                }
            }
            if (toRemove.Count > 0)
            {
                changed = true;
                foreach (var v in toRemove) img[v.x, v.y] = false;
                toRemove.Clear();
            }

            // Sub-iteration 2
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    if (!img[x, y]) continue;
                    int neighbors = CountNeighbors(img, x, y);
                    int transitions = CountTransitions(img, x, y);
                    bool p2 = img[x, y - 1]; bool p4 = img[x + 1, y];
                    bool p6 = img[x, y + 1]; bool p8 = img[x - 1, y];
                    if (neighbors >= 2 && neighbors <= 6 && transitions == 1 && (!p2 || !p4 || !p8) && (!p2 || !p6 || !p8))
                        toRemove.Add(new Vector2Int(x, y));
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

    // Counts the number of active neighbors around a pixel
    private static int CountNeighbors(bool[,] img, int x, int y)
    {
        int c = 0;
        for (int j = -1; j <= 1; j++)
            for (int i = -1; i <= 1; i++)
                if (!(i == 0 && j == 0) && img[x + i, y + j]) c++;
        return c;
    }

    // Counts the number of 0 -> 1 transitions in the neighborhood
    private static int CountTransitions(bool[,] img, int x, int y)
    {
        bool[] p = new bool[8];
        p[0] = img[x, y - 1]; p[1] = img[x + 1, y - 1];
        p[2] = img[x + 1, y]; p[3] = img[x + 1, y + 1];
        p[4] = img[x, y + 1]; p[5] = img[x - 1, y + 1];
        p[6] = img[x - 1, y]; p[7] = img[x - 1, y - 1];
        int transitions = 0;
        for (int k = 0; k < 8; k++)
            if (!p[k] && p[(k + 1) % 8]) transitions++;
        return transitions;
    }
}
