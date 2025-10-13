using UnityEngine;
using B83.Win32;

public class DragAndDropReceiver : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnDroppedFiles;
    }

    private void OnDroppedFiles(System.Collections.Generic.List<string> filePaths, POINT aDropPoint)
    {
        if (filePaths.Count > 0)
        {
            // Tomamos solo la primera ruta si se arrastraron múltiples archivos
            string filePath = filePaths[0];

            // Verificamos que sea un PNG (o el formato que necesites)
            if (filePath.ToLower().EndsWith(".png") || filePath.ToLower().EndsWith(".jpg") || filePath.ToLower().EndsWith(".jpeg"))
            {
                // PASO CLAVE: Llamar a la lógica de carga con la ruta
                ImageLoader.LoadImageFromPath(filePath, UploadImageManager.instance.uploadedImage);
            }
            else
            {
                Debug.LogWarning("Archivo soltado no es PNG, JPG o JPEG.");
            }
        }
    }

    // Update is called once per frame
    void OnDestroy()
    {
        UnityDragAndDropHook.UninstallHook();
    }
}
