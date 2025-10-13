using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SymbolConfigurer : MonoBehaviour
{
    [SerializeField] TMP_Text _symbolname;
    [SerializeField] Image _symbolImage;
    [SerializeField] TMP_InputField _thresholdInputField;
    [SerializeField] Slider _thresholdSlider;
    [SerializeField] TMP_InputField _rotationThresholdInputField;
    [SerializeField] Slider _rotationThresholdSlider;
    [SerializeField] Toggle _isSymmetric;
    [SerializeField] Toggle _UseRotation;

    private void Start()
    {
        _thresholdInputField.onValueChanged.AddListener(SetThresholdSliderValue);
        _thresholdSlider.onValueChanged.AddListener(SetThresholdFieldValue);
        _rotationThresholdInputField.onValueChanged.AddListener(SetRotationThresholdSliderValue);
        _rotationThresholdSlider.onValueChanged.AddListener(SetRotationThresholdFieldValue);
    }

    public void SetImage(Sprite texture)
    {
        _symbolImage.sprite = texture;
    }

    public void SetSimbolName(string name)
    {
        _symbolname.text = name;
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


    public float GetRotationThresholdSliderValue(string value)
    {
        return _rotationThresholdSlider.value;
    }
    public float GetRotationThresholdFieldValue(float value)
    {
        return float.Parse(_rotationThresholdInputField.text); 
    }

    public float GetThresholdSliderValue(string value)
    {
        return _thresholdSlider.value;
    }
    public float GetThresholdFieldValue(float value)
    {
        return float.Parse(_thresholdInputField.text);
    }

    public bool GetIsSymmetric()
    {
        return _isSymmetric;
    }
    public bool GetUseRotation()
    {
        return _UseRotation;
    }
}
