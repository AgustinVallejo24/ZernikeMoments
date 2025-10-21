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
        //string folderPath = Path.Combine(Application.persistentDataPath, "Images/TemplateImages");
        string folderPath2 = Path.Combine(Application.dataPath, "Resources/Template Images/ExternalTemplates");

        // Asegurarse de que el directorio exista
        if (!Directory.Exists(folderPath2))
        {
            Directory.CreateDirectory(folderPath2);
        }

        // Definir el nombre del archivo usando el ID y la extensión
        string fileName = templateId + ".png";
        string filePath = Path.Combine(folderPath2, fileName);

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
    public static void SaveTexture(Texture2D texture, string templateId, bool external = false)
    {
        // 1. Obtener los bytes de la textura (formato PNG, por ejemplo)        
        byte[] bytes = texture.EncodeToPNG();

        // 2. Definir la ruta de guardado. 
        // Se recomienda crear una subcarpeta para mantener organizado.
        //string folderPath = Path.Combine(Application.persistentDataPath, "Images/TemplateImages");
        string folderPath2 = Path.Combine(Application.dataPath, "Resources/Template Images/InProjectTemplates");

        if (external)
        {
            folderPath2 = Path.Combine(Application.dataPath, "Resources/Template Images/ExternalTemplates");
        }

        // Asegurarse de que el directorio exista
        if (!Directory.Exists(folderPath2))
        {
            Directory.CreateDirectory(folderPath2);
        }

        // Definir el nombre del archivo usando el ID y la extensión
        string fileName = templateId + ".png";
        string filePath = Path.Combine(folderPath2, fileName);

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
        //string folderPath = Path.Combine(Application.persistentDataPath, "Images/TemplateImages");
        string folderPath2 = Path.Combine(Application.dataPath, "Resources/Template Images/InProjectTemplates");
        string folderPath3 = Path.Combine(Application.dataPath, "Resources/Template Images/DrawnTemplates");
        string folderPath4 = Path.Combine(Application.dataPath, "Resources/Template Images/ExternalTemplates");
        string fileName = templateId + ".png"; // Usar la misma extensión
        string filePath = "";

        if (File.Exists(Path.Combine(folderPath2, fileName)))
        {
            filePath = Path.Combine(folderPath2, fileName);
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
        else if (File.Exists(Path.Combine(folderPath3, fileName)))
        {
            filePath = Path.Combine(folderPath3, fileName);
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
        else if (File.Exists(Path.Combine(folderPath4, fileName)))
        {
            filePath = Path.Combine(folderPath4, fileName);
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

    public static void SaveRenderTextureToPNG(RenderTexture renderTexture, string fileName = "PlayerDrawing")
    {
        // 1. Crear una Texture2D del mismo tamaño que la RenderTexture
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        // 2. Activar temporalmente la RenderTexture para leer sus píxeles
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        // 3. Leer los píxeles desde la RenderTexture
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        // 4. Restaurar la RenderTexture activa anterior
        RenderTexture.active = currentRT;

        // 5. Guardar la textura como PNG (opcional)
        byte[] bytes = texture.EncodeToPNG();
        //string path = Path.Combine(Application.persistentDataPath, "Images/TemplateImages", fileName + ".png");
        string path2 = Path.Combine(Application.dataPath, "Resources/Template Images/DrawnTemplates");

        if (!Directory.Exists(path2))
        {
            Directory.CreateDirectory(path2);
        }

        string fileNameID = fileName + ".png";
        string filePath = Path.Combine(path2, fileNameID);

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


    public static void SaveTextureToPNG(Texture2D texture, string fileName = "PlayerDrawing")
    {
        // 1. Convertir la textura a PNG
        byte[] bytes = texture.EncodeToPNG();

        // 2. Crear la carpeta si no existe
        string path = Path.Combine(Application.dataPath, "Resources/Template Images/DrawnTemplates");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // 3. Generar el nombre de archivo
        string filePath = Path.Combine(path, fileName + ".png");

        // 4. Guardar la textura en disco
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

}
