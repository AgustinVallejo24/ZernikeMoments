// FileName: ZernikeProcessor.cs (Refactored)
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Fachada que coordina el proceso de rasterización, normalización y cálculo de momentos.
/// Mantiene la misma API pública que la clase original.
/// </summary>
public class ZernikeProcessor
{
    private float[,] _binaryImage;
    private float[] _angularHistogram;
    private readonly int _imageSize;
    private readonly ImageNormalizer _normalizer; // Mantenemos esto para ResizeImage

    public ZernikeProcessor(int imageSize)
    {
        _imageSize = imageSize;
        _binaryImage = new float[imageSize, imageSize];
        _angularHistogram = new float[0]; // Inicializar
        _normalizer = new ImageNormalizer(); // Asumo que esta clase existe
    }

    /// <summary>
    /// Limpia la imagen interna.
    /// </summary>
    public void ClearImage()
    {
        SymbolRasterizer.ClearImage(_binaryImage, _imageSize);
    }

    /// <summary>
    /// Dibuja trazos, los procesa y almacena el resultado internamente.
    /// </summary>
    public void DrawStrokes(List<List<Vector2>> allStrokes)
    {
        // 1. Limpiar y dibujar los trazos "en crudo"
        SymbolRasterizer.ClearImage(_binaryImage, _imageSize);
        SymbolRasterizer.DrawStrokes(_binaryImage, _imageSize, allStrokes);

        // 2. Procesar la imagen (adelgazar, engrosar, histograma)
        // Esto REEMPLAZA _binaryImage con la versión procesada
        _binaryImage = ImageRasterizer.NormalizeThicknessAndComputeAngularHistogram(
            _binaryImage, _imageSize, 3, out _angularHistogram
        );

        // ImprimirMimatriz(_binaryImage); // Descomentar si necesitas depurar
    }

    /// <summary>
    /// Dibuja una textura, la procesa y almacena el resultado internamente.
    /// </summary>
    public void DrawTexture(Texture2D texture)
    {
        // 1. Limpiar y dibujar la textura "en crudo"
        SymbolRasterizer.ClearImage(_binaryImage, _imageSize);
        SymbolRasterizer.DrawTexture(_binaryImage, _imageSize, texture);

        // 2. Procesar la imagen
        _binaryImage = ImageRasterizer.NormalizeThicknessAndComputeAngularHistogram(
            _binaryImage, _imageSize, 3, out _angularHistogram
        );

        // ImprimirMimatriz(_binaryImage); // Descomentar si necesitas depurar
    }

    /// <summary>
    /// Calcula los momentos de Zernike de la imagen procesada interna.
    /// </summary>
    public ZernikeMoment[] ComputeZernikeMoments(int maxN)
    {
        // Llama al calculador estático con la imagen ya procesada
        return ZernikeMomentCalculator.ComputeZernikeMoments(_binaryImage, _imageSize, maxN);
    }

    /// <summary>
    /// Obtiene el conteo de píxeles activos de la imagen procesada interna.
    /// </summary>
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

    /// <summary>
    /// Obtiene el último histograma angular calculado.
    /// </summary>
    public float[] GetSymbolDistribution()
    {
        return _angularHistogram;
    }

    /// <summary>
    /// Compara dos histogramas.
    /// </summary>
    public float CompareAngularHistograms(float[] histA, float[] histB)
    {
        // Simplemente delega a la clase de procesamiento
        // Nota: El bool 'rotationInvariant' estaba en tu código original,
        // pero ZernikeManager nunca lo pasaba. Lo mantengo por si acaso.
        return ImageRasterizer.CompareAngularHistograms(histA, histB, rotationInvariant: false);
    }

    /// <summary>
    /// Redimensiona una textura (funcionalidad delegada).
    /// </summary>
    public Texture2D ResizeImage(Texture2D texture, int size)
    {
        return _normalizer.ResizeSelectedTextures(texture, size);
    }

    // ... ImprimirMimatriz (si la necesitas) ...
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
}

//// Nota: Asumo que ImageNormalizer existe en otro archivo.
//public class ImageNormalizer
//{
//    public Texture2D ResizeSelectedTextures(Texture2D texture, int size)
//    {
//        // ... Lógica de Resize ...
//        // Placeholder - asegúrate de tener esta clase
//        Debug.LogWarning("ImageNormalizer.ResizeSelectedTextures no implementado en este refactor.");
//        return texture;
//    }
//}