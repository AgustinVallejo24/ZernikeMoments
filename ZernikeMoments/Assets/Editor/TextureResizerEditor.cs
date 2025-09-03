using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureResizerEditor : EditorWindow
{
    private static int _targetSize = 64;

    [MenuItem("Tools/Zernike/Resize Zernike Templates")]
    public static void ShowWindow()
    {
        GetWindow<TextureResizerEditor>("Resize Zernike Templates");
    }

    private void OnGUI()
    {
        GUILayout.Label("Selecciona las texturas de templates y haz clic en el botón para redimensionarlas a " + _targetSize + "x" + _targetSize + " píxeles.", EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Resize Selected Textures"))
        {
            ResizeSelectedTextures();
        }
    }

    private static void ResizeSelectedTextures()
    {
        var selectedTextures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);

        if (selectedTextures.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Por favor, selecciona al menos una textura en el Project panel.", "OK");
            return;
        }

        string outputPath = "Assets/ResizedTemplates/";
        if (!AssetDatabase.IsValidFolder(outputPath.Replace("Assets/", "")))
        {
            Directory.CreateDirectory(outputPath);
            AssetDatabase.Refresh();
        }

        foreach (var texture in selectedTextures)
        {
            string assetPath = AssetDatabase.GetAssetPath(texture);

            // Habilita temporalmente la lectura/escritura para la textura
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter == null) continue;

            bool wasReadable = textureImporter.isReadable;
            if (!wasReadable)
            {
                textureImporter.isReadable = true;
                AssetDatabase.ImportAsset(assetPath);
            }

            // Crea un nuevo RenderTexture y renderiza la textura original en él
            RenderTexture rt = new RenderTexture(_targetSize, _targetSize, 24);
            Graphics.Blit(texture, rt);

            // Crea una nueva Texture2D y lee los píxeles del RenderTexture
            Texture2D resizedTexture = new Texture2D(_targetSize, _targetSize);
            RenderTexture.active = rt;
            resizedTexture.ReadPixels(new Rect(0, 0, _targetSize, _targetSize), 0, 0);
            resizedTexture.Apply();

            // Vuelve a la textura original a su estado de lectura/escritura
            if (!wasReadable)
            {
                textureImporter.isReadable = false;
                AssetDatabase.ImportAsset(assetPath);
            }

            // Guarda la nueva textura redimensionada como PNG
            byte[] bytes = resizedTexture.EncodeToPNG();
            string newPath = outputPath + Path.GetFileNameWithoutExtension(assetPath) + ".png";
            File.WriteAllBytes(newPath, bytes);

            Debug.Log($"Resized texture saved to: {newPath}");
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Templates resized successfully!", "OK");
    }
    private static Texture2D ResizeSelectedTextures(Texture2D originalTexture)
    {
        var selectedTextures = originalTexture;

        if (selectedTextures == null)
        {
            EditorUtility.DisplayDialog("Error", "Por favor, selecciona al menos una textura en el Project panel.", "OK");
            return null;
        }

        string outputPath = "Assets/ResizedTemplates/";
        if (!AssetDatabase.IsValidFolder(outputPath.Replace("Assets/", "")))
        {
            Directory.CreateDirectory(outputPath);
            AssetDatabase.Refresh();
        }


            string assetPath = AssetDatabase.GetAssetPath(selectedTextures);

            // Habilita temporalmente la lectura/escritura para la textura
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter == null) return null;

            bool wasReadable = textureImporter.isReadable;
            if (!wasReadable)
            {
                textureImporter.isReadable = true;
                AssetDatabase.ImportAsset(assetPath);
            }

            // Crea un nuevo RenderTexture y renderiza la textura original en él
            RenderTexture rt = new RenderTexture(_targetSize, _targetSize, 24);
            Graphics.Blit(selectedTextures, rt);

            // Crea una nueva Texture2D y lee los píxeles del RenderTexture
            Texture2D resizedTexture = new Texture2D(_targetSize, _targetSize);
            RenderTexture.active = rt;
            resizedTexture.ReadPixels(new Rect(0, 0, _targetSize, _targetSize), 0, 0);
            resizedTexture.Apply();

            // Vuelve a la textura original a su estado de lectura/escritura
            if (!wasReadable)
            {
                textureImporter.isReadable = false;
                AssetDatabase.ImportAsset(assetPath);
            }

            // Guarda la nueva textura redimensionada como PNG
            byte[] bytes = resizedTexture.EncodeToPNG();
            string newPath = outputPath + Path.GetFileNameWithoutExtension(assetPath) + ".png";
            File.WriteAllBytes(newPath, bytes);

            Debug.Log($"Resized texture saved to: {newPath}");

        return resizedTexture;
        //AssetDatabase.Refresh();
        //EditorUtility.DisplayDialog("Success", "Templates resized successfully!", "OK");
    }
}