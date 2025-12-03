using UnityEngine;
using UnityEngine.UI;
public class TutorialManager : MonoBehaviour
{
    [SerializeField] Sprite[] _tutorialSprites;
    [SerializeField] Image _tutorialImage;
    [SerializeField] int _currentIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _currentIndex = 0;
        _tutorialImage.sprite = _tutorialSprites[0];
    }
    public void ChangeImage(int value)
    {
        if(value > 0)
        {
            _currentIndex++;
            if(_currentIndex >  _tutorialSprites.Length - 1)_currentIndex = 0;
            _tutorialImage.sprite = _tutorialSprites[_currentIndex];
        }
        else
        {
            _currentIndex--;
            if (_currentIndex < 0) _currentIndex = _tutorialSprites.Length - 1;
            _tutorialImage.sprite = _tutorialSprites[_currentIndex];
        }
    }


}
