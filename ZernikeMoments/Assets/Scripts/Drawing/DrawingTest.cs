using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections;
using UnityEngine.EventSystems;
using System;
public class DrawingTest : MonoBehaviour
{
    public List<LineRenderer> lineRenderers;
    public List<LineRenderer> renderTexturelineRenderers;
    [SerializeField] LineRenderer currentLR;
    [SerializeField] LineRenderer lRPrefab;
    [SerializeField] LineRenderer lRPrefabRT;
    public int linerendererIndex;
    public LineRenderer recognitionLineRenderer;
    public TMP_Text resultText;
    public Camera cam;
    //public GestureRecognizer recognizer;
    public BaseButton_Recognize recognizebutton;

    private List<Vector2> currentPoints = new();
    private List<Vector2> currentStrokePoints = new();
    private bool isDrawing = false;
    List<Vector2> greenPoints = new List<Vector2>();
    List<Vector2> redPoints = new List<Vector2>();
    List<List<Vector2>> listaDeListas = new List<List<Vector2>>();
    public ZernikeManager zRecognizer;
    public List<int> strokesPointsCount = new List<int>();
    public TMP_InputField symbolNameField;

    [SerializeField] RenderTexture renderTexture;

    
    void Start()
    {
      //  recognizebutton.buttonAction += OnConfirmDrawing;        
    }

