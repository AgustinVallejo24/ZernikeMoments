using UnityEngine;
using System.IO;
using UnityEngine.UI;

public static class ImageLoader
{
    // NOTA: Esta clase estática requiere que le pases el GameObject o Componente 
    // donde quieres visualizar la textura. 
    public static void LoadImageFromPath(string filePath, RawImage sceneImage)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("Error: El archivo no existe en la ruta proporcionada.");
            return;
        }

        try
        {
            // 1. Leer los bytes del archivo
            byte[] bytes = File.ReadAllBytes(filePath);

            // 2. Crear una nueva Texture2D (el tamaño 1,1 es temporal)
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);

            // 3. Cargar los bytes en la Texture2D (esto redimensiona la textura)
            if (texture.LoadImage(bytes))
            {
                Debug.Log("Imagen de usuario cargada con éxito: " + filePath);

                // Aquí debes integrar la nueva 'texture' a tu sistema de templates.
                // Por ejemplo: myTemplateManager.AddNewTexture(texture, "NuevoID");
                sceneImage.texture = texture;
            }
            else
            {
                Debug.LogError("El archivo no pudo ser cargado como imagen válida.");
                Object.Destroy(texture); // Limpiar memoria
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Fallo al acceder al archivo: " + ex.Message);
        }
    }
}
