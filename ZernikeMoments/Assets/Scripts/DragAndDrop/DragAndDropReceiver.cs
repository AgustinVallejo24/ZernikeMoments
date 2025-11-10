using UnityEngine;
using B83.Win32;

public class DragAndDropReceiver : MonoBehaviour
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    
    void Start()
    {
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnDroppedFiles;
    }

    // Detects the file droped and wether it is an image.
    private void OnDroppedFiles(System.Collections.Generic.List<string> filePaths, POINT aDropPoint)
    {
        if (filePaths.Count > 0)
        {            
            string filePath = filePaths[0];
            
            if (filePath.ToLower().EndsWith(".png") || filePath.ToLower().EndsWith(".jpg") || filePath.ToLower().EndsWith(".jpeg"))
            {                
                ImageUtils.LoadImageFromPath(filePath, UploadImageManager.instance.uploadedImage);
            }
            else
            {
                Debug.LogWarning("File droped is not a PNG, JPG or JPEG.");
            }
        }
    }    
    void OnDestroy()
    {
        UnityDragAndDropHook.UninstallHook();
    }
#endif
}
