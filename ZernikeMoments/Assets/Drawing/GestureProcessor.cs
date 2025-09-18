using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class GestureProcessor
{

    public static List<Vector2> Normalize(List<Vector2> points)
    {
      //  var resampled = Resample(points, 64);
        //var reorganized = Reorganize(resampled);
        //var rotated = RotateToZero(reorganized);
        var scaled = ScaleToSquare(points, 9f);
        var translated = TranslateToOrigin(scaled);
        var smoothed = Simplify(translated,0.1f);
        // var reorganized = Reorganize(translated);
        return translated;
    }

    public static List<Vector2> Simplify(List<Vector2> points, float epsilon, float angleThreshold = 170f)
    {
        if (points == null || points.Count < 3)
        {
            return new List<Vector2>(points);
        }

        // Find indices of sharp points
        List<int> sharpIndices = new List<int> { 0 }; // Start point
        float sharpTurnThreshold = 180f - angleThreshold;

        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 incoming = points[i] - points[i - 1];
            Vector2 outgoing = points[i + 1] - points[i];

            if (incoming.sqrMagnitude == 0f || outgoing.sqrMagnitude == 0f)
            {
                continue; // Skip if duplicate points
            }

            float turnAngle = Vector2.Angle(incoming, outgoing);

            if (turnAngle > sharpTurnThreshold)
            {
                sharpIndices.Add(i);
            }
        }
        sharpIndices.Add(points.Count - 1); // End point

        // Simplify each segment between sharp points
        List<Vector2> simplified = new List<Vector2>();

        for (int s = 0; s < sharpIndices.Count - 1; s++)
        {
            int startIdx = sharpIndices[s];
            int endIdx = sharpIndices[s + 1];
            List<Vector2> segment = points.GetRange(startIdx, endIdx - startIdx + 1);
            List<Vector2> simplifiedSegment = RamerDouglasPeucker(segment, epsilon);

            // Add the simplified segment, avoiding duplicating shared points
            if (s == 0)
            {
                simplified.AddRange(simplifiedSegment);
            }
            else
            {
                simplified.AddRange(simplifiedSegment.GetRange(1, simplifiedSegment.Count - 1));
            }
        }

        return simplified;
    }

    private static List<Vector2> RamerDouglasPeucker(List<Vector2> points, float epsilon)
    {
        if (points.Count < 3)
        {
            return new List<Vector2>(points);
        }

        // Find the point with the maximum distance from the line between start and end
        float maxDistance = 0f;
        int maxIndex = 0;
        Vector2 start = points[0];
        Vector2 end = points[points.Count - 1];

        for (int i = 1; i < points.Count - 1; i++)
        {
            float distance = PerpendicularDistance(points[i], start, end);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                maxIndex = i;
            }
        }

        // If max distance is greater than epsilon, recursively simplify
        if (maxDistance > epsilon)
        {
            List<Vector2> left = RamerDouglasPeucker(points.GetRange(0, maxIndex + 1), epsilon);
            List<Vector2> right = RamerDouglasPeucker(points.GetRange(maxIndex, points.Count - maxIndex), epsilon);

            left.RemoveAt(left.Count - 1); // Remove duplicate
            left.AddRange(right);
            return left;
        }
        else
        {
            return new List<Vector2> { start, end };
        }
    }

    private static float PerpendicularDistance(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 direction = lineEnd - lineStart;
        float lengthSquared = direction.sqrMagnitude;
        if (lengthSquared == 0f)
        {
            return Vector2.Distance(point, lineStart);
        }

        float t = Vector2.Dot(point - lineStart, direction) / lengthSquared;
        Vector2 projection = lineStart + t * direction;
        return Vector2.Distance(point, projection);
    }
    public static List<Vector2> GenerateSmoothedPath(List<Vector2> points, int subdivisions)
    {
        if (points.Count < 2)
        {
            return points;
        }

        List<Vector2> smoothedPoints = new List<Vector2>();

        // Crear una copia de los puntos para añadir los puntos de control en los extremos
        List<Vector2> tempPoints = new List<Vector2>(points);
        tempPoints.Insert(0, points[0]);
        tempPoints.Add(points[points.Count - 1]);

        // Generar la curva entre cada par de puntos originales
        for (int i = 1; i < tempPoints.Count - 2; i++)
        {
            Vector2 p0 = tempPoints[i - 1];
            Vector2 p1 = tempPoints[i];
            Vector2 p2 = tempPoints[i + 1];
            Vector2 p3 = tempPoints[i + 2];

            for (int t = 0; t <= subdivisions; t++)
            {
                float normalizedT = (float)t / subdivisions;
                Vector2 interpolatedPoint = GetSplinePoint(normalizedT, p0, p1, p2, p3);
                smoothedPoints.Add(interpolatedPoint);
            }
        }

        return smoothedPoints;
    }

    // Calcula un punto en el spline cúbico usando la fórmula de Catmull-Rom.
    private static Vector2 GetSplinePoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        // Esta es la fórmula de Catmull-Rom (un tipo de spline cúbico)
        float t2 = t * t;
        float t3 = t2 * t;

        float a = 0.5f * (2.0f * p1.x + (-p0.x + p2.x) * t + (2.0f * p0.x - 5.0f * p1.x + 4.0f * p2.x - p3.x) * t2 + (-p0.x + 3.0f * p1.x - 3.0f * p2.x + p3.x) * t3);
        float b = 0.5f * (2.0f * p1.y + (-p0.y + p2.y) * t + (2.0f * p0.y - 5.0f * p1.y + 4.0f * p2.y - p3.y) * t2 + (-p0.y + 3.0f * p1.y - 3.0f * p2.y + p3.y) * t3);

        return new Vector2(a, b);
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