using System;
using System.Collections.Generic;
using UnityEngine;

public static class HausdorffDistance
{
    public static double Calculate(float[,] A, float[,] B)
    {
        List<Vector2> pointsA = GetActivePoints(A);
        List<Vector2> pointsB = GetActivePoints(B);

        double distAB = DirectedHausdorff(pointsA, pointsB);
        double distBA = DirectedHausdorff(pointsB, pointsA);

        return Math.Max(distAB, distBA);
    }

    // Convierte la matriz en lista de puntos activos
    private static List<Vector2> GetActivePoints(float[,] matrix)
    {
        List<Vector2> points = new List<Vector2>();
        int width = matrix.GetLength(0);
        int height = matrix.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (matrix[x, y] > 0)
                {
                    points.Add(new Vector2(x, y));
                }
            }
        }
        return points;
    }

    // Distancia dirigida de A a B
    private static double DirectedHausdorff(List<Vector2> A, List<Vector2> B)
    {
        double maxMinDist = 0;

        foreach (var a in A)
        {
            double minDist = double.MaxValue;

            foreach (var b in B)
            {
                double dist = Vector2.Distance(a, b);
                if (dist < minDist) minDist = dist;
            }

            if (minDist > maxMinDist) maxMinDist = minDist;
        }

        return maxMinDist;
    }



    //public float CalculateElongationAxis(List<Vector2Int> activePixels)
    //{
    //    if (activePixels.Count < 3) return 0f;

    //    float minArea = float.MaxValue;
    //    float bestAngleDeg = 0f;

    //    // --- NUEVO: Necesitamos guardar las dimensiones de la mejor caja ---
    //    float finalWidth = 0f;
    //    float finalHeight = 0f;

    //    // Buscamos en un rango de 180� para encontrar el eje
    //    for (float angleDeg = 0f; angleDeg < 180f; angleDeg += 2.5f)
    //    {
    //        float angleRad = angleDeg * Mathf.Deg2Rad;
    //        float cosA = Mathf.Cos(angleRad);
    //        float sinA = Mathf.Sin(angleRad);

    //        float minX = float.MaxValue, maxX = float.MinValue;
    //        float minY = float.MaxValue, maxY = float.MinValue;

    //        foreach (var pixel in activePixels)
    //        {
    //            float rx = pixel.x * cosA - pixel.y * sinA;
    //            float ry = pixel.x * sinA + pixel.y * cosA;

    //            if (rx < minX) minX = rx;
    //            if (rx > maxX) maxX = rx;
    //            if (ry < minY) minY = ry;
    //            if (ry > maxY) maxY = ry;
    //        }

    //        float width = maxX - minX;
    //        float height = maxY - minY;
    //        float area = width * height;

    //        if (area < minArea)
    //        {
    //            minArea = area;
    //            bestAngleDeg = angleDeg;
    //            // --- NUEVO: Guardamos las dimensiones al encontrar una caja mejor ---
    //            finalWidth = width;
    //            finalHeight = height;
    //        }
    //    }

    //    // --- LA CORRECCI�N CLAVE ---
    //    // Despu�s de encontrar la rotaci�n de la caja m�nima, comprobamos si el objeto
    //    // es "alto" o "ancho" dentro de esa caja para determinar su orientaci�n final.
    //    float orientation;
    //    if (finalHeight > finalWidth)
    //    {
    //        // La forma es m�s alta que ancha, su eje principal est� 90 grados m�s all� del �ngulo de la caja.
    //        orientation = bestAngleDeg + 90f;
    //    }
    //    else
    //    {
    //        // La forma es m�s ancha que alta, su eje coincide con el �ngulo de la caja.
    //        orientation = bestAngleDeg;
    //    }

    //    // Normalizamos el resultado final para que est� siempre en el rango [0, 180)
    //    // Esto maneja casos donde el �ngulo podr�a ser > 180.
    //    return (orientation % 180f + 180f) % 180f;
    //}
    //public float CalculateHybridOrientation()
    //{
    //    // --- PASO 0: OBTENER DATOS B�SICOS ---
    //    float totalPixels = GetActivePixelCount();
    //    if (totalPixels < 3) return 0f; // Necesitamos al menos 3 p�xeles para una orientaci�n significativa
    //    Vector2 centroid = Vector2.zero;
    //    List<Vector2Int> activePixels = new List<Vector2Int>();
    //    for (int y = 0; y < _imageSize; y++)
    //    {
    //        for (int x = 0; x < _imageSize; x++)
    //        {
    //            if (_binaryImage[x, y] > 0)
    //            {
    //                centroid.x += x;
    //                centroid.y += y;
    //                activePixels.Add(new Vector2Int(x, y));
    //            }
    //        }
    //    }
    //    centroid /= totalPixels;

    //    // --- PASO 1: INTENTAR CON EL M�TODO DE ASIMETR�A PONDERADA ---
    //    Vector2 totalOrientationVector = Vector2.zero;
    //    float totalWeight = 0f;
    //    foreach (Vector2Int pixelPos in activePixels)
    //    {
    //        Vector2 vectorToPixel = new Vector2(pixelPos.x - centroid.x, pixelPos.y - centroid.y);
    //        float weight = vectorToPixel.magnitude;
    //        if (weight > 0.001f)
    //        {
    //            totalOrientationVector += vectorToPixel * weight;
    //            totalWeight += weight;
    //        }
    //    }

    //    // --- PASO 2: DECIDIR SI EL RESULTADO DE ASIMETR�A ES CONFIABLE ---
    //    float asymmetryScore = (totalWeight > 0) ? totalOrientationVector.magnitude / totalWeight : 0;
    //    // Este umbral es clave. Puede que necesites ajustarlo.
    //    // Un valor bajo significa que muchas formas se considerar�n sim�tricas.
    //    // Un valor alto significa que se confiar� m�s en el m�todo de asimetr�a.

    //    float symmetryThreshold = 2.5f;
    //    if (asymmetryScore > symmetryThreshold)
    //    {
    //        Debug.Log("FORMA ASIMETRICA - Usando Vector Ponderado.");
    //        return Mathf.Atan2(totalOrientationVector.y, totalOrientationVector.x) * Mathf.Rad2Deg;
    //    }
    //    else
    //    {
    //        Debug.Log("FORMA SIMETRICA - Usando Eje de Elongaci�n.");
    //        // --- CAMBIO AQU� ---
    //        // Llamamos a nuestro nuevo y mejorado m�todo para formas sim�tricas.
    //        return CalculateElongationAxis(activePixels);
    //    }
    //}

    //public float CalculateStableOrientation()
    //{
    //    // --- PASO 1: CALCULAR EL CENTROIDE ---
    //    float totalPixels = GetActivePixelCount();
    //    if (totalPixels < 2) return 0f; // No hay orientaci�n si hay 0 o 1 p�xeles

    //    Vector2 centroid = Vector2.zero;
    //    for (int y = 0; y < _imageSize; y++)
    //    {
    //        for (int x = 0; x < _imageSize; x++)
    //        {
    //            if (_binaryImage[x, y] > 0)
    //            {
    //                centroid.x += x;
    //                centroid.y += y;
    //            }
    //        }
    //    }
    //    centroid /= totalPixels;

    //    // --- PASO 2: ENCONTRAR EL PUNTO M�S LEJANO DEL CENTROIDE ---
    //    Vector2 farthestPoint = Vector2.zero;
    //    float maxDistanceSquared = -1f;

    //    for (int y = 0; y < _imageSize; y++)
    //    {
    //        for (int x = 0; x < _imageSize; x++)
    //        {
    //            if (_binaryImage[x, y] > 0)
    //            {
    //                // Usamos la distancia al cuadrado para evitar el coste de la ra�z cuadrada
    //                float distanceSquared = (x - centroid.x) * (x - centroid.x) + (y - centroid.y) * (y - centroid.y);
    //                if (distanceSquared > maxDistanceSquared)
    //                {
    //                    maxDistanceSquared = distanceSquared;
    //                    farthestPoint.x = x;
    //                    farthestPoint.y = y;
    //                }
    //            }
    //        }
    //    }

    //    // --- PASO 3: CALCULAR EL �NGULO DEL VECTOR RESULTANTE ---
    //    Vector2 orientationVector = farthestPoint - centroid;

    //    // Devolvemos el �ngulo en grados. 0� es a la derecha, 90� es arriba.
    //    return Mathf.Atan2(orientationVector.y, orientationVector.x) * Mathf.Rad2Deg;
    //}


    //public float CalculateWeightedAsymmetryOrientation()
    //{
    //    // --- PASO 1: CALCULAR EL CENTROIDE ---
    //    float totalPixels = GetActivePixelCount();
    //    if (totalPixels < 2) return 0f;

    //    Vector2 centroid = Vector2.zero;
    //    // Usamos una lista para no tener que recorrer la imagen dos veces
    //    List<Vector2Int> activePixels = new List<Vector2Int>();

    //    for (int y = 0; y < _imageSize; y++)
    //    {
    //        for (int x = 0; x < _imageSize; x++)
    //        {
    //            if (_binaryImage[x, y] > 0)
    //            {
    //                centroid.x += x;
    //                centroid.y += y;
    //                activePixels.Add(new Vector2Int(x, y));
    //            }
    //        }
    //    }
    //    centroid /= totalPixels;

    //    // --- PASO 2: CALCULAR EL VECTOR DE ORIENTACI�N SUMANDO TODOS LOS VECTORES PONDERADOS ---
    //    Vector2 totalOrientationVector = Vector2.zero;

    //    foreach (Vector2Int pixelPos in activePixels)
    //    {
    //        // Vector desde el centroide al p�xel actual
    //        Vector2 vectorToPixel = new Vector2(pixelPos.x - centroid.x, pixelPos.y - centroid.y);

    //        // El peso es la magnitud (distancia) del propio vector.
    //        // Esto le da m�s importancia a los puntos lejanos.
    //        float weight = vectorToPixel.magnitude;

    //        if (weight > 0.001f) // Evitar multiplicar por cero
    //        {
    //            // Sumamos el vector, ponderado por su propia magnitud.
    //            totalOrientationVector += vectorToPixel * weight;
    //        }
    //    }

    //    // --- PASO 3: OBTENER EL �NGULO DEL VECTOR FINAL ---
    //    // Si la forma es perfectamente sim�trica (ej. un c�rculo), el vector total ser� (0,0).
    //    if (totalOrientationVector.magnitude < 0.001f)
    //    {
    //        return 0f; // No hay una orientaci�n definida
    //    }

    //    // Devolvemos el �ngulo en grados.
    //    return Mathf.Atan2(totalOrientationVector.y, totalOrientationVector.x) * Mathf.Rad2Deg;
    //}





}








