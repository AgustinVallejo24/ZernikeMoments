using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;
using UnityEngine.Networking;

[RequireComponent(typeof(Button))]
public class CanvasSampleOpenFileImage : MonoBehaviour 
{
    public RawImage output;

    ExtensionFilter[] extensions = new[]
        {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
        };

#if UNITY_WEBGL && !UNITY_EDITOR
    //
    // WebGL
    //
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, ExtensionFilter[] filters, bool multiple);

    /*public void OnPointerDown(PointerEventData eventData) {       

        UploadFile(gameObject.name, "OnFileUpload", extensions, false);
    }*/

    /*private void OnClick() {        

        var paths = StandaloneFileBrowser.OpenFilePanel("Select Template Image", "", extensions, false);
        if (paths.Length > 0) {
            StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
        }
    }*/

    // Called from browser
    public void OnFileUpload(string url) {
        StartCoroutine(OutputRoutine(url));
    }
#else
    //
    // Standalone platforms & editor
    //
    //public void OnPointerDown(PointerEventData eventData) { }

    void Start() {
        //var button = GetComponent<Button>();
        //button.onClick.AddListener(OnClick);
    }

    private void OnClick() {        

        var paths = StandaloneFileBrowser.OpenFilePanel("Select Template Image", "", extensions, false);
        if (paths.Length > 0) {
            StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
        }
    }
#endif

    private IEnumerator OutputRoutine(string url) {
        
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error downloading texture: " + uwr.error);
            }
            else
            {
                // Get downloaded texture
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                // Apply the texture to a material
                if (output != null)
                {
                    output.texture = texture;
                }
                else
                {
                    Debug.LogWarning("No target Renderer assigned to apply the texture.");
                }
            }
        }
    }

    

   
}