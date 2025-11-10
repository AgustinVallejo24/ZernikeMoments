using System.Collections.Generic;
using UnityEngine;
using System;

public class ZernikeProcessor
{
    private float[,] _binaryImage;
    private float[] _angularHistogram;
    private readonly int _imageSize;
    private readonly ImageNormalizer _normalizer;

    public ZernikeProcessor(int imageSize)
    {
        _imageSize = imageSize;
        _binaryImage = new float[imageSize, imageSize];
        _angularHistogram = new float[0];
        _normalizer = new ImageNormalizer();
    }

    // Clears the internal image
    public void ClearImage()
    {
        SymbolRasterizer.ClearImage(_binaryImage, _imageSize);
    }

    // Draws and processes strokes into the internal image
    public void DrawStrokes(List<List<Vector2>> allStrokes)
    {
        SymbolRasterizer.ClearImage(_binaryImage, _imageSize);
        SymbolRasterizer.DrawStrokes(_binaryImage, _imageSize, allStrokes);
        _binaryImage = ImageRasterizer.NormalizeThicknessAndComputeAngularHistogram(
            _binaryImage, _imageSize, 3, out _angularHistogram
        );
    }

    // Draws and processes a texture into the internal image
    public void DrawTexture(Texture2D texture)
    {
        SymbolRasterizer.ClearImage(_binaryImage, _imageSize);
        SymbolRasterizer.DrawTexture(_binaryImage, _imageSize, texture);
        _binaryImage = ImageRasterizer.NormalizeThicknessAndComputeAngularHistogram(
            _binaryImage, _imageSize, 3, out _angularHistogram
        );
    }

    // Computes Zernike moments from the processed image
    public ZernikeMoment[] ComputeZernikeMoments(int maxN)
    {
        return ZernikeMomentCalculator.ComputeZernikeMoments(_binaryImage, _imageSize, maxN);
    }

    // Returns the count of active pixels in the processed image
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

    // Returns the last computed angular histogram
    public float[] GetSymbolDistribution()
    {
        return _angularHistogram;
    }

    // Compares two angular histograms
    public float CompareAngularHistograms(float[] histA, float[] histB)
    {
        return ImageRasterizer.CompareAngularHistograms(histA, histB, rotationInvariant: false);
    }

    // Resizes a texture using the image normalizer
    public Texture2D ResizeImage(Texture2D texture, int size)
    {
        return _normalizer.ResizeSelectedTextures(texture, size);
    }

    // Prints the internal matrix for debugging
    void ImprimirMimatriz(float[,] Mimatriz)
    {
        string texto = "";
        for (int row = 0; row < Mimatriz.GetLength(0); row++)
        {
            for (int col = 0; col < Mimatriz.GetLength(1); col++)
            {
                texto += Mimatriz[row, col] + ", ";
            }
            texto += "\n";
        }
        Debug.Log(texto);
    }
}
