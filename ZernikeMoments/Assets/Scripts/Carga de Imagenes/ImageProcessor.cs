using UnityEngine;

public static class ImageProcessor
{
    // Returns the given texture converted to gray scale, and with an artificial transparency, if the texture doesn't fulffil those conditions.
    public static Texture2D ProcessTextureConditional(Texture2D originalTexture)
    {
        bool needsTransparency = !IsTransparent(originalTexture);
        bool needsGrayscale = !IsGrayscale(originalTexture);
        
        if (!needsTransparency && !needsGrayscale)
        {
            Debug.Log("The texture fulffil both conditions");
            return originalTexture;
        }
        
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
            float finalA = pixel.a; 
            
            if (needsGrayscale)
            {                
                float grayValue = pixel.r * 0.2126f + pixel.g * 0.7152f + pixel.b * 0.0722f;
                finalR = grayValue;
                finalG = grayValue;
                finalB = grayValue;
                
                if (needsTransparency)
                {                    
                    finalA = 1.0f - grayValue;
                }
            }            
            else if (needsTransparency)
            {                
                float grayValue = pixel.r * 0.2126f + pixel.g * 0.7152f + pixel.b * 0.0722f;
                finalA = 1.0f - grayValue;
            }
            
            processedTexture.SetPixel(i % originalTexture.width, i / originalTexture.width,
                                     new Color(finalR, finalG, finalB, finalA));
        }

        processedTexture.Apply();
        return processedTexture;
    }
    
    // Returns wether the texture has transparency or not.
    public static bool IsTransparent(Texture2D texture)
    {        
        Color[] pixels = texture.GetPixels();
        
        foreach (Color pixel in pixels)
        {            
            if (pixel.a < 0.99f)
            {
                return true;
            }
        }
        return false;
    }
    
    // Returns wether the texture is in gray scale.
    public static bool IsGrayscale(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();

        foreach (Color pixel in pixels)
        {            
            if (Mathf.Abs(pixel.r - pixel.g) > 0.01f || Mathf.Abs(pixel.r - pixel.b) > 0.01f)
            {
                return false;
            }
        }
        return true;
    }
}
