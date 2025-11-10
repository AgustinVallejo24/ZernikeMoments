using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
public class ZernikeRecognizer
{
    private bool rotationSensitivity;
    private TMP_Text text;
    private ZernikeProcessor _processor;


    public ZernikeRecognizer(bool rotSensitivity, TMP_Text myText, ZernikeProcessor processor)
    {
        rotationSensitivity = rotSensitivity;
        text = myText;
        _processor = processor;
    }
    public void FindBestCandidate(
List<double> playerMagnitudes,
float[] playerDrawDistribution,
List<ReferenceSymbolGroup> candidates,
out ReferenceSymbolGroup bestMatch,
out double minDistance,
out float bestDistributionDiff,
out ReferenceSymbolGroup closestMismatch,
out double closestMismatchDist, out double closesetMismatchDistributionDist)
    {

        bestMatch = new ReferenceSymbolGroup();
        minDistance = double.MaxValue;
        bestDistributionDiff = 0;
        closestMismatch = new ReferenceSymbolGroup();
        closestMismatchDist = double.MaxValue;
        closesetMismatchDistributionDist = double.MaxValue;

        foreach (var group in candidates)
        {
            foreach (var reference in group.symbols)
            {
                double zernikeDistance = CalculateZernikeDistance(playerMagnitudes, reference.momentMagnitudes);
                float distributionDifference = _processor.CompareAngularHistograms(reference.distribution, playerDrawDistribution);

                double finalDistance = zernikeDistance;
                if (group.isSymmetric)
                {
                    finalDistance += distributionDifference;
                }

                bool meetsThresholds = CheckThresholds(finalDistance, distributionDifference, group);

                if (meetsThresholds && finalDistance < minDistance)
                {
                    minDistance = finalDistance;
                    bestMatch = group;
                    bestDistributionDiff = distributionDifference;
                }
                else if (!meetsThresholds && finalDistance < closestMismatchDist)
                {
                    closestMismatchDist = finalDistance;
                    closestMismatch = group;
                    closesetMismatchDistributionDist = distributionDifference;
                }

                Debug.Log($"Distancia con '{reference.symbolName}': {finalDistance:F3}");
            }
        }

    }

    public double CalculateZernikeDistance(List<double> playerMagnitudes, List<double> referenceMagnitudes)
    {
        double distanceSquared = 0;
        int count = Mathf.Min(playerMagnitudes.Count, referenceMagnitudes.Count);

        for (int i = 0; i < count; i++)
        {
            double diff = playerMagnitudes[i] - referenceMagnitudes[i];
            distanceSquared += diff * diff;
        }

        return Math.Sqrt(distanceSquared) * 100;
    }

    private bool CheckThresholds(double distance, float distributionDifference, ReferenceSymbolGroup reference)
    {
        if (distance > reference.Threshold)
        {
            return false;
        }

        if (rotationSensitivity && reference.useRotation)
        {
            if (distributionDifference > reference.orientationThreshold)
            {
                return false;
            }
        }

        return true;
    }

    public void DisplayResult(
    ReferenceSymbolGroup bestMatch,
    double minDistance,
    float distributionDiff,
    ReferenceSymbolGroup closestMismatch,
    double closestMismatchDist, double closesetMismatchDistributionDist)
    {
        Debug.Log(bestMatch.symbolName);
        if (bestMatch.symbols != null)
        {
            string resultText = $"Símbolo reconocido: {bestMatch.symbolName}\nDistancia: {minDistance:F3}";

            if (rotationSensitivity && bestMatch.useRotation)
            {
                resultText += $", Dif. Distribución: {distributionDiff:F3}";
            }
            text.text = resultText;
        }
        else if (closestMismatch.symbols.Count > 0)
        {
            if(rotationSensitivity && closestMismatch.useRotation)
            {
                text.text = $"Símbolo no reconocido. Match más cercano: '{closestMismatch.symbolName}' con distancia {closestMismatchDist:F3} y Dif. Distribución: {closesetMismatchDistributionDist:F3}";
            }
            else
            {
                text.text = $"Símbolo no reconocido. Match más cercano: '{closestMismatch.symbolName}' con distancia {closestMismatchDist:F3}";
            }
           
        }
        else
        {
            text.text = "Símbolo no reconocido.";
        }
    }
}
