using UnityEngine;
using UnityEngine.UI;

public class UploadImageManager : MonoBehaviour
{
    public static UploadImageManager instance;
    public RawImage uploadedImage;
    Texture2D tex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
