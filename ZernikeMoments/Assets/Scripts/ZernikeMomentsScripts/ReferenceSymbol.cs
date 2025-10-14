using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ReferenceSymbol
{
    public string symbolName;
    public bool useRotation = true;
    public bool isSymmetric = false;
    public Texture2D templateTexture;
    public float Threshold;
    public int strokes = 1;
    public float orientationThreshold;
    public string symbolID;

    // [HideInInspector]
    public float[] distribution;

    //   [HideInInspector]
    public List<double> momentMagnitudes;


    public ReferenceSymbol(string name, float[] rotDistribution, List<double> magnitudes, int strokesQ, string sID)
    {
        symbolName = name;
        distribution = rotDistribution;
        momentMagnitudes = magnitudes;
        strokes = strokesQ;
        symbolID = sID;
    }

   
}
