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
}








