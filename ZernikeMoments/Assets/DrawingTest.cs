using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections;
public class DrawingTest : MonoBehaviour
{
    public LineRenderer[] lineRenderer;
    public int linerendererIndex;
    public LineRenderer recognitionLineRenderer;
    public TMP_Text resultText;
    public Camera cam;
    //public GestureRecognizer recognizer;
 
    private List<Vector2> currentPoints = new();
    private List<Vector2> currentStrokePoints = new();
    private bool isDrawing = false;
    List<Vector2> greenPoints = new List<Vector2>();
    List<Vector2> redPoints = new List<Vector2>();

    public ZernikeManager zRecognizer;
    void Start()
    {

       // recognizer = new GestureRecognizer();
       // recognizer.AddTemplate("Line", LineTemplate(), 0.02f);
       // //recognizer.AddTemplate("Line", LineTemplateR(),0.05f);
       // recognizer.AddTemplate("Circle", CircleTemplate(), .035f);
       // //recognizer.AddTemplate("Circle", CircleTemplateR(), 1.5f);
       // recognizer.AddTemplate("V", VTemplate(), .05f);
       // //recognizer.AddTemplate("V", VTemplateR(), .05f);
       // recognizer.AddTemplate("Cup", CupTemplate(), .03f);
       // recognizer.AddTemplate("L", LTemplate(), .05f);
       //// recognizer.AddTemplate("L", LTemplateR(), .05f);
       // recognizer.AddTemplate("Z", ZTemplate(), .07f);
       // recognizer.AddTemplate("C", CTemplate(), .05f);
       // recognizer.AddTemplate("Square", SquareTemplate(), .1f);
       // recognizer.AddTemplate("Square", SquareTemplate2(), .1f);
       // recognizer.AddTemplate("Square", SquareTemplate3(), .1f);

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && linerendererIndex < lineRenderer.Length)
        {
            isDrawing = true;
            //lineRenderer[linerendererIndex].SetPosition(0, cam.ScreenToWorldPoint(Input.mousePosition));

        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;

            if(linerendererIndex < lineRenderer.Length)
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
            if (currentPoints.Count == 0 || Vector2.Distance(currentPoints[^1], pos) > 0.01f)
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
