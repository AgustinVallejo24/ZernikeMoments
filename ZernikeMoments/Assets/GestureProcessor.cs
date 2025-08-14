using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class GestureProcessor
{

    public static List<Vector2> Normalize(List<Vector2> points)
    {
        var resampled = Resample(points, 64);
        //var reorganized = Reorganize(resampled);
        //var rotated = RotateToZero(reorganized);
        var scaled = ScaleToSquare(resampled, 6f);
        var translated = TranslateToOrigin(scaled);
        // var reorganized = Reorganize(translated);
        return translated;
    }

    public static List<Vector2> Reorganize(List<Vector2> points)
    {
        var pointss = points.OrderBy(x => x.x).ThenByDescending(x => x.y).ToList();
        //foreach (var item in pointss)
        //{
        //    Debug.Log(item);
        //}
        return pointss;
    }
    public static List<Vector2> Resample(List<Vector2> points, int n)
    {
        float I = PathLength(points) / (n - 1);
        float D = 0f;
        List<Vector2> newPoints = new() { points[0] };

        for (int i = 1; i < points.Count; i++)
        {
            float d = Vector2.Distance(points[i - 1], points[i]);
            if ((D + d) >= I)
            {
                float t = (I - D) / d;
                Vector2 newPoint = Vector2.Lerp(points[i - 1], points[i], t);
                newPoints.Add(newPoint);
                points.Insert(i, newPoint);
                D = 0f;
            }
            else
            {
                D += d;
            }
        }

        if (newPoints.Count < n)
            newPoints.Add(points[^1]);

        return newPoints;
    }

    public static float PathLength(List<Vector2> points)
    {
        float length = 0f;
        for (int i = 1; i < points.Count; i++)
            length += Vector2.Distance(points[i - 1], points[i]);
        return length;
    }

    public static List<Vector2> RotateToZero(List<Vector2> points)
    {
        Vector2 centroid = Centroid(points);
        float angle = Mathf.Atan2(points[0].y - centroid.y, points[0].x - centroid.x);
        return RotateBy(points, -angle);
    }

    public static List<Vector2> RotateBy(List<Vector2> points, float angle)
    {
        Vector2 centroid = Centroid(points);
        List<Vector2> newPoints = new();
        foreach (var p in points)
        {
            float dx = p.x - centroid.x;
            float dy = p.y - centroid.y;
            float x = dx * Mathf.Cos(angle) - dy * Mathf.Sin(angle) + centroid.x;
            float y = dx * Mathf.Sin(angle) + dy * Mathf.Cos(angle) + centroid.y;
            newPoints.Add(new Vector2(x, y));
        }
        return newPoints;
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

    //public static List<Vector2> Preprocess(List<Vector2> points, int targetCount = 32)
    //{
    //    var resampled = Resample(points, targetCount);
    //    var rotated = RotateToZero(resampled);
    //    var scaled = ScaleToSquare(rotated, 1f);
    //    var translated = TranslateToOrigin(scaled);
    //    return translated;
    //}

    //public static List<Vector2> Resample(List<Vector2> points, int n)
    //{
    //    float totalLength = PathLength(points);
    //    float interval = totalLength / (n - 1);
    //    float distanceAccum = 0;

    //    List<Vector2> resampled = new List<Vector2> { points[0] };

    //    for (int i = 1; i < points.Count; i++)
    //    {
    //        float d = Vector2.Distance(points[i - 1], points[i]);
    //        if ((distanceAccum + d) >= interval)
    //        {
    //            float t = (interval - distanceAccum) / d;
    //            Vector2 newPoint = Vector2.Lerp(points[i - 1], points[i], t);
    //            resampled.Add(newPoint);
    //            points.Insert(i, newPoint);
    //            distanceAccum = 0;
    //        }
    //        else
    //        {
    //            distanceAccum += d;
    //        }
    //    }

    //    if (resampled.Count < n)
    //        resampled.Add(points[points.Count - 1]);

    //    return resampled;
    //}

    //public static float PathLength(List<Vector2> points)
    //{
    //    float length = 0;
    //    for (int i = 1; i < points.Count; i++)
    //        length += Vector2.Distance(points[i - 1], points[i]);
    //    return length;
    //}

    //public static List<Vector2> RotateToZero(List<Vector2> points)
    //{
    //    Vector2 centroid = GetCentroid(points);
    //    float angle = Mathf.Atan2(points[0].y - centroid.y, points[0].x - centroid.x);
    //    return RotateBy(points, -angle);
    //}

    //public static List<Vector2> RotateBy(List<Vector2> points, float angle)
    //{
    //    Vector2 centroid = GetCentroid(points);
    //    List<Vector2> result = new List<Vector2>();

    //    float cos = Mathf.Cos(angle);
    //    float sin = Mathf.Sin(angle);

    //    foreach (var p in points)
    //    {
    //        float dx = p.x - centroid.x;
    //        float dy = p.y - centroid.y;

    //        float x = dx * cos - dy * sin + centroid.x;
    //        float y = dx * sin + dy * cos + centroid.y;
    //        result.Add(new Vector2(x, y));
    //    }

    //    return result;
    //}

    //public static List<Vector2> ScaleToSquare(List<Vector2> points, float size)
    //{
    //    Rect bounds = GetBoundingBox(points);
    //    List<Vector2> scaled = new List<Vector2>();

    //    foreach (var p in points)
    //    {
    //        float x = (p.x - bounds.x) / bounds.width * size;
    //        float y = (p.y - bounds.y) / bounds.height * size;
    //        scaled.Add(new Vector2(x, y));
    //    }

    //    return scaled;
    //}

    //public static List<Vector2> TranslateToOrigin(List<Vector2> points)
    //{
    //    Vector2 centroid = GetCentroid(points);
    //    List<Vector2> translated = new List<Vector2>();

    //    foreach (var p in points)
    //        translated.Add(p - centroid);

    //    return translated;
    //}

    //public static Rect GetBoundingBox(List<Vector2> points)
    //{
    //    float minX = float.MaxValue, minY = float.MaxValue;
    //    float maxX = float.MinValue, maxY = float.MinValue;

    //    foreach (var p in points)
    //    {
    //        minX = Mathf.Min(minX, p.x);
    //        minY = Mathf.Min(minY, p.y);
    //        maxX = Mathf.Max(maxX, p.x);
    //        maxY = Mathf.Max(maxY, p.y);
    //    }

    //    return new Rect(minX, minY, maxX - minX, maxY - minY);
    //}

    //public static Vector2 GetCentroid(List<Vector2> points)
    //{
    //    Vector2 sum = Vector2.zero;
    //    foreach (var p in points)
    //        sum += p;
    //    return sum / points.Count;
    //}
}