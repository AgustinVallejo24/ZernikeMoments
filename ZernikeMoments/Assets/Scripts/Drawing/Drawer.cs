using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.Collections;
using System;
public class Drawer : MonoBehaviour
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


    void Update()
    {
   
        if (Input.GetMouseButtonDown(0))
        {
           
            if (IsPointerOverUI())
                return;

            isDrawing = true;
            currentStrokePoints.Clear();
        }

        
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

    bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null &&
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }



    void AddSmoothedPoint(Vector3 newPoint)
    {
        currentStrokePoints.Add(newPoint);
        currentPoints.Add(newPoint);
        int count = currentStrokePoints.Count;

        if (count == 1)
        {


            lineRenderers[linerendererIndex].positionCount = 1;
            lineRenderers[linerendererIndex].SetPosition(0, newPoint);
            return;
        }

        if (count < 4)
        {
            
            lineRenderers[linerendererIndex].positionCount++;
            lineRenderers[linerendererIndex].SetPosition(lineRenderers[linerendererIndex].positionCount - 1, newPoint);
            return;
        }


        Vector3 p0 = currentStrokePoints[count - 4];
        Vector3 p1 = currentStrokePoints[count - 3];
        Vector3 p2 = currentStrokePoints[count - 2];
        Vector3 p3 = currentStrokePoints[count - 1];

        int subdivisions = 4; 
        for (int j = 1; j <= subdivisions; j++) 
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
            var normalizedPositions = DrawNormalizer.Normalize(currentPoints);
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
            var normalizedPositions = DrawNormalizer.Normalize(currentPoints);
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
 

    IEnumerator SaveSymbolCoroutine()
    {
        if (currentPoints.Any())
        {
            var normalizedPositions = DrawNormalizer.Normalize(currentPoints);
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

}
