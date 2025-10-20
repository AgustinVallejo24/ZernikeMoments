using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ReferenceSymbol
{
    public string symbolName;

    public Texture2D templateTexture;
  

    public string symbolID;


    //public float Threshold;
    //public float orientationThreshold;
    //public bool useRotation = true;
    //public bool isSymmetric = false;
    public int strokes = 1;

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
