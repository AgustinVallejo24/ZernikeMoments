using System.IO;
using UnityEngine;
using UnityEngine.UI;


public static class ImageUtils
{
    public static void SaveTexture(RawImage image, string templateId)
    {
        // 1. Obtener los bytes de la textura (formato PNG, por ejemplo)
        Texture2D texture = (Texture2D)image.texture;
        byte[] bytes = texture.EncodeToPNG();

        // 2. Definir la ruta de guardado. 
        // Se recomienda crear una subcarpeta para mantener organizado.
        string folderPath = Path.Combine(Application.persistentDataPath, "Images/TemplateImages");

        // Asegurarse de que el directorio exista
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Definir el nombre del archivo usando el ID y la extensión
        string fileName = templateId + ".png";
        string filePath = Path.Combine(folderPath, fileName);

        // 3. Escribir los bytes en el archivo
        try
        {
            File.WriteAllBytes(filePath, bytes);
            Debug.Log("Imagen guardada en: " + filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error al guardar la imagen: " + ex.Message);
        }
    }
    public static void SaveTexture(Texture2D texture, string templateId)
    {
        // 1. Obtener los bytes de la textura (formato PNG, por ejemplo)        
        byte[] bytes = texture.EncodeToPNG();

        // 2. Definir la ruta de guardado. 
        // Se recomienda crear una subcarpeta para mantener organizado.
        string folderPath = Path.Combine(Application.persistentDataPath, "Images/TemplateImages");

        // Asegurarse de que el directorio exista
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Definir el nombre del archivo usando el ID y la extensión
        string fileName = templateId + ".png";
        string filePath = Path.Combine(folderPath, fileName);

        // 3. Escribir los bytes en el archivo
        try
        {
            File.WriteAllBytes(filePath, bytes);
            Debug.Log("Imagen guardada en: " + filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error al guardar la imagen: " + ex.Message);
        }
    }
    public static Texture2D LoadTexture(string templateId)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "Images/TemplateImages");
        string fileName = templateId + ".png"; // Usar la misma extensión
        string filePath = Path.Combine(folderPath, fileName);

        if (File.Exists(filePath))
        {
            // 1. Leer los bytes del archivo
            byte[] bytes = File.ReadAllBytes(filePath);

            // 2. Crear una nueva Texture2D (debe ser legible)
            // El tamaño (1, 1) es temporal; se ajustará con LoadImage.
            
            Texture2D texture = new Texture2D(1, 1);

            // 3. Cargar los bytes en la Texture2D
            // LoadImage ajusta el tamaño de la textura automáticamente.
            if (texture.LoadImage(bytes))
            {
                Debug.Log("Imagen cargada desde: " + filePath);
                
                return texture;
            }
            else
            {
                Debug.LogError("Error al cargar la imagen en la Texture2D.");
                return null;
            }
        }
        else
        {
            Debug.LogWarning("Archivo de imagen no encontrado para el ID: " + templateId);
            return null;
        }
    }
}
