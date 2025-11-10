using UnityEngine;
using System.IO;
using SFB;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using System.Text;

public static class FileBrowserLoader
{    
#if UNITY_WEBGL && !UNITY_EDITOR

#else
    // Opens the file selection window
    public static void OnBrowseButtonClick(RawImage sceneImage)
    {        
        ExtensionFilter[] extensions = new[]
        {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
        };
        
        string[] paths = StandaloneFileBrowser.OpenFilePanel(
            "Select Template Image",  
            "",                       
            extensions,               
            false                     
        );
        
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string filePath = paths[0];
            Debug.Log("File selected: " + filePath);
            
            LoadImageFromPath(filePath, sceneImage);
        }
    }

    // Method used when loading an image from files, to a RawImage.
    private static void LoadImageFromPath(string filePath, RawImage sceneImage)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found at: " + filePath);
            return;
        }
        try
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);

            if (texture.LoadImage(bytes))
            {
                Debug.Log("Template image loaded successfully.");           
                
                sceneImage.texture = texture;
            }
            else
            {
                Debug.LogError("Error: Failed to process file as a valid image.");
                Object.Destroy(texture);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("An error occurred while accessing the file: " + ex.Message);
        }
    }
#endif


}