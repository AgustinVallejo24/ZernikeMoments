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
  
    void Start()
    {
        var referenceSymbols = ReferenceSymbolStorage.LoadFromResources("symbols").Concat(ReferenceSymbolStorage.LoadFromResources("drawnSymbols")).ToList();
        _symbolList = referenceSymbols;
        foreach (var item in referenceSymbols)
        {
            SymbolConfigurer newSymbol = Instantiate(_symbolConfigurerPrefab, _content);

            //Texture2D texture = ImageUtils.LoadTexture(item.symbolID);
            Texture2D texture = default;
            newSymbol.SetSymbolValues(texture, item.symbolName, item.Threshold, item.orientationThreshold, item.isSymmetric, item.useRotation,item.symbols);
            _symbolConfigList.Add(newSymbol);
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
            newSymbol.SetSymbolValues(null, item.symbolName, item.Threshold, item.orientationThreshold, item.isSymmetric, item.useRotation, item.symbols);
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
            newSymbol.SetSymbolValues(null, item.symbolName, item.Threshold, item.orientationThreshold, item.isSymmetric, item.useRotation, item.symbols);
        }
    }
}
