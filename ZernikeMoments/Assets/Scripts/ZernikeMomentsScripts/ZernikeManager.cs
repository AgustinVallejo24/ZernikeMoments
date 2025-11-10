using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
public class ZernikeManager : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    public int imageSize = 64;
    public int maxMomentOrder = 10;

    public float recognitionThreshold = 0.5f;

    public RenderTexture renderTexture;

    public bool rotationSensitivity;

    public GameObject UICarga;
    public Image barrita;
    public GameObject UIDibujo;
    public GameObject DrawingTest;
       
    public List<ReferenceSymbol> referenceSymbols;
    public List<ReferenceSymbolGroup> newReferenceSymbolsList;
    public ZernikeProcessor processor;
    public ZernikeRecognizer recognizer;
   
    public bool shouldLoad;
    string jsonPath;
       #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SyncFilesystem();
       #endif
    void Start()
    {
       
        jsonPath = Path.Combine(Application.persistentDataPath,"Saves", "symbols.json");
    
        processor = new ZernikeProcessor(imageSize);
        recognizer = new ZernikeRecognizer(rotationSensitivity,text,processor);

        if (PlayerPrefs.HasKey("shouldLoad"))
        {
            shouldLoad = false;
        }


        if (SceneManager.GetActiveScene().name == "Menu" && shouldLoad)
        {
            Debug.LogError("COMPUTEEEEEE");
            StartCoroutine(Compute());
            PlayerPrefs.SetInt("shouldLoad", 1);
  
        }
        else if(SceneManager.GetActiveScene().name != "Menu")
        {
            newReferenceSymbolsList.Clear();
            newReferenceSymbolsList = ReferenceSymbolStorage.LoadSymbols(Path.Combine(Application.persistentDataPath, "Saves", "symbols.json")).ToList();            
            UICarga.SetActive(false);
            UIDibujo.SetActive(true);
            DrawingTest.SetActive(true);
        }
        else 
        {
            UICarga.SetActive(false);
            UIDibujo.SetActive(true);
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayerPrefs.DeleteAll();
        }
    }

    public void SaveSymbolList()
    {
        ReferenceSymbolStorage.SaveSymbols(newReferenceSymbolsList, jsonPath);
    }
    IEnumerator Compute()
    {
        float carga = 0;

        foreach (var group in newReferenceSymbolsList)
        {
            foreach (var reference in group.symbols)
            {

                if (reference.templateTexture != null)
                {
                    reference.templateTexture = processor.ResizeImage(reference.templateTexture, 64);
                    reference.templateTexture.name = reference.symbolName;
                    Debug.Log(reference.templateTexture.height);
                    processor.DrawTexture(reference.templateTexture);

                    float totalPixels = processor.GetActivePixelCount();
                    Debug.Log("Divido por " + totalPixels);

                    ZernikeMoment[] moments = processor.ComputeZernikeMoments(maxMomentOrder);

                    reference.momentMagnitudes = new List<double>();
                    foreach (var moment in moments)
                    {
                        double normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
                        reference.momentMagnitudes.Add(normalizedMagnitude);
                    }

                    reference.distribution = processor.GetSymbolDistribution();

                    reference.symbolID = Guid.NewGuid().ToString();

                    ImageUtils.SaveTexture(reference.templateTexture, reference.symbolID);
                }
                carga += 1f / referenceSymbols.Count;


                barrita.fillAmount = carga;

                yield return null;
            }
        }
        
        UICarga.SetActive(false);
        UIDibujo.SetActive(true);
        yield return new WaitForSeconds(.2f);
        ReferenceSymbolStorage.SaveSymbols(newReferenceSymbolsList, jsonPath);

    }
    public void OnDrawingFinished(List<List<Vector2>> finishedPoints, int strokeQuantity)
    {

        processor.DrawStrokes(finishedPoints);
        float totalPixels = processor.GetActivePixelCount();
        ZernikeMoment[] playerMoments = processor.ComputeZernikeMoments(maxMomentOrder);
        float[] playerDistribution = processor.GetSymbolDistribution();
        List<double> playerMagnitudes = new List<double>();
        foreach (var moment in playerMoments)
        {
            double normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
            playerMagnitudes.Add(normalizedMagnitude);
        }

        RecognizeSymbol(playerMagnitudes, strokeQuantity, playerDistribution);
    }


    public void SaveSymbol(List<List<Vector2>> finishedPoints, int strokeQuantity, string symbolName, string symbolID)
    {
       
        processor.DrawStrokes(finishedPoints);
        float totalPixels = processor.GetActivePixelCount();
        ZernikeMoment[] playerMoments = processor.ComputeZernikeMoments(maxMomentOrder);
        float[] playerDistribution = processor.GetSymbolDistribution();
        List<double> playerMagnitudes = new List<double>();
        foreach (var moment in playerMoments)
        {
            double normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
            playerMagnitudes.Add(normalizedMagnitude);
        }
        
        ReferenceSymbol newSymbol = new ReferenceSymbol(symbolName, playerDistribution, playerMagnitudes, strokeQuantity, symbolID);

        referenceSymbols.Add(newSymbol);
        ReferenceSymbolStorage.SaveSymbols(newReferenceSymbolsList, jsonPath);
        
        
    }

    public ReferenceSymbol ReturnNewSymbol(List<List<Vector2>> finishedPoints, int strokeQuantity, string symbolName, string symbolID)
    {
        processor.DrawStrokes(finishedPoints);
        float totalPixels = processor.GetActivePixelCount();
        ZernikeMoment[] playerMoments = processor.ComputeZernikeMoments(maxMomentOrder);
        float[] playerDistribution = processor.GetSymbolDistribution();
        List<double> playerMagnitudes = new List<double>();
        foreach (var moment in playerMoments)
        {
            double normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
            playerMagnitudes.Add(normalizedMagnitude);
        }

        return new ReferenceSymbol(symbolName, playerDistribution, playerMagnitudes, strokeQuantity, symbolID);

    }

    private void RecognizeSymbol(List<double> playerMagnitudes, int strokeQuantity, float[] playerDrawDistribution)
    {
        var relevantSymbols = newReferenceSymbolsList.Where(x => x.strokes == strokeQuantity).ToList();

        if (relevantSymbols.Count == 0)
        {
            text.text = "No hay ningún símbolo con esa cantidad de trazos.";
            return;
        }

        recognizer.FindBestCandidate(playerMagnitudes, playerDrawDistribution, relevantSymbols, out ReferenceSymbolGroup bestMatch,
            out double minDistance, out float bestDistributionDiff, out ReferenceSymbolGroup closestMismatch, out double closestMismatchDist, out double closesetMismatchDistributionDist);

        recognizer.DisplayResult(bestMatch, minDistance, bestDistributionDiff, closestMismatch, closestMismatchDist, closesetMismatchDistributionDist);
    }


}


[System.Serializable]
public struct ReferenceSymbolGroup
{

    public string symbolName;
    public List<ReferenceSymbol> symbols;

    public float Threshold;
    public float orientationThreshold;
    public bool useRotation;
    public bool isSymmetric;
    public int strokes;
}