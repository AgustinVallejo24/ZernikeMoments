using UnityEngine;

public static class ImageProcessor
{
    public static Texture2D ProcessTextureConditional(Texture2D originalTexture)
    {
        bool needsTransparency = !ImageAnalyzer.IsTransparent(originalTexture);
        bool needsGrayscale = !ImageAnalyzer.IsGrayscale(originalTexture);

        // Caso 1: No necesita NING�N proceso extra
        if (!needsTransparency && !needsGrayscale)
        {
            Debug.Log("La textura ya cumple ambas condiciones.");
            return originalTexture;
        }

        // Crear la textura de destino (siempre en RGBA32 para manejar la posible transparencia)
        Texture2D processedTexture = new Texture2D(
            originalTexture.width,
            originalTexture.height,
            TextureFormat.RGBA32,
            false
        );

        Color[] pixels = originalTexture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];

            float finalR = pixel.r;
            float finalG = pixel.g;
            float finalB = pixel.b;
            float finalA = pixel.a; // Mantiene la transparencia original por defecto

            // --- L�gica de Escala de Grises (si es necesaria) ---
            if (needsGrayscale)
            {
                // F�rmula de luminosidad para escala de grises
                float grayValue = pixel.r * 0.2126f + pixel.g * 0.7152f + pixel.b * 0.0722f;
                finalR = grayValue;
                finalG = grayValue;
                finalB = grayValue;

                // Si tambi�n necesita transparencia, la creamos a partir del gris (s�mbolo negro)
                if (needsTransparency)
                {
                    // Blanco (1.0) -> Transparente (0.0). Negro (0.0) -> Opaco (1.0)
                    finalA = 1.0f - grayValue;
                }
            }

            // --- L�gica de Transparencia (si solo es necesaria la transparencia) ---
            else if (needsTransparency)
            {
                // Mantiene el color original (finalR, finalG, finalB ya tienen los valores originales)

                // Creamos la transparencia a partir del color original (si no es B/N)
                // Usaremos la luminosidad (grayValue) para definir qu� es transparente, 
                // incluso si el color se mantiene.
                float grayValue = pixel.r * 0.2126f + pixel.g * 0.7152f + pixel.b * 0.0722f;
                finalA = 1.0f - grayValue;
            }

            // Aplica el p�xel modificado o sin modificar
            processedTexture.SetPixel(i % originalTexture.width, i / originalTexture.width,
                                     new Color(finalR, finalG, finalB, finalA));
        }

        processedTexture.Apply();
        return processedTexture;
    }
}
