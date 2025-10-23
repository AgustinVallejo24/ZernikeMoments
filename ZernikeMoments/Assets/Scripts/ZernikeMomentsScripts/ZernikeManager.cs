using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
public class ZernikeManager : MonoBehaviour
{
    // Tama�o de la imagen para el procesamiento
    [SerializeField] TMP_Text text;
    public int imageSize = 64;
    // Orden m�ximo para los momentos de Zernike
    public int maxMomentOrder = 10;
    // Umbral de distancia para un reconocimiento exitoso
    public float recognitionThreshold = 0.5f;

    public RenderTexture renderTexture;

    public bool rotationSensitivity;

    public GameObject UICarga;
    public Image barrita;
    public GameObject UIDibujo;
    public GameObject DrawingTest;
    // Almacena los descriptores (magnitudes de momentos) de los s�mbolos de referencia.
   
    public List<ReferenceSymbol> referenceSymbols;
    public List<ReferenceSymbolGroup> newReferenceSymbolsList;
    public ZernikeProcessor _processor;
    private List<Vector2> _currentStrokePoints;
   
    public bool shouldLoad;
    string jsonPath;
       #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SyncFilesystem();
       #endif
    void Start()
    {
       

        jsonPath = Path.Combine(Application.persistentDataPath,"Saves", "symbols.json");
    
        _processor = new ZernikeProcessor(imageSize);
        _currentStrokePoints = new List<Vector2>();
//#if UNITY_WEBGL && !UNITY_EDITOR
//if(TryLoadFile(Path.Combine(Application.persistentDataPath,"Saves", "symbols.json")))
//{
//shouldLoad = false;

//}

//#else
     //   Debug.LogError("no funca el if");
        if (PlayerPrefs.HasKey("shouldLoad"))
        {
            shouldLoad = false;
        }
//#endif

        if (SceneManager.GetActiveScene().name == "Menu" && shouldLoad)
        {
            Debug.LogError("COMPUTEEEEEE");
            StartCoroutine(Compute());
            PlayerPrefs.SetInt("shouldLoad", 1);
  
        }
        else if(SceneManager.GetActiveScene().name != "Menu")
        {
            newReferenceSymbolsList.Clear();
            newReferenceSymbolsList = ReferenceSymbolStorage.LoadSymbols(Path.Combine(Application.persistentDataPath, "Saves", "symbols.json")).ToList();            
            UICarga.SetActive(false);
            UIDibujo.SetActive(true);
            DrawingTest.SetActive(true);
        }
        else 
        {
            UICarga.SetActive(false);
            UIDibujo.SetActive(true);
        }
    }

    public bool TryLoadFile(string path)
    {
        //string directoryPath = Path.Combine(Application.persistentDataPath, FOLDER);
        //string filePath = Path.Combine(directoryPath, FILENAME);

        // 1. Asegurarse de que la carpeta existe (CreateDirectory es seguro y no falla si ya existe)
//        Directory.CreateDirectory(path);



        try
        {
            // 2. Intentar leer el archivo directamente.
            // Si no existe, esto lanzar� una excepci�n (FileNotFoundException).
            //  jsonContent = File.ReadAllText(filePath);
            Directory.CreateDirectory(path);
#if UNITY_WEBGL && !UNITY_EDITOR
    SyncFilesystem(); // Esta funci�n llama a FS.syncfs(false, ...) en el plugin .jslib
#endif
            // Si la lectura fue exitosa:
            return false; // El archivo existe y se carg�
        }
        catch (FileNotFoundException)
        {
            // La excepci�n indica que el archivo no existe.
            Debug.Log($"Archivo  NO encontrado en WebGL. Creando uno nuevo.");
            return false; // El archivo no existe
        }
        catch (Exception ex)
        {
            // Manejar otros posibles errores (permisos, etc.)
            Debug.LogError($"Error inesperado al cargar el archivo en WebGL: {ex.Message}");
            return true;
        }
    }
    public void DeleteSaveFolder(string path)
    {
        string directoryPath = path;

        if (TryLoadFile(path))
        {
            Directory.Delete(directoryPath, true);
            Debug.Log("Carpeta borrada.");

            // 2. Llamar a la funci�n para guardar el cambio en IndexedDB
        #if UNITY_WEBGL && !UNITY_EDITOR
                SyncFilesystem();
        #endif
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.LogError("AAAAAAAAA");
            PlayerPrefs.DeleteAll();


        }
    }

