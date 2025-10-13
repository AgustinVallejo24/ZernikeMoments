// FileName: BinaryImage.cs
using UnityEngine;

/// <summary>
/// Representa una imagen binaria como una matriz de flotantes y proporciona operaciones básicas.
/// </summary>
public class BinaryImage
{
    public readonly float[,] Pixels;
    public readonly int Size;

    public BinaryImage(int imageSize)
    {
        Size = imageSize;
        Pixels = new float[imageSize, imageSize];
    }

    /// <summary>
    /// Establece todos los píxeles a 0.
    /// </summary>
    public void Clear()
    {
        System.Array.Clear(Pixels, 0, Pixels.Length);
    }

    /// <summary>
    /// Establece el valor de un píxel, asegurando que esté dentro de los límites.
    /// La intensidad se acumula para manejar cruces de líneas.
    /// </summary>
    public void SetPixelSafe(int x, int y, float intensity)
    {
        if (x >= 0 && x < Size && y >= 0 && y < Size)
        {
            Pixels[x, y] = Mathf.Min(Pixels[x, y] + intensity, 1f);
        }
    }

    /// <summary>
    /// Calcula la suma de las intensidades de todos los píxeles.
    /// </summary>
    /// <returns>La suma total de intensidades.</returns>
    public float GetActivePixelCount()
    {
        float count = 0;
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                count += Pixels[x, y];
            }
        }
        return count;
    }

    /// <summary>
    /// Genera una representación en string de la matriz para depuración.
    /// </summary>
    public void PrintToDebugLog()
    {
        string texto = "";
        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                texto += Pixels[row, col].ToString("F1") + ", ";
            }
            texto += "\n";
        }
        Debug.Log(texto);
    }
}
