using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.IO;
using System.Collections;
public class DrawUpdloadingManager : MonoBehaviour
{

    [SerializeField] TMP_InputField _symbolNameInputField;
    [SerializeField] TMP_Text _instructionText;
    [SerializeField] TMP_Text _warningText;
    [SerializeField] Button _confirmSymbolButton;
    [SerializeField] Button _configSymbolButton;
    [SerializeField] GameObject _symbolConfigurerPrefab;
    [SerializeField] DrawingTest _drawingTest;
    [SerializeField] ZernikeManager _zernikeManager;
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] RectTransform _canvas;
    ReferenceSymbol _currentSymbol;
    GameObject _currentSymbolConfigurer;
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmDraw();
        }
    }
    public void ConfirmDraw()
    {

        if(_symbolNameInputField.text != "")
        {
            _currentSymbol = _drawingTest.ReturnNewSymbol(_symbolNameInputField.text,true);
            if(_currentSymbol == null)
            {
                _warningText.gameObject.SetActive(true);
                _warningText.text = "The line is too short";
            }
            else
            {
                _confirmSymbolButton.gameObject.SetActive(false);
                _symbolNameInputField.gameObject.SetActive(false);
                _instructionText.text = "Draw the symbol again";
                _configSymbolButton.gameObject.SetActive(true);
                _drawingTest.ClearAllLineRenderers(false);
            }
        }
        else
        {
            _warningText.gameObject.SetActive(true);
            StartCoroutine(Warning("A name is needed"));
        }
        

    }

    
    public void OpenSymbolConfiguration()
    {
        var newSymbol = _drawingTest.ReturnNewSymbol(_symbolNameInputField.text,false);
        var threshold = _zernikeManager.CalculateZernikeDistance(newSymbol.momentMagnitudes, _currentSymbol.momentMagnitudes) + .2f;
        if (_currentSymbol == null)
        {
            StartCoroutine(Warning("The line is too short"));

        }
        else
        {       
            _drawingTest.gameObject.SetActive(false);
            _configSymbolButton.gameObject.SetActive(false);
            _instructionText.gameObject.SetActive(false);
            _warningText.gameObject.SetActive(false);
            _currentSymbolConfigurer = Instantiate(_symbolConfigurerPrefab, _canvas);
            _currentSymbolConfigurer.GetComponentInChildren<Button>().onClick.AddListener(SaveSymbol);
            _currentSymbolConfigurer.GetComponentInChildren<SymbolConfigurer>().SetSymbolValues(ImageUtils.GetTexture2DCopy(renderTexture), _currentSymbol.symbolName, Convert.ToSingle(threshold), .1f, false, false);
        }
    }
    public void SaveSymbol()
    {
        StartCoroutine(SaveSymbolCoroutine());
    }


    IEnumerator Warning(string text)
    {
        
        _warningText.gameObject.SetActive(true);
        _warningText.text = text;
        yield return new WaitForSeconds(4f);
        _warningText.gameObject.SetActive(false);
    }

    IEnumerator SaveSymbolCoroutine()
    {
        var symbolConfig = _currentSymbolConfigurer.GetComponentInChildren<SymbolConfigurer>();
        _currentSymbol.Threshold = symbolConfig.GetThresholdFieldValue();
        _currentSymbol.orientationThreshold = symbolConfig.GetRotationThresholdFieldValue();
        _currentSymbol.useRotation = symbolConfig.GetUseRotation();
        _currentSymbol.isSymmetric = symbolConfig.GetIsSymmetric();
        string symbolID = Guid.NewGuid().ToString();
        ImageUtils.SaveTextureToPNG(symbolConfig.GetTexture(), symbolID);
        _currentSymbol.symbolID = symbolID;
        var symbolList = ReferenceSymbolStorage.LoadFromResources("symbols");
        symbolList.Add(_currentSymbol);

        ReferenceSymbolStorage.SaveSymbols(symbolList, Path.Combine(Application.dataPath, "Resources", "symbols.json"));
        ReferenceSymbolStorage.AppendSymbol(_currentSymbol, Path.Combine(Application.dataPath, "Resources", "drawnSymbols.json"));
        yield return new WaitForSeconds(.1f);
        Destroy(_currentSymbolConfigurer.gameObject);
        _drawingTest.ClearAllLineRenderers(true);

        _instructionText.text = "Draw the new symbol";
        _instructionText.gameObject.SetActive(true);
        _confirmSymbolButton.gameObject.SetActive(true);
        _symbolNameInputField.gameObject.SetActive(true);
        _symbolNameInputField.text = "";
        _drawingTest.gameObject.SetActive(true);
    }
}
