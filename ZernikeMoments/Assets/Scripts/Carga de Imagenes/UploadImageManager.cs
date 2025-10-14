using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UploadImageManager : MonoBehaviour
{
    public static UploadImageManager instance;
    public RawImage uploadedImage;
    Texture2D tex;
    TMP_InputField simbolName;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        
    }

    
}
