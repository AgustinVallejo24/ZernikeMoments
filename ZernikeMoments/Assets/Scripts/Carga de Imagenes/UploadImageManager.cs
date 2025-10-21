using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class UploadImageManager : MonoBehaviour
{
    public static UploadImageManager instance;
    public RawImage uploadedImage;
    public Texture2D tex;
    [SerializeField]TMP_InputField _symbolNameF;
    string _symbolName;
    [SerializeField] GameObject _uploadCanvas;
    [SerializeField] GameObject _configurationCanvas;
    [SerializeField] SymbolConfigurer _symbolConfigurer;
    [SerializeField] TMP_InputField _symbolStrokeQ;
    [SerializeField] TMP_Text _warningText;
    public int maxMomentOrder = 10;
    [SerializeField] ZernikeManager _zernikeManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        
    }

    public void OnBrowseButtonClick()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
#else
        FileBrowserLoader.OnBrowseButtonClick(uploadedImage);
#endif

    }

    public void GetImageTexture()
    {
        tex = (Texture2D)uploadedImage.texture;
    }

    public void GetName()
    {
        _symbolName = _symbolNameF.text;
    }

    public void GoToConfiguration()
    {
        if(_symbolNameF.text != "" && uploadedImage.texture != null)
        {
            _warningText.gameObject.SetActive(false);
            GetImageTexture();
            GetName();
            _symbolConfigurer.SetImage(tex);
            _symbolConfigurer.SetSimbolName(_symbolName);

            _uploadCanvas.SetActive(false);
            _configurationCanvas.SetActive(true);
        }
        else
        {
            _warningText.gameObject.SetActive(true);
            StartCoroutine(Warning("A name or an image is needed"));
        }
        
        
    }

    public void ReturnToUplodingImage()
    {
        _warningText.gameObject.SetActive(false);
        _configurationCanvas.SetActive(false);
        _uploadCanvas.SetActive(true);
        _symbolConfigurer.SetsSymmetric(false);
        _symbolConfigurer.SetUseRotation(false);
        _symbolConfigurer.SetThresholdFieldValue(0);
        _symbolConfigurer.SetRotationThresholdFieldValue(0);
        _symbolStrokeQ.text = "";
    }

    public void SaveSymbol()
    {
        if(_symbolStrokeQ.text != "" && int.Parse(_symbolStrokeQ.text) > 0)
        {
            ReferenceSymbolGroup symbolGroup = default;
            if(_zernikeManager.newReferenceSymbolsList.Where(x => x.symbolName == _symbolName).Any())
            {
                symbolGroup = _zernikeManager.newReferenceSymbolsList.Where(x => x.symbolName == _symbolName).First();
                
            }
            else
            {
                symbolGroup = new ReferenceSymbolGroup();
                symbolGroup.symbolName = _symbolName;
                symbolGroup.strokes = int.Parse(_symbolStrokeQ.text);
                symbolGroup.isSymmetric = _symbolConfigurer.GetIsSymmetric();
                symbolGroup.useRotation = _symbolConfigurer.GetUseRotation();
                symbolGroup.Threshold = _symbolConfigurer.GetThresholdFieldValue();
                symbolGroup.orientationThreshold = _symbolConfigurer.GetRotationThresholdFieldValue();

                _zernikeManager.newReferenceSymbolsList.Add(symbolGroup);
            }
                
            ReferenceSymbol newSymbol = new ReferenceSymbol(symbolGroup.symbolName, default, default, int.Parse(_symbolStrokeQ.text), Guid.NewGuid().ToString());
        // Procesar la textura para obtener la matriz binaria        
            //reference.templateTexture.name = reference.symbolName;
            //Debug.Log(reference.templateTexture.height);
            
            tex = _zernikeManager._processor.ResizeImage(tex, 64);
            _zernikeManager._processor.DrawTexture(tex);
            newSymbol.templateTexture = tex;
            // Calcular la suma de todos los píxeles activos para la normalización
            float totalPixels = _zernikeManager._processor.GetActivePixelCount();
            Debug.Log("Divido por " + totalPixels);

            ZernikeMoment[] moments = _zernikeManager._processor.ComputeZernikeMoments(maxMomentOrder);

            newSymbol.momentMagnitudes = new List<double>();
            // Normalizar y guardar las magnitudes
            foreach (var moment in moments)
            {
                // Evitar división por cero
                double normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
                newSymbol.momentMagnitudes.Add(normalizedMagnitude);
            }

            newSymbol.distribution = _zernikeManager._processor.GetSymbolDistribution();           

            ImageUtils.SaveTexture(newSymbol.templateTexture, newSymbol.symbolID, true);

            symbolGroup.symbols.Add(newSymbol);
        }
        else
        {
            _warningText.gameObject.SetActive(true);
            StartCoroutine(Warning("The Stroke Quantity must be greater than 0"));
        }
        
    }

    public void GoToMenu()
    {

    }

    IEnumerator Warning(string text)
    {

        _warningText.gameObject.SetActive(true);
        _warningText.text = text;
        yield return new WaitForSeconds(4f);
        _warningText.gameObject.SetActive(false);
    }
}