    public void SaveSymbolList()
    {
        ReferenceSymbolStorage.SaveSymbols(newReferenceSymbolsList, jsonPath);
    }
    IEnumerator Compute()
    {
        float carga = 0;

        //Directory.Delete(Path.Combine(Application.dataPath, "Resources/Template Images/InProjectTemplates"), true);
     //   if(File.Exists(Path.Combine(Application.persistentDataPath, "Images/TemplateImages/InProjectTemplates")))
      //  Directory.Delete(Path.Combine(Application.persistentDataPath, "Images/TemplateImages/InProjectTemplates"), true);


        foreach (var group in newReferenceSymbolsList)
        {
            foreach (var reference in group.symbols)
            {

                if (reference.templateTexture != null)
                {
                    // Procesar la textura para obtener la matriz binaria
                    reference.templateTexture = _processor.ResizeImage(reference.templateTexture, 64);
                    reference.templateTexture.name = reference.symbolName;
                    Debug.Log(reference.templateTexture.height);
                    _processor.DrawTexture(reference.templateTexture);

                    // Calcular la suma de todos los p�xeles activos para la normalizaci�n
                    float totalPixels = _processor.GetActivePixelCount();
                    Debug.Log("Divido por " + totalPixels);

                    ZernikeMoment[] moments = _processor.ComputeZernikeMoments(maxMomentOrder);

                    reference.momentMagnitudes = new List<double>();
                    // Normalizar y guardar las magnitudes
                    foreach (var moment in moments)
                    {
                        // Evitar divisi�n por cero
                        double normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
                        reference.momentMagnitudes.Add(normalizedMagnitude);
                    }

                    reference.distribution = _processor.GetSymbolDistribution();

                    reference.symbolID = Guid.NewGuid().ToString();

                    ImageUtils.SaveTexture(reference.templateTexture, reference.symbolID);
                }
                carga += 1f / referenceSymbols.Count;


                barrita.fillAmount = carga;

                yield return null;
            }
        }
        
        UICarga.SetActive(false);
        UIDibujo.SetActive(true);
        yield return new WaitForSeconds(.2f);
    //    DrawingTest.SetActive(true);
        ReferenceSymbolStorage.SaveSymbols(newReferenceSymbolsList, jsonPath);

        //  newReferenceSymbolsList = newReferenceSymbolsList.Concat(ReferenceSymbolStorage.LoadFromResources("drawnSymbols")).ToList();
    }
    public void OnDrawingFinished(List<List<Vector2>> finishedPoints, int strokeQuantity)
    {

        _processor.DrawStrokes(finishedPoints);
        float totalPixels = _processor.GetActivePixelCount();
        ZernikeMoment[] playerMoments = _processor.ComputeZernikeMoments(maxMomentOrder);
        float[] playerDistribution = _processor.GetSymbolDistribution();
        List<double> playerMagnitudes = new List<double>();
        foreach (var moment in playerMoments)
        {
            double normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
            playerMagnitudes.Add(normalizedMagnitude);
        }

        RecognizeSymbol(playerMagnitudes, strokeQuantity, playerDistribution);
    }


