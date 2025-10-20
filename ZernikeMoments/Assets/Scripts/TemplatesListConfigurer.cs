using UnityEngine;
using System.Collections.Generic;
public class TemplatesListConfigurer : MonoBehaviour
{
    [SerializeField] RectTransform _content;
    [SerializeField] GameObject _symbolImagePrefab;
    public bool isOpen;

    public void AddContent(List<ReferenceSymbol> symbols)
    {
        Debug.LogError("A");
        foreach (var item in symbols)
        {
          
            Instantiate(_symbolImagePrefab, _content);
        }
    }

    public void Dropdown()
    {
        isOpen = !isOpen;
        if (isOpen)
        {
            _content.gameObject.SetActive(true);
        }
        else
        {
            _content.gameObject.SetActive(false);
        }
    }
}
