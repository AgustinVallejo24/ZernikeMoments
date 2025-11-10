using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ReferenceSymbol
{
    public string symbolName;
    public Texture2D templateTexture;
    public string symbolID;    
    public int strokes = 1;    
    public float[] distribution;    
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
