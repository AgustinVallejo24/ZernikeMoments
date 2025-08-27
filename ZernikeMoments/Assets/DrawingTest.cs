using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections;
using UnityEngine.EventSystems;
public class DrawingTest : MonoBehaviour
{
    public LineRenderer[] lineRenderer;
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

    public ZernikeManager zRecognizer;


    void Start()
    {


    }

    void Update()
    {
        //if (recognizebutton.isPressed)
        //{
        //    OnConfirmDrawing();
        //}
        if (Input.GetMouseButtonDown(0) && linerendererIndex < lineRenderer.Length)
        {
            isDrawing = true;
            //lineRenderer[linerendererIndex].SetPosition(0, cam.ScreenToWorldPoint(Input.mousePosition));

        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;

            if (linerendererIndex < lineRenderer.Length)
            {
                linerendererIndex++;
            }

            currentStrokePoints.Clear();


            //   var result = recognizer.RecognizeCurrentDrawing();
            //Debug.Log("Resultado reconocimiento: " + result);
        }

        if (isDrawing)
        {
            Vector2 pos = cam.ScreenToWorldPoint(Input.mousePosition);
            if (currentPoints.Count == 0 || Vector2.Distance(currentPoints[^1], pos) > 0.1f)
            {
                currentPoints.Add(pos);
                currentStrokePoints.Add(pos);
                lineRenderer[linerendererIndex].positionCount = currentStrokePoints.Count;
                lineRenderer[linerendererIndex].SetPosition(currentStrokePoints.Count - 1, pos);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && currentPoints.Any())
        {
            var normalizedPositions = GestureProcessor.Normalize(currentPoints);
            zRecognizer.OnDrawingFinished(normalizedPositions, linerendererIndex);
            currentPoints.Clear();
            linerendererIndex = 0;
            foreach (var item in lineRenderer)
            {
                item.positionCount = 0;
            }

        }

    }

    bool IsTouchOverUI(Touch touch)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = touch.position;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }
//    void Update()
//{

//        if (recognizebutton.isPressed)
//        {
//            OnConfirmDrawing();
//        }


//    if (Input.touchCount > 0)
//    {
//        Touch touch = Input.GetTouch(0);

//        //  ignorar si el toque está sobre UI
//        if (IsTouchOverUI(touch))
//            {
//                //Debug.Log("TOCOBOTON");
//                return;
//            }
            

//        Vector2 pos = cam.ScreenToWorldPoint(touch.position);

//        switch (touch.phase)
//        {
//            case TouchPhase.Began:
//                if (linerendererIndex < lineRenderer.Length)
//                {
//                    isDrawing = true;
//                    currentStrokePoints.Clear();
//                }
//                break;

//            case TouchPhase.Moved:
//            case TouchPhase.Stationary:
//                if (isDrawing)
//                {
//                    if (currentPoints.Count == 0 || Vector2.Distance(currentPoints[^1], pos) > 0.001f)
//                    {
//                            AddSmoothedPoint(pos);
                   
//                    }
//                }
//                break;

//            case TouchPhase.Ended:
//            case TouchPhase.Canceled:
//                isDrawing = false;

//                if (linerendererIndex < lineRenderer.Length)
//                {
//                    linerendererIndex++;
//                }

//                currentStrokePoints.Clear();
//                break;
//        }
//    }
//}
    void AddSmoothedPoint(Vector3 newPoint)
    {
        currentStrokePoints.Add(newPoint);
        currentPoints.Add(newPoint);
        int count = currentStrokePoints.Count;

        if (count == 1)
        {
            // primer punto del trazo inicializamos el LineRenderer
            lineRenderer[linerendererIndex].positionCount = 1;
            lineRenderer[linerendererIndex].SetPosition(0, newPoint);
            return;
        }

        if (count < 4)
        {
            // si todavía no hay suficientes puntos para interpolar, solo agrego directo
            lineRenderer[linerendererIndex].positionCount++;
            lineRenderer[linerendererIndex].SetPosition(lineRenderer[linerendererIndex].positionCount - 1, newPoint);
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

            lineRenderer[linerendererIndex].positionCount++;
            lineRenderer[linerendererIndex].SetPosition(lineRenderer[linerendererIndex].positionCount - 1, point);
            
        }

        
    }
    public void OnConfirmDrawing()
    {
        if (currentPoints.Any())
        {
            var normalizedPositions = GestureProcessor.Normalize(currentPoints);
            zRecognizer.OnDrawingFinished(normalizedPositions, linerendererIndex);
            currentPoints.Clear();
            linerendererIndex = 0;
            foreach (var item in lineRenderer)
            {
                item.positionCount = 0;
            }
        }
    }
    List<Vector2> CircleTemplate()
    {
        List<Vector2> points = new();
        int segments = 64;
        float radius = 1f;

        for (int i = 0; i < segments; i++)
        {
            float theta = 2f * Mathf.PI * i / segments;
            points.Add(new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * radius);
        }

        return points;
    }
    List<Vector2> CircleTemplateR()
    {
        List<Vector2> points = new();
        int segments = 64;
        float radius = 1f;

        for (int i = 64; i > 0; i--)
        {
            float theta = 2f * Mathf.PI * i / segments;
            points.Add(new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * radius);
        }

        return points;
    }
    List<Vector2> VTemplate()
    {
        return new List<Vector2>
        {
            new Vector2(-1, 1),
            new Vector2(0, -1),
            new Vector2(1, 1)
        };
    }
    List<Vector2> VTemplateR()
    {
        return new List<Vector2>
        {
            new Vector2(1, 1),
            new Vector2(0, -1),
            new Vector2(-1, 1)
        };
    }

