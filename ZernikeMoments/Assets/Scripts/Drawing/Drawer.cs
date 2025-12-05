using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.Collections;
using System;
public class Drawer : MonoBehaviour
{
  

    [SerializeField] LineRenderer lRPrefab;
    [SerializeField] LineRenderer lRPrefabRT;
    [SerializeField] RenderTexture renderTexture;
    public TMP_Text resultText;
    public ZernikeManager zRecognizer;
   
    private Camera cam;
    private List<Vector2> currentPoints = new();
    private List<Vector2> currentStrokePoints = new();
    private bool isDrawing = false;

    private List<LineRenderer> _lineRenderers = new List<LineRenderer>(0);
    private List<LineRenderer> _renderTexturelineRenderers = new List<LineRenderer>(0);
    private LineRenderer currentLR;
    List<List<Vector2>> listaDeListas = new List<List<Vector2>>();
    private int _linerendererIndex;
    private List<int> _strokesPointsCount = new List<int>();

    

    private void Start()
    {
        cam = Camera.main;
        var lineR = Instantiate(lRPrefab, transform);
        _lineRenderers.Add(lineR);
        currentLR = lineR;
    }
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

            

            if (GetLineLength(currentLR) < .8f)
            {
                foreach (var item in currentStrokePoints)
                {
                    if (currentPoints.Contains(item))
                        currentPoints.Remove(item);
                }

                currentStrokePoints.Clear();
                currentLR.positionCount = 0;
                isDrawing = false;
                return;
            }

            if (isDrawing)
            {
                isDrawing = false;
                listaDeListas.Add(new List<Vector2>());
                _linerendererIndex++;
                currentLR = Instantiate(lRPrefab, this.transform);
                _lineRenderers.Add(currentLR);
                _strokesPointsCount.Add(currentStrokePoints.Count);
                currentStrokePoints.Clear();
               
            }

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


            _lineRenderers[_linerendererIndex].positionCount = 1;
            _lineRenderers[_linerendererIndex].SetPosition(0, newPoint);
            return;
        }

        if (count < 4)
        {
            
            _lineRenderers[_linerendererIndex].positionCount++;
            _lineRenderers[_linerendererIndex].SetPosition(_lineRenderers[_linerendererIndex].positionCount - 1, newPoint);
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

            _lineRenderers[_linerendererIndex].positionCount++;
            _lineRenderers[_linerendererIndex].SetPosition(_lineRenderers[_linerendererIndex].positionCount - 1, point);

        }


    }

    public ReferenceSymbol ReturnNewSymbol(string symbolName,bool shouldRender)
    {
        if (currentPoints.Any())
        {
            var normalizedPositions = DrawNormalizer.Normalize(currentPoints);
            for (int i = 0; i < _strokesPointsCount.Count; i++)
            {
                if (shouldRender)
                {
                    _renderTexturelineRenderers.Add(Instantiate(lRPrefabRT, transform));
                    _renderTexturelineRenderers[i].positionCount = _strokesPointsCount[i];
                }

                if (_strokesPointsCount[i] == 0) continue;
                for (int j = 0; j < _strokesPointsCount[i]; j++)
                {
                    listaDeListas[i].Add(normalizedPositions[j]);
                    if (shouldRender)
                    {
                        _renderTexturelineRenderers[i].SetPosition(j, normalizedPositions[j]);
                    }
                  
                }
                normalizedPositions = normalizedPositions.Skip(_strokesPointsCount[i]).ToList();
            }



            return zRecognizer.ReturnNewSymbol(listaDeListas, _lineRenderers.Count - 1, symbolName, "symbolID");
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
            for (int i = 0; i < _strokesPointsCount.Count; i++)
            {
                if (_strokesPointsCount[i] == 0) continue;
                for (int j = 0; j < _strokesPointsCount[i]; j++)
                {
                    listaDeListas[i].Add(normalizedPositions[j]);
                    
                }
                normalizedPositions = normalizedPositions.Skip(_strokesPointsCount[i]).ToList();
            }
    
         
            zRecognizer.OnDrawingFinished(listaDeListas, _lineRenderers.Count -1);
            currentPoints.Clear();
            _strokesPointsCount.Clear();
            foreach (var item in listaDeListas)
            {
                item.Clear();
            }
            _linerendererIndex = 0;
            _lineRenderers[0].positionCount = 0;
            var count = _lineRenderers.Count;
            for (int i = 1; i < count; i++)
            {
                Destroy(_lineRenderers[1].gameObject);
                _lineRenderers.RemoveAt(1);
            }

            currentLR = _lineRenderers[0];
        }
        else
        {
            resultText.text = "The stroke is too short";
            currentPoints.Clear();
            _strokesPointsCount.Clear();
            foreach (var item in listaDeListas)
            {
                item.Clear();
            }
            _linerendererIndex = 0;
            _lineRenderers[0].positionCount = 0;
            foreach (var item in _lineRenderers.Skip(1))
            {
                _lineRenderers.Remove(item);
                Destroy(item.gameObject);
            }
            currentLR = _lineRenderers[0];
        }
    }
 

      public void ClearAllLineRenderers(bool clearRenderDraw)
    {
        currentPoints.Clear();
        _strokesPointsCount.Clear();
        foreach (var item in listaDeListas)
        {
            item.Clear();
        }
        _linerendererIndex = 0;
        _lineRenderers[0].positionCount = 0;
        var count = _lineRenderers.Count;
        for (int i = 1; i < count; i++)
        {
            Destroy(_lineRenderers[1].gameObject);
            _lineRenderers.RemoveAt(1);
        }

        currentLR = _lineRenderers[0];
        if (clearRenderDraw)
        {
            foreach (var item in _renderTexturelineRenderers)
            {
                Destroy(item.gameObject);
            }
            _renderTexturelineRenderers.Clear();
        }


    }

}
