using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ZernikeManager : MonoBehaviour
{
    // Tamaño de la imagen para el procesamiento
    public int imageSize = 64;
    // Orden máximo para los momentos de Zernike
    public int maxMomentOrder = 10;
    // Umbral de distancia para un reconocimiento exitoso
    public float recognitionThreshold = 0.5f;

    public RenderTexture renderTexture;
    // Almacena los descriptores (magnitudes de momentos) de los símbolos de referencia.
    [System.Serializable]
    public class ReferenceSymbol
    {
        public string symbolName;
        public List<float> momentMagnitudes;
        public Texture2D templateTexture;
        public float Threshold;
        public int strokes = 1;
    }
    public List<ReferenceSymbol> referenceSymbols;

    private ZernikeProcessor _processor;
    private List<Vector2> _currentStrokePoints;

    void Start()
    {
        _processor = new ZernikeProcessor(imageSize);
        _currentStrokePoints = new List<Vector2>();

        foreach (var reference in referenceSymbols)
        {
            if (reference.templateTexture != null)
            {
                // Procesar la textura para obtener la matriz binaria
                _processor.DrawTexture(reference.templateTexture);

                // Calcular la suma de todos los píxeles activos para la normalización
                float totalPixels = _processor.GetActivePixelCount();

                // Calcular los momentos
                ZernikeMoment[] moments = _processor.ComputeZernikeMoments(maxMomentOrder);

                reference.momentMagnitudes = new List<float>();
                // Normalizar y guardar las magnitudes
                foreach (var moment in moments)
                {
                    // Evitar división por cero
                    float normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
                    reference.momentMagnitudes.Add(normalizedMagnitude);
                }
            }
        }
    }
    Texture2D CaptureRenderTexture(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new UnityEngine.Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        return tex;
    }

    // Este método se llamaría cuando el jugador termina un trazo.
    //public void OnDrawingFinished(List<Vector2> finishedPoints)
    //{
    //    _currentStrokePoints = finishedPoints;
    //    RecognizeSymbol();
    //}

    public static Texture2D PreprocessTexture(Texture2D source, int size = 128, float threshold = 0.5f)
    {
        // 1. Crear copia en el tamaño deseado
        Texture2D resized = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = resized.GetPixels();

        // 2. Escalar la textura
        RenderTexture rt = RenderTexture.GetTemporary(size, size);
        Graphics.Blit(source, rt);
        RenderTexture.active = rt;
        resized.ReadPixels(new Rect(0, 0, size, size), 0, 0);
        resized.Apply();
        RenderTexture.ReleaseTemporary(rt);

        // 3. Convertir a blanco y negro con threshold
        Color[] bwPixels = resized.GetPixels();
        for (int i = 0; i < bwPixels.Length; i++)
        {
            float gray = bwPixels[i].grayscale;
            bwPixels[i] = gray > threshold ? Color.white : Color.black;
        }
        resized.SetPixels(bwPixels);
        resized.Apply();

        // Opcional: centrar el símbolo detectando su bounding box
        Texture2D centered = CenterSymbol(resized, size);

        return centered;
    }

    /// <summary>
    /// Centra el contenido blanco de la textura en un lienzo negro del mismo tamaño.
    /// </summary>
    private static Texture2D CenterSymbol(Texture2D tex, int size)
    {
        Color[] pixels = tex.GetPixels();

        int minX = size, minY = size, maxX = 0, maxY = 0;

        // Buscar bounding box del dibujo
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (pixels[y * size + x].r > 0.5f) // píxel blanco
                {
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }
        }

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        Texture2D cropped = new Texture2D(width, height);
        cropped.SetPixels(tex.GetPixels(minX, minY, width, height));
        cropped.Apply();

        // Redibujar centrado
        Texture2D result = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] clearPixels = new Color[size * size];
        for (int i = 0; i < clearPixels.Length; i++) clearPixels[i] = Color.black;
        result.SetPixels(clearPixels);

        int startX = (size - width) / 2;
        int startY = (size - height) / 2;
        result.SetPixels(startX, startY, width, height, cropped.GetPixels());
        result.Apply();

        return result;
    }
    public void OnDrawingFinished(List<Vector2> finishedPoints, int strokeQuantity)
    {
        _currentStrokePoints = finishedPoints;

        // Aquí también se debe normalizar el trazo del jugador
        _processor.DrawStroke(_currentStrokePoints);
        float totalPixels = _processor.GetActivePixelCount();
        ZernikeMoment[] playerMoments = _processor.ComputeZernikeMoments(maxMomentOrder);

        List<float> playerMagnitudes = new List<float>();
        foreach (var moment in playerMoments)
        {
            float normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
            playerMagnitudes.Add(normalizedMagnitude);
        }

        RecognizeSymbol(playerMagnitudes, strokeQuantity);
    }
    private void RecognizeSymbol(List<float> playerMagnitudes, int strokeQuantity)
    {
        float minDistance = float.MaxValue;
        string recognizedSymbol = "None";
        ReferenceSymbol mySymbol = new ReferenceSymbol();
        foreach (var reference in referenceSymbols.Where(x =>x.strokes == strokeQuantity))
        {
            float distanceSquared = 0f;
            int count = Mathf.Min(playerMagnitudes.Count, reference.momentMagnitudes.Count);
            
            for (int i = 0; i < count; i++)
            {
                float diff = playerMagnitudes[i] - reference.momentMagnitudes[i];
                distanceSquared += diff * diff;
            }
            Debug.Log("La distancia con "+ reference.symbolName+ " es " + Mathf.Sqrt(distanceSquared));
            if (distanceSquared < minDistance && distanceSquared <= Mathf.Pow(reference.Threshold,2))
            {
                minDistance = distanceSquared;
                recognizedSymbol = reference.symbolName;
                mySymbol = reference;
            }


        }

        float finalDistance = Mathf.Sqrt(minDistance);

        if (finalDistance < mySymbol.Threshold)
        {
            Debug.Log("Símbolo reconocido: " + recognizedSymbol + " con una distancia de " + finalDistance);
        }
        else
        {
            Debug.Log("Símbolo no reconocido. Distancia mínima: " + finalDistance);
        }
    }
}