    List<Vector2> LineTemplate()
    {
        return new List<Vector2>
        {
            new Vector2(-1, 0),
            new Vector2(1, 0)
        };
    }
    List<Vector2> LineTemplateR()
    {
        return new List<Vector2>
        {
            new Vector2(1, 0),
            new Vector2(-1, 0)
        };
    }
    List<Vector2> CupTemplate()
    {
        return new List<Vector2>
        {
            new Vector2(-1, 1),
            new Vector2(0, 0),
            new Vector2(1, 1),
               new Vector2(0, 0),
             new Vector2(0, -2),
              new Vector2(1, -2),
        };
    }
    List<Vector2> LTemplate()
    {
        return new List<Vector2>
        {
            new Vector2(0, 1),
            new Vector2(0, -1),
            new Vector2(1, -1),

        };
    }
    List<Vector2> LTemplateR()
    {
        return new List<Vector2>
        {
            new Vector2(1, -1),
            new Vector2(0, 1),
            new Vector2(0, -1),


        };
    }

    List<Vector2> SquareTemplate()
    {
        return new List<Vector2> {
    new Vector2(0f, 0f),
    new Vector2(0f, 1f),
    new Vector2(1f, 1f),
    new Vector2(1f, 0f),
    new Vector2(0f, 0f)
};
    }
    List<Vector2> SquareTemplate2()
    {
        return new List<Vector2> {
    new Vector2(0f, 0f),
    new Vector2(0f, 2f),
    new Vector2(1f, 2f),
    new Vector2(1f, 0f),
    new Vector2(0f, 0f)
};
    }

    List<Vector2> SquareTemplate3()
    {
        return new List<Vector2> {
    new Vector2(0f, 0f),
    new Vector2(0f, 1f),
    new Vector2(2f, 1f),
    new Vector2(2f, 0f),
    new Vector2(0f, 0f)
};
    }
    List<Vector2> ZTemplate()
    {
        return new List<Vector2> {
    new Vector2(0f, 1f),
    new Vector2(1f, 1f),
    new Vector2(0f, 0f),
    new Vector2(1f, 0f)
};
    }
    List<Vector2> CTemplate()
    {
        return new List<Vector2> {
    new Vector2(0.8f, 0.8f),
    new Vector2(0.6f, 0.9f),
    new Vector2(0.4f, 0.9f),
    new Vector2(0.2f, 0.7f),
    new Vector2(0.2f, 0.3f),
    new Vector2(0.4f, 0.1f),
    new Vector2(0.6f, 0.1f),
    new Vector2(0.8f, 0.2f)
         };
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
