using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.IO;
public class ConfigurationSceneManager : MonoBehaviour
{
    
    [SerializeField] RectTransform _content;
    [SerializeField] SymbolConfigurer _symbolConfigurerPrefab;
    [SerializeField] VerticalLayoutGroup _layoutGroup;
    [SerializeField] TMP_InputField _nameInputField;
    [SerializeField] List<ReferenceSymbolGroup> _symbolList;
    [SerializeField] List<SymbolConfigurer> _symbolConfigList;
    public static ConfigurationSceneManager instance;
    public RectTransform canvas;
  
    void Start()
    {
        instance = this;
        var referenceSymbols = ReferenceSymbolStorage.LoadFromResources("symbols").Concat(ReferenceSymbolStorage.LoadFromResources("drawnSymbols")).ToList();
        _symbolList = referenceSymbols;
        foreach (var item in referenceSymbols)
        {
            SymbolConfigurer newSymbol = Instantiate(_symbolConfigurerPrefab, _content);
            newSymbol.canvas = canvas;
            Texture2D texture = ImageUtils.LoadTexture(item.symbols[0].symbolID);
         //   Texture2D texture = item.symbols[0].templateTexture;
            newSymbol.SetSymbolValues(texture, item.symbolName, item.Threshold, item.orientationThreshold, item.isSymmetric, item.useRotation,item.symbols,item);
            _symbolConfigList.Add(newSymbol);
            newSymbol.deleteGroup = DeleteGroup;
            newSymbol.deleteSymbol = DeleteSymbol;
        }
    }

    public void DeleteSymbol(ReferenceSymbol currentSymbol)
    {

        var symbols = _symbolList.Where(x => x.symbols.Contains(currentSymbol));
        if(symbols.Count() > 0)
        {
            symbols.First().symbols.Remove(currentSymbol);
        }
    }

    public void DeleteGroup(ReferenceSymbolGroup currentgroup)
    {
        if (_symbolList.Contains(currentgroup))
        {
            _symbolList.Remove(currentgroup);
        }
        else
        {
            Debug.Log("No Se encuentra");
        }
    }

    public void SaveSymbols()
    {

      

        for (int i = 0; i < _symbolList.Count; i++)
        {
            var currentGroup = _symbolList[i];
            currentGroup.isSymmetric = _symbolConfigList[i].GetIsSymmetric();
            currentGroup.useRotation = _symbolConfigList[i].GetUseRotation();
            currentGroup.Threshold = _symbolConfigList[i].GetThresholdSliderValue();
            currentGroup.orientationThreshold = _symbolConfigList[i].GetRotationThresholdSliderValue();
            _symbolList[i] = currentGroup;
        }
        ReferenceSymbolStorage.SaveSymbols(_symbolList, Path.Combine(Application.dataPath, "Resources", "symbols.json"));

    }

    public void SearchSymbols()
    {
        if (_nameInputField.text == "")
        {
            ClearFilters();
            return;
        }

        foreach (RectTransform item in _content)
        {
            Destroy(item.gameObject);
        }
        foreach (var item in _symbolList.Where(x => x.symbolName == _nameInputField.text))
        {
            SymbolConfigurer newSymbol = Instantiate(_symbolConfigurerPrefab, _content);
            newSymbol.SetSymbolValues(null, item.symbolName, item.Threshold, item.orientationThreshold, item.isSymmetric, item.useRotation, item.symbols,item);
        }
    }

    public void ClearFilters()
    {
        foreach (RectTransform item in _content)
        {
            Destroy(item.gameObject);
        }
        foreach (var item in _symbolList)
        {
            SymbolConfigurer newSymbol = Instantiate(_symbolConfigurerPrefab, _content);
            newSymbol.SetSymbolValues(null, item.symbolName, item.Threshold, item.orientationThreshold, item.isSymmetric, item.useRotation, item.symbols,item);
        }
    }
}
