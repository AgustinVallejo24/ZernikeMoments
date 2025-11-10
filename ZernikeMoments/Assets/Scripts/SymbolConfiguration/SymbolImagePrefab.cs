using UnityEngine;
using UnityEngine.UI;
using Delegates;
public class SymbolImagePrefab : MonoBehaviour
{
    public Button deleteButton;
    public ReferenceSymbol mySymbol;
    public RawImage image;
    public DeleteDelegate<ReferenceSymbol> deleteDelegate;
    void Start()
    {
        
    }
    public void DeleteSymbol()
    {
        deleteDelegate.Invoke(mySymbol);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
