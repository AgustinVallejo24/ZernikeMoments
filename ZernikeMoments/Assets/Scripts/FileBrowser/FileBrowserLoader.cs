using UnityEngine;
using System.IO;
using SFB;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using System.Text;

public static class FileBrowserLoader
{
    // M�todo a llamar al hacer clic en tu bot�n de "Browse"
#if UNITY_WEBGL && !UNITY_EDITOR

#else

    public static void OnBrowseButtonClick(RawImage sceneImage)
    {
        // Define qu� tipos de archivos se pueden seleccionar (solo PNG y JPG en este caso)
        ExtensionFilter[] extensions = new[]
        {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
        };

        // Abre el di�logo de selecci�n de archivo nativo del sistema operativo
        // El resultado es un array de rutas (el usuario puede seleccionar uno o varios)
        string[] paths = StandaloneFileBrowser.OpenFilePanel(
            "Select Template Image",  // T�tulo de la ventana
            "",                       // Ruta inicial (vac�o para usar la �ltima o la predeterminada del sistema)
            extensions,               // Filtros de extensi�n
            false                     // Permite la selecci�n de m�ltiples archivos (false = solo uno)
        );

        // Verifica si el usuario seleccion� un archivo
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string filePath = paths[0];
            Debug.Log("File selected: " + filePath);

            // Llama a la l�gica de carga de la imagen con la ruta obtenida
            LoadImageFromPath(filePath, sceneImage);
        }
    }

    // L�gica para cargar el archivo en una Texture2D (la misma que vimos)
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

                // Aqu� debes integrar la 'texture' a tu sistema
                // Por ejemplo: templateManager.SetNewTemplateTexture(texture);
                
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