// FileName: ZernikeProcessor.cs (Updated)
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Fachada que coordina el proceso de rasterización, normalización y cálculo de momentos.
/// Mantiene la misma API pública que la clase original.
/// </summary>
public class ZernikeProcessor
{
    private BinaryImage _processedImage;
    private readonly int _imageSize;
    private readonly ImageNormalizer _normalizer;
    private readonly ZernikeMomentCalculator _momentCalculator;

    public ZernikeProcessor(int imageSize)
    {
        _imageSize = imageSize;
        _processedImage = new BinaryImage(imageSize);
        _normalizer = new ImageNormalizer();
        _momentCalculator = new ZernikeMomentCalculator();
    }


    public Texture2D ResizeImage(Texture2D texture, int size)
    {
        return _normalizer.ResizeSelectedTextures(texture, size);
    }

    /// <summary>
    /// Dibuja, normaliza y procesa una lista de trazos.
    /// </summary>
    /// 
    public void DrawStrokes(List<List<Vector2>> allStrokes)
    {
        // 1. Dibuja los trazos en una imagen temporal
        BinaryImage rawImage = new BinaryImage(_imageSize);
        ImageRasterizer.DrawStrokes(rawImage, allStrokes);

        // 2. Normaliza la imagen y calcula el histograma, guardando el resultado
        _processedImage = _normalizer.NormalizeThicknessAndComputeHistogram(rawImage, 3);

        // Opcional: Depuración
        _processedImage.PrintToDebugLog();
    }

    /// <summary>
    /// Dibuja, normaliza y procesa una textura.
    /// </summary>
    public void DrawTexture(Texture2D texture)
    {
        // 1. Dibuja la textura en una imagen temporal
        BinaryImage rawImage = new BinaryImage(_imageSize);
        ImageRasterizer.DrawTexture(rawImage, texture);

        // 2. Normaliza la imagen y calcula el histograma, guardando el resultado
        _processedImage = _normalizer.NormalizeThicknessAndComputeHistogram(rawImage, 3);

        // Opcional: Depuración
        _processedImage.PrintToDebugLog();
    }

    /// <summary>
    /// Calcula los momentos de Zernike sobre la última imagen procesada.
    /// </summary>
    public ZernikeMoment[] ComputeZernikeMoments(int maxN)
    {
        // El pre-cálculo del centroide y normalización del paper se hace aquí de manera implícita
        // usando toda la información de la imagen.
        float totalPixels = GetActivePixelCount();
        if (totalPixels <= 0) return new ZernikeMoment[0];

        // La fórmula original normalizaba por (n+1)/pi y el área diferencial.
        // Dado que la comparación es relativa, y ya normalizamos por masa total en el ZernikeManager,
        // nos enfocamos en el cálculo del momento en sí.
        return _momentCalculator.Compute(_processedImage, maxN);
    }

    /// <summary>
    /// Obtiene el histograma angular de la última operación de dibujo.
    /// </summary>
    public float[] GetSymbolDistribution()
    {
        return _normalizer.GetLastHistogram();
    }

    /// <summary>
    /// Obtiene el conteo de píxeles activos de la última imagen procesada.
    /// </summary>
    public float GetActivePixelCount()
    {
        return _processedImage.GetActivePixelCount();
    }

    /// <summary>
    /// Compara dos histogramas angulares.
    /// </summary>
    public float CompareAngularHistograms(float[] histA, float[] histB, bool rotationInvariant = false)
    {
        int sectors = 16;
        if (histA.Length != histB.Length) throw new ArgumentException("Length mismatch");
        int rings = histA.Length / sectors;
        float best = float.MaxValue;

        if (!rotationInvariant)
        {
            float sum = 0f;
            for (int i = 0; i < histA.Length; i++) sum += (histA[i] - histB[i]) * (histA[i] - histB[i]);
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
                        float d = histA[r * sectors + s] - histB[r * sectors + ((s + shift) % sectors)];
                        sum += d * d;
                    }
                }
                best = Mathf.Min(best, sum);
            }
            return Mathf.Sqrt(best);
        }
    }
}