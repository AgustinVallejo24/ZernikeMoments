using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;

public static class ReferenceSymbolStorage
{
    [Serializable]
    private class ReferenceSymbolWrapper
    {
        public List<ReferenceSymbolGroup> symbols;
    }

    // JSON por defecto para crear el archivo si no existe
    private static readonly string DEFAULT_JSON_CONTENT = JsonUtility.ToJson(new ReferenceSymbolWrapper { symbols = new List<ReferenceSymbolGroup>() }, true);

    private static readonly string FOLDER = "Saves";
    private static readonly string FILENAME = "symbols.json";

    [DllImport("__Internal")]
    private static extern void SyncFilesystem();

    private static string GetFilePath()
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, FOLDER);
        // Asegurar que la carpeta exista antes de intentar cualquier I/O
        try { Directory.CreateDirectory(directoryPath); }
        catch (IOException) { /* Ignorar si ya existe */ }

        return Path.Combine(directoryPath, FILENAME);
    }

    // ----------------------------------------------------------------------------------
    //  FUNCIÓN DE GUARDADO (Ahora simplificada)
    // ----------------------------------------------------------------------------------
    public static void SaveSymbols(List<ReferenceSymbolGroup> symbols, string path)
    {
        string filePath = GetFilePath();

        ReferenceSymbolWrapper wrapper = new ReferenceSymbolWrapper { symbols = symbols };
        string json = JsonUtility.ToJson(wrapper, true);

        // Escritura
        File.WriteAllText(filePath, json);

        // SINCRONIZACIÓN OBLIGATORIA
#if UNITY_WEBGL && !UNITY_EDITOR
            SyncFilesystem();
            Debug.Log("Símbolos guardados y sincronizados en WebGL: " + filePath);
#else
        Debug.Log("Símbolos guardados en Editor: " + filePath);
#endif
    }

    // ----------------------------------------------------------------------------------
    //  FUNCIÓN DE CARGA/CREACIÓN (El reemplazo del File.Exists)
    // ----------------------------------------------------------------------------------
    public static List<ReferenceSymbolGroup> LoadSymbols(string path)
    {
        string filePath = GetFilePath();
        string json = null;

        // 1. Intentar leer el archivo para verificar existencia
        try
        {
            json = File.ReadAllText(filePath);
            Debug.Log("Símbolos cargados exitosamente.");
        }
        catch (FileNotFoundException)
        {
            // 2. Si no existe, crear el archivo por defecto y sincronizar
            Debug.LogWarning($"Archivo {FILENAME} no encontrado. Creando archivo por defecto.");

            json = DEFAULT_JSON_CONTENT;
            File.WriteAllText(filePath, json);

#if UNITY_WEBGL && !UNITY_EDITOR
                SyncFilesystem(); // Sincroniza la creación
#endif
        }
        catch (Exception ex)
        {
            // 3. Manejar otros errores graves
            Debug.LogError($"Error al leer el archivo. Usando datos por defecto: {ex.Message}");
            return new List<ReferenceSymbolGroup>();
        }

        // 4. Deserialización
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("Contenido JSON vacío. Usando lista vacía.");
            return new List<ReferenceSymbolGroup>();
        }

        try
        {
            ReferenceSymbolWrapper wrapper = JsonUtility.FromJson<ReferenceSymbolWrapper>(json);
            return wrapper?.symbols ?? new List<ReferenceSymbolGroup>();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al deserializar JSON: {ex.Message}. Usando lista vacía.");
            return new List<ReferenceSymbolGroup>();
        }
    }

    // ----------------------------------------------------------------------------------
    //  FUNCIÓN DE AÑADIR (Ajustada para usar la nueva LoadSymbols y SaveSymbols)
    // ----------------------------------------------------------------------------------
    public static void AppendSymbol(ReferenceSymbol newSymbol)
    {
        // 1. Cargar el JSON (que lo crea si no existe)
        List<ReferenceSymbolGroup> current = LoadSymbols("a");

        // 2. Lógica de modificación (Mantenida sin cambios)
        ReferenceSymbolGroup existingGroup = current.FirstOrDefault(x => string.Equals(x.symbolName, newSymbol.symbolName, StringComparison.OrdinalIgnoreCase));

        if (existingGroup.symbolName != null)
        {
            existingGroup.symbols.Add(newSymbol);
        }
        else
        {
            ReferenceSymbolGroup newGroup = new ReferenceSymbolGroup
            {
                symbolName = newSymbol.symbolName,
                symbols = new List<ReferenceSymbol> { newSymbol }
            };
            current.Add(newGroup);
        }

        // 3. Guardar el contenido modificado
        SaveSymbols(current,"A");
        Debug.Log("Appended symbol: " + newSymbol.symbolName);
    }

    // NOTAS: Las funciones LoadFromResources y LoadSymbols(string path) original se han fusionado o eliminado por simplicidad.
    // La función LoadOrCreateJson fue ELIMINADA y su lógica integrada en LoadSymbols.
}