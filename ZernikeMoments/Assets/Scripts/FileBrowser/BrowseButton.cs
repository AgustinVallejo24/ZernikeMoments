using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine.Networking;

public class BrowseButton : MonoBehaviour
{
    [SerializeField] RawImage imageTarget;

    public void CargarImagenDesdeWeb(string base64)
    {
        StartCoroutine(LoadImageCoroutine(base64));
    }

    private IEnumerator LoadImageCoroutine(string base64)
    {
        byte[] imageBytes = System.Convert.FromBase64String(base64.Replace("data:image/png;base64,", "").Replace("data:image/jpeg;base64,", ""));
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageBytes);
        imageTarget.texture = tex;
        yield return null;
    }

    public void AbrirSelectorDeArchivos()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenFileDialog();
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void OpenFileDialog();
#endif
}
