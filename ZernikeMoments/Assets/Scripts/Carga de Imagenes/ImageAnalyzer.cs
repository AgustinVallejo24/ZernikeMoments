using UnityEngine;

public static class ImageAnalyzer
{
    // Detecta si la textura contiene píxeles con un valor alfa (transparencia) inferior a 1.0f
    public static bool IsTransparent(Texture2D texture)
    {
        // Optimizamos obteniendo todos los píxeles a la vez
        Color[] pixels = texture.GetPixels();

        // Comprobamos si algún píxel tiene transparencia activa
        foreach (Color pixel in pixels)
        {
            // Usamos un pequeño umbral (epsilon) en lugar de == 1.0f para mayor seguridad
            if (pixel.a < 0.99f)
            {
                return true;
            }
        }
        return false;
    }

    // Detecta si la textura ya está en escala de grises
    public static bool IsGrayscale(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();

        foreach (Color pixel in pixels)
        {
            // En escala de grises, los componentes R, G y B deben ser prácticamente iguales.
            // Usamos una tolerancia (0.01f) para los posibles errores de compresión/flotantes.
            if (Mathf.Abs(pixel.r - pixel.g) > 0.01f || Mathf.Abs(pixel.r - pixel.b) > 0.01f)
            {
                return false;
            }
        }
        return true;
    }
}