    public void SaveSymbol(List<List<Vector2>> finishedPoints, int strokeQuantity, string symbolName, string symbolID)
    {
       
        _processor.DrawStrokes(finishedPoints);
        float totalPixels = _processor.GetActivePixelCount();
        ZernikeMoment[] playerMoments = _processor.ComputeZernikeMoments(maxMomentOrder);
        float[] playerDistribution = _processor.GetSymbolDistribution();
        List<double> playerMagnitudes = new List<double>();
        foreach (var moment in playerMoments)
        {
            double normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
            playerMagnitudes.Add(normalizedMagnitude);
        }
        
        ReferenceSymbol newSymbol = new ReferenceSymbol(symbolName, playerDistribution, playerMagnitudes, strokeQuantity, symbolID);
      //  string savePath = Path.Combine(Application.persistentDataPath, "Saves", "drawnSymbols.json");
        //ReferenceSymbolStorage.AppendSymbol(newSymbol, savePath);
        referenceSymbols.Add(newSymbol);
        ReferenceSymbolStorage.SaveSymbols(newReferenceSymbolsList, jsonPath);
        
        
    }
    public IEnumerator CheckFileExists(System.Action<bool> onResult, string path)
    {
        // 1. Construir la URL completa para el archivo.
        // WebGL solo puede acceder a estos archivos mediante el protocolo file:// o http://, por eso usamos UnityWebRequest.
        string fullPath = path;
        Debug.LogError("si funca el if");
        // 2. Usar UnityWebRequest para intentar obtener el archivo.
        // El m�todo GET sirve para verificar la existencia e iniciar la carga al mismo tiempo.
        using (UnityWebRequest request = UnityWebRequest.Get(fullPath))
        {
            // Esperar a que la solicitud termine
            yield return request.SendWebRequest();

            bool fileExists = false;

            // 3. Evaluar el resultado
            if (request.result == UnityWebRequest.Result.Success)
            {
                // La solicitud fue exitosa, lo que significa que el archivo existe en IndexedDB.
                fileExists = true;
                // Si solo quieres verificar la existencia, puedes ignorar 'request.downloadHandler.text'
            }
            else if (request.result == UnityWebRequest.Result.ProtocolError && request.responseCode == 404)
            {
                // El error 404 (Not Found) indica que no existe el archivo.
                fileExists = false;
            }
            else
            {
                // Otros errores (red, etc.)
                Debug.LogError($"Error al verificar el archivo: {request.error}");
                fileExists = false;
            }

            // Llamar al callback con el resultado
            onResult?.Invoke(fileExists);
        }
    }
    public ReferenceSymbol ReturnNewSymbol(List<List<Vector2>> finishedPoints, int strokeQuantity, string symbolName, string symbolID)
    {
        _processor.DrawStrokes(finishedPoints);
        float totalPixels = _processor.GetActivePixelCount();
        ZernikeMoment[] playerMoments = _processor.ComputeZernikeMoments(maxMomentOrder);
        float[] playerDistribution = _processor.GetSymbolDistribution();
        List<double> playerMagnitudes = new List<double>();
        foreach (var moment in playerMoments)
        {
            double normalizedMagnitude = totalPixels > 0 ? moment.magnitude / totalPixels : 0;
            playerMagnitudes.Add(normalizedMagnitude);
        }

        return new ReferenceSymbol(symbolName, playerDistribution, playerMagnitudes, strokeQuantity, symbolID);

    }

    private void RecognizeSymbol(List<double> playerMagnitudes, int strokeQuantity, float[] playerDrawDistribution)
    {
        // 1. Filtrar los s�mbolos de referencia que coinciden con la cantidad de trazos.
        var relevantSymbols = newReferenceSymbolsList.Where(x => x.strokes == strokeQuantity).ToList();

        if (relevantSymbols.Count == 0)
        {
            text.text = "No hay ning�n s�mbolo con esa cantidad de trazos.";
            return;
        }

        // 2. Encontrar el mejor candidato. Las variables se pasan por 'out' para ser asignadas dentro de la funci�n.
        FindBestCandidate(playerMagnitudes, playerDrawDistribution, relevantSymbols, out ReferenceSymbolGroup bestMatch,
            out double minDistance, out float bestDistributionDiff, out ReferenceSymbolGroup closestMismatch, out double closestMismatchDist);

        // 3. Mostrar el resultado final en la interfaz.
        DisplayResult(bestMatch, minDistance, bestDistributionDiff, closestMismatch, closestMismatchDist);
    }