    void Update()
    {
        // Si se está presionando el botón izquierdo del mouse
        if (Input.GetMouseButtonDown(0))
        {
            // Ignorar si está sobre UI
            if (IsPointerOverUI())
                return;

            isDrawing = true;
            currentStrokePoints.Clear();
        }

        // Mientras el botón izquierdo esté presionado
        if (Input.GetMouseButton(0))
        {
            if (IsPointerOverUI())
                return;

            if (isDrawing)
            {
                Vector2 pos = cam.ScreenToWorldPoint(Input.mousePosition);

                if (currentPoints.Count == 0 || Vector2.Distance(currentPoints[^1], pos) > 0.001f)
                {
                    AddSmoothedPoint(pos);
                }
            }
        }

        // Cuando se suelta el botón izquierdo
        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;

            if (GetLineLength(currentLR) < 1.5f)
            {
                foreach (var item in currentStrokePoints)
                {
                    if (currentPoints.Contains(item))
                        currentPoints.Remove(item);
                }

                currentStrokePoints.Clear();
                currentLR.positionCount = 0;
                return;
            }

            listaDeListas.Add(new List<Vector2>());
            linerendererIndex++;
            currentLR = Instantiate(lRPrefab, this.transform);
            lineRenderers.Add(currentLR);
            strokesPointsCount.Add(currentStrokePoints.Count);
            currentStrokePoints.Clear();
        }
    }

    // --- NUEVO ---
    // Equivalente a IsTouchOverUI pero para mouse
    bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null &&
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    bool IsTouchOverUI(Touch touch)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = touch.position;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }
    //void Update()
    //{


    //    if (Input.touchCount > 0)
    //    {
    //        Touch touch = Input.GetTouch(0);

    //        //  ignorar si el toque está sobre UI
    //        if (IsTouchOverUI(touch))
    //        {
    //            //Debug.Log("TOCOBOTON");
    //            return;
    //        }


    //        Vector2 pos = cam.ScreenToWorldPoint(touch.position);

    //        switch (touch.phase)
    //        {
    //            case TouchPhase.Began:
    //                isDrawing = true;
    //                currentStrokePoints.Clear();
    //                break;

    //            case TouchPhase.Moved:
    //            case TouchPhase.Stationary:
    //                if (isDrawing)
    //                {
    //                    if (currentPoints.Count == 0 || Vector2.Distance(currentPoints[^1], pos) > 0.001f)
    //                    {
    //                        AddSmoothedPoint(pos);

    //                    }
    //                }
    //                break;

    //            case TouchPhase.Ended:
    //            case TouchPhase.Canceled:
    //                isDrawing = false;

    //                if (GetLineLength(currentLR) < 1.5f)
    //                {
    //                    // listaDeListas[linerendererIndex] = GestureProcessor.Normalize(listaDeListas[linerendererIndex]);
    //                    foreach (var item in currentStrokePoints)
    //                    {
    //                        if (currentPoints.Contains(item))
    //                        {
    //                            currentPoints.Remove(item);
    //                        }
    //                    }
    //                    currentStrokePoints.Clear();



    //                    currentLR.positionCount = 0;
    //                    return;
    //                }
    //                listaDeListas.Add(new List<Vector2>());
    //                linerendererIndex++;
    //                currentLR = Instantiate(lRPrefab, this.transform);
    //                lineRenderers.Add(currentLR);
    //                // listaDeListas[linerendererIndex] = GestureProcessor.Normalize(listaDeListas[linerendererIndex]);
    //                strokesPointsCount.Add(currentStrokePoints.Count);
    //                currentStrokePoints.Clear();
    //                break;
    //        }
    //    }
    //}


    void AddSmoothedPoint(Vector3 newPoint)
    {
        currentStrokePoints.Add(newPoint);
        currentPoints.Add(newPoint);
        // listaDeListas[linerendererIndex].Add(newPoint);
        int count = currentStrokePoints.Count;

        if (count == 1)
        {
            // primer punto del trazo inicializamos el LineRenderer

            lineRenderers[linerendererIndex].positionCount = 1;
            lineRenderers[linerendererIndex].SetPosition(0, newPoint);
            return;
        }

        if (count < 4)
        {
            // si todavía no hay suficientes puntos para interpolar, solo agrego directo
            lineRenderers[linerendererIndex].positionCount++;
            lineRenderers[linerendererIndex].SetPosition(lineRenderers[linerendererIndex].positionCount - 1, newPoint);
            return;
        }

        // últimos 4 puntos para Catmull-Rom
        Vector3 p0 = currentStrokePoints[count - 4];
        Vector3 p1 = currentStrokePoints[count - 3];
        Vector3 p2 = currentStrokePoints[count - 2];
        Vector3 p3 = currentStrokePoints[count - 1];

        int subdivisions = 4; // más subdivisiones = más suave
        for (int j = 1; j <= subdivisions; j++) // arranco en 1 para no repetir p1
        {
            float t = j / (float)subdivisions;
            Vector3 point = 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
            );

            lineRenderers[linerendererIndex].positionCount++;
            lineRenderers[linerendererIndex].SetPosition(lineRenderers[linerendererIndex].positionCount - 1, point);

        }


    }

    public void SaveSymbol()
    {
        StartCoroutine(SaveSymbolCoroutine());
    }

    public ReferenceSymbol ReturnNewSymbol(string symbolName,bool shouldRender)
    {
        if (currentPoints.Any())
        {
            var normalizedPositions = GestureProcessor.Normalize(currentPoints);
            for (int i = 0; i < strokesPointsCount.Count; i++)
            {
                if (shouldRender)
                {
                    renderTexturelineRenderers.Add(Instantiate(lRPrefabRT, transform));
                    renderTexturelineRenderers[i].positionCount = strokesPointsCount[i];
                }

                if (strokesPointsCount[i] == 0) continue;
                for (int j = 0; j < strokesPointsCount[i]; j++)
                {
                    listaDeListas[i].Add(normalizedPositions[j]);
                    if (shouldRender)
                    {
                        renderTexturelineRenderers[i].SetPosition(j, normalizedPositions[j]);
                    }
                  
                }
                normalizedPositions = normalizedPositions.Skip(strokesPointsCount[i]).ToList();
            }



            return zRecognizer.ReturnNewSymbol(listaDeListas, lineRenderers.Count - 1, symbolName, "symbolID");
        }
        else
        {
            return null;
        }
    }
    float GetLineLength(LineRenderer line)
    {
        float length = 0f;
        for (int i = 1; i < line.positionCount; i++)
        {
            length += Vector3.Distance(line.GetPosition(i - 1), line.GetPosition(i));
        }
        return length;
    }
    public void OnConfirmDrawing()
    {


        if (currentPoints.Any()) 
        {
            var normalizedPositions = GestureProcessor.Normalize(currentPoints);
            for (int i = 0; i < strokesPointsCount.Count; i++)
            {
                if (strokesPointsCount[i] == 0) continue;
                for (int j = 0; j < strokesPointsCount[i]; j++)
                {
                    listaDeListas[i].Add(normalizedPositions[j]);
                    
                }
                normalizedPositions = normalizedPositions.Skip(strokesPointsCount[i]).ToList();
            }
    
         
            zRecognizer.OnDrawingFinished(listaDeListas, lineRenderers.Count -1);
            currentPoints.Clear();
            strokesPointsCount.Clear();
            foreach (var item in listaDeListas)
            {
                item.Clear();
            }
            linerendererIndex = 0;
            lineRenderers[0].positionCount = 0;
            var count = lineRenderers.Count;
            for (int i = 1; i < count; i++)
            {
                Destroy(lineRenderers[1].gameObject);
                lineRenderers.RemoveAt(1);
            }

            currentLR = lineRenderers[0];
        }
        else
        {
            resultText.text = "Trazo demasiado corto";
            currentPoints.Clear();
            strokesPointsCount.Clear();
            foreach (var item in listaDeListas)
            {
                item.Clear();
            }
            linerendererIndex = 0;
            lineRenderers[0].positionCount = 0;
            foreach (var item in lineRenderers.Skip(1))
            {
                lineRenderers.Remove(item);
                Destroy(item.gameObject);
            }
            currentLR = lineRenderers[0];
        }
    }
    private class PointData
    {
        public Vector2 point;
        public float area;
    }

    // Método principal para simplificar el trazo
    public static List<Vector2> Simplify(List<Vector2> points, float tolerance)
    {
        if (points.Count < 3)
        {
            return points;
        }

        List<PointData> pointData = new List<PointData>();
        for (int i = 0; i < points.Count; i++)
        {
            pointData.Add(new PointData { point = points[i] });
        }

        // Calcular el área de cada punto
        for (int i = 1; i < pointData.Count - 1; i++)
        {
            pointData[i].area = CalculateTriangleArea(pointData[i - 1].point, pointData[i].point, pointData[i + 1].point);
        }

        // Puntos de inicio y fin siempre se mantienen
        pointData[0].area = float.MaxValue;
        pointData[pointData.Count - 1].area = float.MaxValue;

        // Bucle principal para eliminar puntos
        while (true)
        {
            int minIndex = -1;
            float minArea = float.MaxValue;

            for (int i = 1; i < pointData.Count - 1; i++)
            {
                if (pointData[i].area < minArea)
                {
                    minArea = pointData[i].area;
                    minIndex = i;
                }
            }

            if (minIndex == -1 || minArea > tolerance)
            {
                break; // Se detiene cuando el área más pequeña es mayor que la tolerancia
            }

            // Eliminar el punto con la menor área
            pointData.RemoveAt(minIndex);

            // Recalcular el área de los vecinos del punto eliminado
            if (minIndex > 0 && minIndex < pointData.Count - 1)
            {
                pointData[minIndex].area = CalculateTriangleArea(pointData[minIndex - 1].point, pointData[minIndex].point, pointData[minIndex + 1].point);
                if (minIndex > 1)
                {
                    pointData[minIndex - 1].area = CalculateTriangleArea(pointData[minIndex - 2].point, pointData[minIndex - 1].point, pointData[minIndex].point);
                }
            }
        }

        // Convertir los datos a la lista de puntos final
        List<Vector2> simplifiedPoints = new List<Vector2>();
        foreach (var data in pointData)
        {
            simplifiedPoints.Add(data.point);
        }

        return simplifiedPoints;
    }

    // Método para calcular el área de un triángulo usando las coordenadas
    private static float CalculateTriangleArea(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return Mathf.Abs(p1.x * (p2.y - p3.y) + p2.x * (p3.y - p1.y) + p3.x * (p1.y - p2.y)) * 0.5f;
    }
    void OnDrawGizmos()
    {
        if (greenPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (var item in greenPoints)
            {
                Gizmos.DrawSphere(new Vector3(item.x, item.y, 0), 0.1f);
            }
        }

        if (redPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (var item in redPoints)
            {
                Gizmos.DrawSphere(new Vector3(item.x, item.y, 0), 0.1f);
            }
        }
    }

    IEnumerator SaveSymbolCoroutine()
    {
        if (currentPoints.Any())
        {
            var normalizedPositions = GestureProcessor.Normalize(currentPoints);
            for (int i = 0; i < strokesPointsCount.Count; i++)
            {
                renderTexturelineRenderers.Add(Instantiate(lRPrefabRT, transform));
                renderTexturelineRenderers[i].positionCount = strokesPointsCount[i];
                if (strokesPointsCount[i] == 0) continue;
                for (int j = 0; j < strokesPointsCount[i]; j++)
                {
                    listaDeListas[i].Add(normalizedPositions[j]);
                    renderTexturelineRenderers[i].SetPosition(j, normalizedPositions[j]);
                }
                normalizedPositions = normalizedPositions.Skip(strokesPointsCount[i]).ToList();
            }


            yield return new WaitForSeconds(.1f);
            string symbolID = Guid.NewGuid().ToString();
            ImageUtils.SaveRenderTextureToPNG(renderTexture, symbolID);
            zRecognizer.SaveSymbol(listaDeListas, lineRenderers.Count - 1, symbolNameField.text, symbolID);
            symbolNameField.text = "";
            foreach (var item in renderTexturelineRenderers)
            {
                Destroy(item.gameObject);
            }
            renderTexturelineRenderers.Clear();
            currentPoints.Clear();
            strokesPointsCount.Clear();
            foreach (var item in listaDeListas)
            {
                item.Clear();
            }
            linerendererIndex = 0;
            lineRenderers[0].positionCount = 0;
            foreach (var item in lineRenderers.Skip(1))
            {
                lineRenderers.Remove(item);
                Destroy(item.gameObject);
            }
            currentLR = lineRenderers[0];
        }
        else
        {
            resultText.text = "Trazo demasiado corto";
            currentPoints.Clear();
            strokesPointsCount.Clear();
            foreach (var item in listaDeListas)
            {
                item.Clear();
            }
            linerendererIndex = 0;
            lineRenderers[0].positionCount = 0;
            foreach (var item in lineRenderers.Skip(1))
            {
                lineRenderers.Remove(item);
                Destroy(item.gameObject);
            }
            currentLR = lineRenderers[0];
        }
    }

    public void ClearAllLineRenderers(bool clearRenderDraw)
    {
        currentPoints.Clear();
        strokesPointsCount.Clear();
        foreach (var item in listaDeListas)
        {
            item.Clear();
        }
        linerendererIndex = 0;
        lineRenderers[0].positionCount = 0;
        var count = lineRenderers.Count;
        for (int i = 1; i < count; i++)
        {
            Destroy(lineRenderers[1].gameObject);
            lineRenderers.RemoveAt(1);
        }

        currentLR = lineRenderers[0];
        if (clearRenderDraw)
        {
            foreach (var item in renderTexturelineRenderers)
            {
                Destroy(item.gameObject);
            }
            renderTexturelineRenderers.Clear();
        }


    }
    //IEnumerator Drawpoints()
    //{
    //    var normalizedPositions = GestureProcessor.Normalize(currentPoints);
    //    recognitionLineRenderer.positionCount = normalizedPositions.Count;
    //    for (int i = 0; i < normalizedPositions.Count; i++)
    //    {
    //        recognitionLineRenderer.SetPosition(i, normalizedPositions[i]);
    //    }
    //    yield return new WaitForSeconds(.1f);
    //    zRecognizer.OnDrawingFinished(normalizedPositions);
    //}
}
