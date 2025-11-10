using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using Delegates;
public class SymbolConfigurer : MonoBehaviour
{
    [SerializeField] ReferenceSymbolGroup myGroup;
    [SerializeField] TMP_Text _symbolname;
    [SerializeField] RawImage _symbolImage;
    [SerializeField] TMP_InputField _thresholdInputField;
    [SerializeField] Slider _thresholdSlider;
    [SerializeField] TMP_InputField _rotationThresholdInputField;
    [SerializeField] Slider _rotationThresholdSlider;
    [SerializeField] Toggle _isSymmetric;
    [SerializeField] Toggle _UseRotation;
    [SerializeField] TemplatesListConfigurer _listConfigurer;
    [SerializeField] List<ReferenceSymbol> _symbols;
    public RectTransform canvas;

    public DeleteDelegate<ReferenceSymbol> deleteSymbol;
    public DeleteDelegate<ReferenceSymbolGroup> deleteGroup;
    private void Start()
    {
        _thresholdInputField.onValueChanged.AddListener(SetThresholdSliderValue);
        _thresholdSlider.onValueChanged.AddListener(SetThresholdFieldValue);
        _rotationThresholdInputField.onValueChanged.AddListener(SetRotationThresholdSliderValue);
        _rotationThresholdSlider.onValueChanged.AddListener(SetRotationThresholdFieldValue);
  
    }

    public void SetSymbolList()
    {
        var list = Instantiate(_listConfigurer, canvas);
        list.AddContent(_symbols,deleteSymbol);
    }


    public void DeleteGroup()
    {
        deleteGroup.Invoke(myGroup);
        Destroy(gameObject);
    }
    public void SetSymbolValues(Texture2D texture, string name, float threshold, float rotationThreshold, bool symmetric, bool rotation, List<ReferenceSymbol> symbols,ReferenceSymbolGroup group)
    {
        SetImage(texture);
        SetSimbolName(name);
        SetThresholdFieldValue(threshold);
        SetThresholdSliderValue(threshold.ToString());
        SetRotationThresholdFieldValue(rotationThreshold);
        SetRotationThresholdSliderValue(rotationThreshold.ToString());
        SetsSymmetric(symmetric);
        SetUseRotation(rotation);
        _symbols = symbols;
        SetGroup(group);
      //  SetSymbolList(symbols);
    }

    public void SetGroup(ReferenceSymbolGroup group)
    {
        myGroup = group;
    }
    public void SetImage(Texture2D texture)
    {
        _symbolImage.texture = texture;
    }

    public Texture2D GetTexture()
    {
        return _symbolImage.texture as Texture2D;
    }
    public void SetSimbolName(string name)
    {
        _symbolname.text = name;
    }
    public string GetSymbolName()
    {
       return _symbolname.text;
    }
    public string GetInputFieldValue(InputField field)
    {
        return field.text;
    }
    
    public void SetSliderValue(Slider slider, string value)
    {
        slider.value = float.Parse(value);
    }
    public void SetThresholdSliderValue(string value)
    {
        _thresholdSlider.value = float.Parse(value);
    }
    public void SetThresholdFieldValue(float value)
    {
        _thresholdInputField.text = value.ToString();
    }
    public void SetRotationThresholdSliderValue(string value)
    {
        _rotationThresholdSlider.value = float.Parse(value);
    }
    public void SetRotationThresholdFieldValue(float value)
    {
        _rotationThresholdInputField.text = value.ToString();
    }


    public float GetRotationThresholdSliderValue()
    {
        return _rotationThresholdSlider.value;
    }
    public float GetRotationThresholdFieldValue()
    {
        return float.Parse(_rotationThresholdInputField.text); 
    }

    public float GetThresholdSliderValue()
    {
        return _thresholdSlider.value;
    }
    public float GetThresholdFieldValue()
    {
        return float.Parse(_thresholdInputField.text);
    }

    public bool GetIsSymmetric()
    {
        return _isSymmetric.isOn;
    }
    public bool GetUseRotation()
    {
        return _UseRotation.isOn;
    }

    public void SetsSymmetric(bool value)
    {
       _isSymmetric.isOn = value;
    }
    public void SetUseRotation(bool value)
    {
        _UseRotation.isOn = value;
    }
}
