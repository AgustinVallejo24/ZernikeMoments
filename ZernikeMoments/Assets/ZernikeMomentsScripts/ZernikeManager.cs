using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
public class ZernikeManager : MonoBehaviour
{
    // Tamaño de la imagen para el procesamiento
    [SerializeField] TMP_Text text;
    public int imageSize = 64;
    // Orden máximo para los momentos de Zernike
    public int maxMomentOrder = 10;
    // Umbral de distancia para un reconocimiento exitoso
    public float recognitionThreshold = 0.5f;

    public RenderTexture renderTexture;

    public bool rotationSensitivity;
    // Almacena los descriptores (magnitudes de momentos) de los símbolos de referencia.
    [System.Serializable]
    public class ReferenceSymbol
    {
        public string symbolName;
        public List<double> momentMagnitudes;
        public Texture2D templateTexture;
        public float Threshold;
        public int strokes = 1;
        public float orientationThreshold;
        [HideInInspector]
        public float[] distribution;
        
    }
    public List<ReferenceSymbol> referenceSymbols;

    private ZernikeProcessor _processor;
    private List<Vector2> _currentStrokePoints;

    void Start()
    {
        _processor = new ZernikeProcessor(imageSize);
        _currentStrokePoints = new List<Vector2>();

        StartCoroutine(Compute());
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

    IEnumerator Compute()
    {
        foreach (var reference in referenceSymbols)
        {
            if (reference.templateTexture != null)
            {
                // Procesar la textura para obtener la matriz binaria
                _processor.DrawTexture(reference.templateTexture);

                // Calcular la suma de todos los píxeles activos para la normalización
                float totalPixels = _processor.GetActivePixelCount();
                Debug.Log("Divido por " + totalPixels);
               
                ZernikeMoment[] moments = _processor.ComputeZernikeMoments(maxMomentOrder);

                reference.momentMagnitudes = new List<double>();
                // Normalizar y guardar las magnitudes
                foreach (var moment in moments)
                {
                    // Evitar división por cero
                    double normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
                    reference.momentMagnitudes.Add(normalizedMagnitude);
                }

                reference.distribution = _processor.GetSymbolDistribution();
            }
            yield return null;
        }

    }
    public void OnDrawingFinished(List<Vector2> finishedPoints, int strokeQuantity)
    {
        _currentStrokePoints = finishedPoints;

        _processor.DrawStroke(_currentStrokePoints);
        float totalPixels = _processor.GetActivePixelCount();
        ZernikeMoment[] playerMoments = _processor.ComputeZernikeMoments(maxMomentOrder);
        float[] playerDistribution = _processor.GetSymbolDistribution();
        List<double> playerMagnitudes = new List<double>();
        foreach (var moment in playerMoments)
        {
            double normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
            playerMagnitudes.Add(normalizedMagnitude);
        }

        RecognizeSymbol(playerMagnitudes, strokeQuantity, playerDistribution);
    }

    private void RecognizeSymbol(List<double> playerMagnitudes, int strokeQuantity, float[] playerDrawDistribution)
    {
        double minDistance = double.MaxValue;
        string recognizedSymbolName = "None";
        ReferenceSymbol bestMatch = null;
 
        // --- NUEVO: Calcular la orientación del dibujo del jugador ---
        // float playerOrientationDegrees = playerOrientationRadians * Mathf.Rad2Deg;

        foreach (var reference in referenceSymbols.Where(x => x.strokes == strokeQuantity))
        {
            // 1. CÁLCULO DE DISTANCIA (Zernike) - sin cambios
            double distanceSquared = 0; // Usar double para consistencia
            int count = Mathf.Min(playerMagnitudes.Count, reference.momentMagnitudes.Count);

            for (int i = 0; i < count; i++)
            {
                double diff = playerMagnitudes[i] - reference.momentMagnitudes[i];
                distanceSquared += diff * diff;
            }

            double distance = Math.Sqrt(distanceSquared);
            Debug.Log($"Distancia de Zernike con '{reference.symbolName}': {distance}");

            // 2. COMPROBACIÓN COMBINADA
            if (distance < minDistance)
            {
                minDistance = distance;
                bestMatch = reference;
                recognizedSymbolName = reference.symbolName;
            }
        }

        // 3. VERIFICACIÓN FINAL DESPUÉS DE ENCONTRAR EL MEJOR MATCH DE FORMA
        if (bestMatch != null && minDistance < bestMatch.Threshold)
        {
            // La forma coincide, ahora verificamos la orientación.
            if(rotationSensitivity && bestMatch.orientationThreshold<360)
            {
                float distributionDiference = _processor.CompareAngularHistograms(bestMatch.distribution, playerDrawDistribution);

                if (distributionDiference <= bestMatch.orientationThreshold)
                {
                    text.text = $"Símbolo reconocido: {recognizedSymbolName}\nDistancia: {minDistance:F3}, Diferencia de distribución: {distributionDiference:F3}";
                }
                else
                {
                    text.text = $"Casi era un '{recognizedSymbolName}', Distancia: {minDistance:F3}, pero la rotación no coincide. ( distribución: {distributionDiference:F3})";
                }
            }
            else
            {
                text.text = $"Símbolo reconocido: {recognizedSymbolName}\nDistancia: {minDistance:F3}";
            }

        }
        else
        {
            text.text = $"Símbolo no reconocido. Match más cercano: '{recognizedSymbolName}' con distancia {minDistance:F3}";
        }
    }
}