    private void FindBestCandidate(
    List<double> playerMagnitudes,
    float[] playerDrawDistribution,
    List<ReferenceSymbolGroup> candidates,
    out ReferenceSymbolGroup bestMatch,
    out double minDistance,
    out float bestDistributionDiff,
    out ReferenceSymbolGroup closestMismatch,
    out double closestMismatchDist)
    {
        // Inicializar los valores de salida
        bestMatch = new ReferenceSymbolGroup();
        minDistance = double.MaxValue;
        bestDistributionDiff = 0;
        closestMismatch = new ReferenceSymbolGroup();
        closestMismatchDist = double.MaxValue;

        foreach (var group in candidates)
        {
            foreach (var reference in group.symbols)
            {
                // A. Calcula las m�tricas de similitud.
                double zernikeDistance = CalculateZernikeDistance(playerMagnitudes, reference.momentMagnitudes);
                float distributionDifference = _processor.CompareAngularHistograms(reference.distribution, playerDrawDistribution);

                // B. Calcula la "puntuaci�n" final, aplicando penalizaci�n si es sim�trico.
                double finalDistance = zernikeDistance;
                if (group.isSymmetric)
                {
                    finalDistance += distributionDifference;
                }

                // C. Verifica si el s�mbolo cumple con los umbrales requeridos.
                bool meetsThresholds = CheckThresholds(finalDistance, distributionDifference, group);

                // D. Actualiza el mejor candidato o el "casi" candidato.
                if (meetsThresholds && finalDistance < minDistance)
                {
                    minDistance = finalDistance;
                    bestMatch = group;
                    bestDistributionDiff = distributionDifference;
                }
                else if (!meetsThresholds && finalDistance < closestMismatchDist)
                {
                    closestMismatchDist = finalDistance;
                    closestMismatch = group;
                }

                Debug.Log($"Distancia con '{reference.symbolName}': {finalDistance:F3}");
            }
        }
       
    }

    public double CalculateZernikeDistance(List<double> playerMagnitudes, List<double> referenceMagnitudes)
    {
        double distanceSquared = 0;
        int count = Mathf.Min(playerMagnitudes.Count, referenceMagnitudes.Count);

        for (int i = 0; i < count; i++)
        {
            double diff = playerMagnitudes[i] - referenceMagnitudes[i];
            distanceSquared += diff * diff;
        }

        return Math.Sqrt(distanceSquared) * 100;
    }

    private bool CheckThresholds(double distance, float distributionDifference, ReferenceSymbolGroup reference)
    {
        if (distance > reference.Threshold)
        {
            return false;
        }

        if (rotationSensitivity && reference.useRotation)
        {
            if (distributionDifference > reference.orientationThreshold)
            {
                return false;
            }
        }

        return true;
    }

    private void DisplayResult(
    ReferenceSymbolGroup bestMatch,
    double minDistance,
    float distributionDiff,
    ReferenceSymbolGroup closestMismatch,
    double closestMismatchDist)
    {
        Debug.Log(bestMatch.symbolName);
        if (bestMatch.symbols != null)
        {
            // Se encontr� una coincidencia v�lida.
            string resultText = $"S�mbolo reconocido: {bestMatch.symbolName}\nDistancia: {minDistance:F3}";

            if (rotationSensitivity && bestMatch.useRotation)
            {
                resultText += $", Dif. Distribuci�n: {distributionDiff:F3}";
            }
            text.text = resultText;
        }
        else if (closestMismatch.symbols.Count >0)
        {
            // No hubo coincidencias v�lidas, pero se muestra la m�s cercana que fall�.
            text.text = $"S�mbolo no reconocido. Match m�s cercano: '{closestMismatch.symbolName}' con distancia {closestMismatchDist:F3}";
        }
        else
        {
            // No se encontr� ning�n s�mbolo cercano.
            text.text = "S�mbolo no reconocido.";
        }
    }
}


[System.Serializable]
public struct ReferenceSymbolGroup
{

    public string symbolName;
    public List<ReferenceSymbol> symbols;

    public float Threshold;
    public float orientationThreshold;
    public bool useRotation;
    public bool isSymmetric;
    public int strokes;
}