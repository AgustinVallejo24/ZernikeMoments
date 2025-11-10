using System.IO;
using UnityEngine;
using UnityEngine.UI;


public static class ImageUtils
{
    // Saves the given texture.
    public static void SaveTexture(Texture2D texture, string templateId, bool external = false)
    {                
        byte[] bytes = texture.EncodeToPNG();
                
        string folderPath2 = Path.Combine(Application.persistentDataPath, "Images/TemplateImages/InProjectTemplates");

        if (external)
        {            
            folderPath2 = Path.Combine(Application.persistentDataPath, "Images/TemplateImages/ExternalTemplates");
        }
        
        if (!Directory.Exists(folderPath2))
        {
            Directory.CreateDirectory(folderPath2);
        }
        
        string fileName = templateId + ".png";
        string filePath = Path.Combine(folderPath2, fileName);
        
        try
        {
            File.WriteAllBytes(filePath, bytes);
            Debug.Log("Image saved to: " + filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error while saving the image: " + ex.Message);
        }
    }

    // Loads a texture, given an ID.
    public static Texture2D LoadTexture(string templateId)
    {        
        string folderPath2 = Path.Combine(Application.persistentDataPath, "Images/TemplateImages/InProjectTemplates");
        
        string folderPath3 = Path.Combine(Application.persistentDataPath, "Images/TemplateImages/DrawnTemplates");
        
        string folderPath4 = Path.Combine(Application.persistentDataPath, "Images/TemplateImages/ExternalTemplates");

        string fileName = templateId + ".png"; 
        string filePath = "";

        if (File.Exists(Path.Combine(folderPath2, fileName)))
        {
            filePath = Path.Combine(folderPath2, fileName);
            
            byte[] bytes = File.ReadAllBytes(filePath);            
            
            Texture2D texture = new Texture2D(1, 1);
            
            if (texture.LoadImage(bytes))
            {
                Debug.Log("Image loaded from: " + filePath);
                
                return texture;
            }
            else
            {
                Debug.LogError("Error while loading the image into a Texture2D.");
                return null;
            }
        }
        else if (File.Exists(Path.Combine(folderPath3, fileName)))
        {
            filePath = Path.Combine(folderPath3, fileName);
            
            byte[] bytes = File.ReadAllBytes(filePath);            

            Texture2D texture = new Texture2D(1, 1);
            
            if (texture.LoadImage(bytes))
            {
                Debug.Log("Image loaded from: " + filePath);

                return texture;
            }
            else
            {
                Debug.LogError("Error while loading the image into a Texture2D.");
                return null;
            }
        }
        else if (File.Exists(Path.Combine(folderPath4, fileName)))
        {
            filePath = Path.Combine(folderPath4, fileName);
            
            byte[] bytes = File.ReadAllBytes(filePath);            

            Texture2D texture = new Texture2D(1, 1);
            
            if (texture.LoadImage(bytes))
            {
                Debug.Log("Image loaded from: " + filePath);

                return texture;
            }
            else
            {
                Debug.LogError("Error while loading the image into a Texture2D.");
                return null;
            }
        }
        else
        {
            Debug.LogWarning("File not found for ID: " + templateId);
            return null;
        }
    }

    // Saves a texture, given a RenderTexture.
    public static void SaveRenderTextureToPNG(RenderTexture renderTexture, string fileName = "PlayerDrawing")
    {        
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        
        RenderTexture.active = currentRT;
        
        byte[] bytes = texture.EncodeToPNG();
        
        string path2 = Path.Combine(Application.persistentDataPath, "Images/TemplateImages/DrawnTemplates");

        if (!Directory.Exists(path2))
        {
            Directory.CreateDirectory(path2);
        }

        string fileNameID = fileName + ".png";
        string filePath = Path.Combine(path2, fileNameID);

        try
        {
            File.WriteAllBytes(filePath, bytes);
            Debug.Log("Image saved in: " + filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error while saving the image: " + ex.Message);
        }        
    }

    // Saves the given drawn texture.
    public static void SaveTextureToPNG(Texture2D texture, string fileName = "PlayerDrawing")
    {        
        byte[] bytes = texture.EncodeToPNG();
        
        string path = Path.Combine(Application.persistentDataPath, "Images/TemplateImages/DrawnTemplates");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        string filePath = Path.Combine(path, fileName + ".png");
        
        try
        {
            File.WriteAllBytes(filePath, bytes);
            Debug.Log("Image saved in: " + filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error while saving the image: " + ex.Message);
        }
    }

    // Creates a Texture2D from a RenderTexture.
    public static Texture2D GetTexture2DCopy(RenderTexture renderTexture)
    {
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        RenderTexture.active = currentRT;

        return texture;
    }
    
    // Loads an image to a RawImage.
    public static void LoadImageFromPath(string filePath, RawImage sceneImage)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("Error: The file does not exist in the given path.");
            return;
        }
        try
        {            
            byte[] bytes = File.ReadAllBytes(filePath);
            
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            
            if (texture.LoadImage(bytes))
            {
                Debug.Log("User image was succesefully loaded: " + filePath);
                
                sceneImage.texture = texture;
            }
            else
            {
                Debug.LogError("The file couldn't be loaded as a valid texture");
                Object.Destroy(texture); 
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Acces failed to file: " + ex.Message);
        }
    }

}
