using System;
using System.Collections.Generic;

/// <summary>
/// Calcula los momentos de Zernike para una BinaryImage dada, incluyendo la normalización interna original.
/// </summary>
public class ZernikeMomentCalculator
{
    public ZernikeMoment[] Compute(BinaryImage image, int maxN)
    {
        List<ZernikeMoment> moments = new List<ZernikeMoment>();
        int size = image.Size;
        var pixels = image.Pixels;

        // 1. Calcular centroide y masa total
        double cx = 0.0, cy = 0.0, mass = 0.0;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                double v = pixels[x, y];
                mass += v;
                cx += x * v;
                cy += y * v;
            }
        }
        if (mass > 0.0) { cx /= mass; cy /= mass; } else { cx = cy = size / 2.0; }

        // 2. Encontrar radio máximo para normalizar a un disco unitario
        double maxR = 0.0;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (pixels[x, y] > 0.0)
                    maxR = Math.Max(maxR, Math.Sqrt(Math.Pow(x - cx, 2) + Math.Pow(y - cy, 2)));

        if (maxR < 1e-6) maxR = size / 2.0;

        // 3. Contar masa dentro del disco para el factor de normalización 'deltaA'
        double massInDisk = 0.0;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                double xN = (x - cx) / maxR;
                double yN = (y - cy) / maxR;
                if (xN * xN + yN * yN <= 1.0)
                {
                    massInDisk += pixels[x, y];
                }
            }
        }
        double deltaA = massInDisk > 0.0 ? (1.0 / massInDisk) : (1.0 / (size * size));

        // 4. Calcular momentos con la normalización original
        for (int n = 0; n <= maxN; n++)
        {
            for (int m = -n; m <= n; m++)
            {
                if ((n - Math.Abs(m)) % 2 != 0) continue;

                double realAcc = 0.0, imagAcc = 0.0;
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        if (pixels[x, y] <= 0.0) continue;

                        double xN = (x - cx) / maxR;
                        double yN = (y - cy) / maxR;
                        double rho = Math.Sqrt(xN * xN + yN * yN);
                        if (rho > 1.0) continue;

                        double theta = Math.Atan2(yN, xN);
                        double radial = RadialPolynomialDouble(n, Math.Abs(m), rho);

                        realAcc += pixels[x, y] * radial * Math.Cos(m * theta);
                        imagAcc -= pixels[x, y] * radial * Math.Sin(m * theta); // Conjugado
                    }
                }

                // *** PASO CLAVE RESTAURADO ***
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

    private double RadialPolynomialDouble(int n, int m, double rho)
    {
        if ((n - m) % 2 != 0) return 0;
        double sum = 0.0;
        for (int s = 0; s <= (n - m) / 2; s++)
        {
            double term = Math.Pow(-1, s) * FactorialDouble(n - s) /
                          (FactorialDouble(s) * FactorialDouble((n + m) / 2 - s) * FactorialDouble((n - m) / 2 - s)) *
                          Math.Pow(rho, n - 2 * s);
            sum += term;
        }
        return sum;
    }

    private double FactorialDouble(int n)
    {
        if (n < 0) return 1.0; // Definición matemática para el contexto
        double r = 1.0;
        for (int i = 2; i <= n; i++) r *= i;
        return r;
    }
}