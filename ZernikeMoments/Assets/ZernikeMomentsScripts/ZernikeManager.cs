using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.IO;
public class ZernikeManager : MonoBehaviour
{
    // Tamaño de la imagen para el procesamiento
    [SerializeField] TMP_Text text;
    public int imageSize = 64;
    // Orden máximo para los momentos de Zernike
    public int maxMomentOrder = 10;
    // Umbral de distancia para un reconocimiento exitoso
    public float recognitionThreshold = 0.5f;

    public RenderTexture renderTexture;

    public bool rotationSensitivity;

    public GameObject UICarga;
    public Image barrita;
    public GameObject UIDibujo;
    public GameObject DrawingTest;
    // Almacena los descriptores (magnitudes de momentos) de los símbolos de referencia.
   
    public List<ReferenceSymbol> referenceSymbols;

    private ZernikeProcessor _processor;
    private List<Vector2> _currentStrokePoints;
   
    public bool shouldLoad;
    string jsonPath;


    void Start()
    {
        jsonPath = Path.Combine(Application.dataPath, "Resources", "symbols.json");
    
        _processor = new ZernikeProcessor(imageSize);
        _currentStrokePoints = new List<Vector2>();
        if (shouldLoad)
        {
            StartCoroutine(Compute());
  
        }
        else
        {
            referenceSymbols.Clear();
            referenceSymbols = ReferenceSymbolStorage.LoadFromResources("symbols");
            UICarga.SetActive(false);
            UIDibujo.SetActive(true);
            DrawingTest.SetActive(true);
        }
    }


    public void SaveSymbolList()
    {
        ReferenceSymbolStorage.SaveSymbols(referenceSymbols, jsonPath);
    }
    IEnumerator Compute()
    {
        float carga = 0;
    
        foreach (var reference in referenceSymbols)
        {
       
            if (reference.templateTexture != null)
            {
                // Procesar la textura para obtener la matriz binaria
                reference.templateTexture = reference.ResizeSelectedTextures(reference.templateTexture, 64);
                Debug.Log(reference.templateTexture.height);
                _processor.DrawTexture(reference.templateTexture);
                
                // Calcular la suma de todos los píxeles activos para la normalización
                float totalPixels = _processor.GetActivePixelCount();
                Debug.Log("Divido por " + totalPixels);
               
                ZernikeMoment[] moments = _processor.ComputeZernikeMoments(maxMomentOrder);

                reference.momentMagnitudes = new List<double>();
                // Normalizar y guardar las magnitudes
                foreach (var moment in moments)
                {
                    // Evitar división por cero
                    double normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
                    reference.momentMagnitudes.Add(normalizedMagnitude);
                }

                reference.distribution = _processor.GetSymbolDistribution();
            }
            carga += 1f/referenceSymbols.Count;

       
            barrita.fillAmount = carga;
           
            yield return null;
        }
        UICarga.SetActive(false);
        UIDibujo.SetActive(true);
        yield return new WaitForSeconds(.2f);
        DrawingTest.SetActive(true);
        ReferenceSymbolStorage.SaveSymbols(referenceSymbols, jsonPath);
    }
    public void OnDrawingFinished(List<List<Vector2>> finishedPoints, int strokeQuantity)
    {

       // _currentStrokePoints = finishedPoints;
        _processor.DrawStrokes(finishedPoints);
        float totalPixels = _processor.GetActivePixelCount();
        ZernikeMoment[] playerMoments = _processor.ComputeZernikeMoments(maxMomentOrder);
        float[] playerDistribution = _processor.GetSymbolDistribution();
        List<double> playerMagnitudes = new List<double>();
        foreach (var moment in playerMoments)
        {
            double normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
            playerMagnitudes.Add(normalizedMagnitude);
        }

        RecognizeSymbol(playerMagnitudes, strokeQuantity, playerDistribution);
    }


    public void SaveSymbol(List<List<Vector2>> finishedPoints, int strokeQuantity, string symbolName)
    {
       
        _processor.DrawStrokes(finishedPoints);
        float totalPixels = _processor.GetActivePixelCount();
        ZernikeMoment[] playerMoments = _processor.ComputeZernikeMoments(maxMomentOrder);
        float[] playerDistribution = _processor.GetSymbolDistribution();
        List<double> playerMagnitudes = new List<double>();
        foreach (var moment in playerMoments)
        {
            double normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
            playerMagnitudes.Add(normalizedMagnitude);
        }
        ReferenceSymbol newSymbol = new ReferenceSymbol(symbolName, playerDistribution, playerMagnitudes, strokeQuantity);
        string savePath = Path.Combine(Application.dataPath, "Resources", "drawnSymbols.json");
        ReferenceSymbolStorage.AppendSymbol(newSymbol, savePath);
        referenceSymbols.Add(newSymbol);
        ReferenceSymbolStorage.SaveSymbols(referenceSymbols, jsonPath);
        
    }

