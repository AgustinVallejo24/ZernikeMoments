using System;
using System.Collections.Generic;

/// <summary>
/// Calcula los momentos de Zernike para una BinaryImage dada, incluyendo la normalización interna original.
/// </summary>
public class ZernikeMomentCalculator
{
        public static ZernikeMoment[] ComputeZernikeMoments(float[,] binaryImage, int imageSize, int maxN)
        {
            List<ZernikeMoment> moments = new List<ZernikeMoment>();
            int size = imageSize;

            // 0) centroid
            double cx = 0.0, cy = 0.0, mass = 0.0;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double v = binaryImage[x, y];
                    mass += v;
                    cx += x * v;
                    cy += y * v;
                }
            }
            if (mass > 0.0) { cx /= mass; cy /= mass; }
            else { cx = size / 2.0; cy = size / 2.0; }

            // 1) radio max
            double maxR = 0.0;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    if (binaryImage[x, y] > 0.0)
                    {
                        double dx = x - cx;
                        double dy = y - cy;
                        double d = Math.Sqrt(dx * dx + dy * dy);
                        if (d > maxR) maxR = d;
                    }
            if (maxR < 1e-6) maxR = Math.Max(size / 2.0, 1.0);

            // 2) masa en disco
            double massInDisk = 0.0;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double xN = (x - cx) / maxR;
                    double yN = (y - cy) / maxR;
                    double rho = Math.Sqrt(xN * xN + yN * yN);
                    if (rho <= 1.0) massInDisk += binaryImage[x, y];
                }
            }
            double deltaA = massInDisk > 0.0 ? (1.0 / massInDisk) : (1.0 / (size * size));

            // 3) calculo
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
                            double val = binaryImage[x, y];
                            if (val <= 0.0) continue;

                            double xN = (x - cx) / maxR;
                            double yN = (y - cy) / maxR;
                            double rho = Math.Sqrt(xN * xN + yN * yN);
                            if (rho > 1.0) continue;

                            double theta = Math.Atan2(yN, xN);
                            double radial = RadialPolynomialDouble(n, Math.Abs(m), rho);

                            realAcc += val * radial * Math.Cos(m * theta);
                            imagAcc -= val * radial * Math.Sin(m * theta);
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

        private static double RadialPolynomialDouble(int n, int m, double rho)
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

        private static double FactorialDouble(int n)
        {
            if (n < 0) return 1.0;
            double r = 1.0;
            for (int i = 2; i <= n; i++) r *= i;
            return r;
        }
    }
