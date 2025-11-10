using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class DrawNormalizer
{

    public static List<Vector2> Normalize(List<Vector2> points)
    {

        var scaled = ScaleToSquare(points, 6f);
        var translated = TranslateToOrigin(scaled);
        return translated;
    }

   
    
  
    public static List<Vector2> ScaleToSquare(List<Vector2> points, float size)
    {
        Rect box = BoundingBox(points);
        float scale = size / Mathf.Max(box.width, box.height);
        List<Vector2> newPoints = new();

        foreach (var p in points)
        {
            float x = (p.x - box.x) * scale;
            float y = (p.y - box.y) * scale;
            newPoints.Add(new Vector2(x, y));
        }

        return newPoints;
    }

    public static List<Vector2> TranslateToOrigin(List<Vector2> points)
    {
        Vector2 centroid = Centroid(points);
        List<Vector2> newPoints = new();
        foreach (var p in points)
            newPoints.Add(p - centroid);
        return newPoints;
    }

    public static Vector2 Centroid(List<Vector2> points)
    {
        Vector2 sum = Vector2.zero;
        foreach (var p in points)
            sum += p;
        return sum / points.Count;
    }

    public static Rect BoundingBox(List<Vector2> points)
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var p in points)
        {
            if (p.x < minX) minX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.x > maxX) maxX = p.x;
            if (p.y > maxY) maxY = p.y;
        }

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

   
}