using UnityEngine;
using System.Collections.Generic;
using Delegates;
public class TemplatesListConfigurer : MonoBehaviour
{
    [SerializeField] RectTransform _content;
    [SerializeField] SymbolImagePrefab _symbolImagePrefab;
 
    public bool isOpen;

    public void AddContent(List<ReferenceSymbol> symbols, DeleteDelegate<ReferenceSymbol> newDeleteDelegate)
    {
        Debug.LogError("A");
        foreach (var item in symbols)
        {
            var imagePrefab = Instantiate(_symbolImagePrefab, _content);
            imagePrefab.deleteDelegate = newDeleteDelegate;
            imagePrefab.mySymbol = item;
            imagePrefab.image.texture = ImageUtils.LoadTexture(item.symbolID);
        }
    }

    
    public void DestroySelf()
    {
        Destroy(gameObject);
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
