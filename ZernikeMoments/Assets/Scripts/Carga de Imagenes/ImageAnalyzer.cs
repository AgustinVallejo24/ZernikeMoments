using UnityEngine;

public static class ImageAnalyzer
{
    // Detecta si la textura contiene p�xeles con un valor alfa (transparencia) inferior a 1.0f
    public static bool IsTransparent(Texture2D texture)
    {
        // Optimizamos obteniendo todos los p�xeles a la vez
        Color[] pixels = texture.GetPixels();

        // Comprobamos si alg�n p�xel tiene transparencia activa
        foreach (Color pixel in pixels)
        {
            // Usamos un peque�o umbral (epsilon) en lugar de == 1.0f para mayor seguridad
            if (pixel.a < 0.99f)
            {
                return true;
            }
        }
        return false;
    }

    // Detecta si la textura ya est� en escala de grises
    public static bool IsGrayscale(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();

        foreach (Color pixel in pixels)
        {
            // En escala de grises, los componentes R, G y B deben ser pr�cticamente iguales.
            // Usamos una tolerancia (0.01f) para los posibles errores de compresi�n/flotantes.
            if (Mathf.Abs(pixel.r - pixel.g) > 0.01f || Mathf.Abs(pixel.r - pixel.b) > 0.01f)
            {
                return false;
            }
        }
        return true;
    }
}
