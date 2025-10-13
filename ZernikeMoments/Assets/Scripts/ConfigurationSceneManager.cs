using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
public class ConfigurationSceneManager : MonoBehaviour
{
    [SerializeField] RectTransform _content;
    [SerializeField] SymbolConfigurer _symbolConfigurerPrefab;
    [SerializeField] VerticalLayoutGroup _layoutGroup;
    [SerializeField] TMP_InputField _nameInputField;
    [SerializeField] List<ReferenceSymbol> _symbolList;
    [SerializeField] List<SymbolConfigurer> _symbolConfigList;
    void Start()
    {
       var referenceSymbols = ReferenceSymbolStorage.LoadFromResources("symbols");
        _symbolList = referenceSymbols;
        foreach (var item in referenceSymbols)
        {
            SymbolConfigurer newSymbol = Instantiate(_symbolConfigurerPrefab, _content);
            newSymbol.SetSymbolValues(null, item.symbolName, item.Threshold, item.orientationThreshold, item.isSymmetric, item.useRotation);
            _symbolConfigList.Add(newSymbol);
        }
    }

    public void SaveSymbols()
    {

        for (int i = 0; i < _symbolList.Count; i++)
        {
            _symbolList[i].isSymmetric = _symbolConfigList[i].GetIsSymmetric();
            _symbolList[i].useRotation = _symbolConfigList[i].GetUseRotation();
            _symbolList[i].Threshold = _symbolConfigList[i].GetThresholdSliderValue();
            _symbolList[i].orientationThreshold = _symbolConfigList[i].GetRotationThresholdSliderValue();
        }
        ReferenceSymbolStorage.SaveSymbols(_symbolList, "symbols");

    }

    public void SearchSymbols()
    {
        if(_nameInputField.text == "")
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
            newSymbol.SetSymbolValues(null, item.symbolName, item.Threshold, item.orientationThreshold, item.isSymmetric, item.useRotation);
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
            newSymbol.SetSymbolValues(null, item.symbolName, item.Threshold, item.orientationThreshold, item.isSymmetric, item.useRotation);
        }
    }
}