    private void RecognizeSymbol(List<double> playerMagnitudes, int strokeQuantity, float[] playerDrawDistribution)
    {
        double minDistance = double.MaxValue;
        float minDistributionDiference = 0;
        string recognizedSymbolName = "None";
        ReferenceSymbol bestMatch = null;
        double wrongDistance = 0;
        string wrongSymbolName = "None";
        float wrongDistributionDiference = 0;
        // --- NUEVO: Calcular la orientación del dibujo del jugador ---
        // float playerOrientationDegrees = playerOrientationRadians * Mathf.Rad2Deg;
        if (referenceSymbols.Where(x => x.strokes == strokeQuantity).Count() < 1)
        {
            text.text = $"No hay ningun simbolo con esa cantidad de trazos";
            return;
        }
        foreach (var reference in referenceSymbols.Where(x => x.strokes == strokeQuantity))
        {

            
            // 1. CÁLCULO DE DISTANCIA (Zernike) - sin cambios
            double distanceSquared = 0; // Usar double para consistencia
            int count = Mathf.Min(playerMagnitudes.Count, reference.momentMagnitudes.Count);

            for (int i = 0; i < count; i++)
            {
                double diff = playerMagnitudes[i] - reference.momentMagnitudes[i];
                distanceSquared += diff * diff;
            }

            double distance = Math.Sqrt(distanceSquared) *100;
          
            float distributionDiference = _processor.CompareAngularHistograms(reference.distribution, playerDrawDistribution);
            if (rotationSensitivity && reference.useRotation)
            {
                if (reference.isSymmetric)
                {
                    distance += distributionDiference;
                }

                if (distance < minDistance && distance <= reference.Threshold && distributionDiference <= reference.orientationThreshold)
                {
                    minDistance = distance;
                    bestMatch = reference;
                    recognizedSymbolName = reference.symbolName;
                    minDistributionDiference = distributionDiference;
                }
                else if (distance < minDistance && (distance > reference.Threshold || distributionDiference > reference.orientationThreshold))
                {
                    wrongDistance = distance;
                    wrongSymbolName = reference.symbolName;
                    wrongDistributionDiference = distributionDiference;
                }
            }
            else
            {
                if (reference.isSymmetric)
                {
                    distance += distributionDiference;
                }
                if (distance < minDistance && distance <= reference.Threshold)
                {
                    minDistance = distance;
                    bestMatch = reference;
                    recognizedSymbolName = reference.symbolName;
                }
                else if (distance < minDistance && distance > reference.Threshold)
                {
                    wrongDistance = distance;
                    wrongSymbolName = reference.symbolName;
                }
            }


            Debug.Log($"Distancia de Zernike con '{reference.symbolName}': {distance}");


        }


        if(minDistance == double.MaxValue)
        {
            text.text = $"Símbolo no reconocido. Match más cercano: '{wrongSymbolName}' con distancia {wrongDistance:F3}";
            Debug.LogError("EEEEEEE ACAAAAAA");
            return;
        }
        // 3. VERIFICACIÓN FINAL DESPUÉS DE ENCONTRAR EL MEJOR MATCH DE FORMA
        if (bestMatch != null && minDistance < bestMatch.Threshold)
        {
            // La forma coincide, ahora verificamos la orientación.
            if(rotationSensitivity && bestMatch.useRotation)
            {
               
                text.text = $"Símbolo reconocido: {recognizedSymbolName}\nDistancia: {minDistance:F3}, Diferencia de distribución: {minDistributionDiference:F3}";

            }
            else
            {
                text.text = $"Símbolo reconocido: {recognizedSymbolName}\nDistancia: {minDistance:F3}";
            }

        }
        else
        {

            
            text.text = $"Símbolo no reconocido. Match más cercano: '{recognizedSymbolName}' con distancia {minDistance:F3}";
        }
    }
}


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

   // [HideInInspector]
    public float[] distribution;

 //   [HideInInspector]
    public List<double> momentMagnitudes;


    public ReferenceSymbol(string name, float[] rotDistribution, List<double> magnitudes,int strokesQ)
    {
        symbolName = name;
        distribution = rotDistribution;
        momentMagnitudes = magnitudes;
        strokes = strokesQ;
    }

    public Texture2D ResizeSelectedTextures(Texture2D originalTexture, int targetSize)
    {
        var selectedTextures = originalTexture;



        // Crea un nuevo RenderTexture y renderiza la textura original en él
        RenderTexture rt = new RenderTexture(targetSize, targetSize, 24);
        Graphics.Blit(selectedTextures, rt);

        // Crea una nueva Texture2D y lee los píxeles del RenderTexture
        Texture2D resizedTexture = new Texture2D(targetSize, targetSize);
        RenderTexture.active = rt;
        resizedTexture.ReadPixels(new Rect(0, 0, targetSize, targetSize), 0, 0);
        resizedTexture.Apply();


        // Guarda la nueva textura redimensionada como PNG
        byte[] bytes = resizedTexture.EncodeToPNG();


        return resizedTexture;
        //AssetDatabase.Refresh();
        //EditorUtility.DisplayDialog("Success", "Templates resized successfully!", "OK");
    }

}