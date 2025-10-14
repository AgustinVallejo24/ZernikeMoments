using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
public static class ReferenceSymbolStorage
{
    [Serializable]
    private class ReferenceSymbolWrapper
    {
        public List<ReferenceSymbol> symbols;
    }

    public static void SaveSymbols(List<ReferenceSymbol> symbols, string path)
    {
        
        ReferenceSymbolWrapper wrapper = new ReferenceSymbolWrapper { symbols = symbols };
        string json = JsonUtility.ToJson(wrapper, true);

        File.WriteAllText(path, json);
        Debug.Log("Symbols saved to: " + path);
    }

    public static List<ReferenceSymbol> LoadFromResources(string resourceName)
    {
        // Carga el archivo como TextAsset desde Resources
        AssetDatabase.Refresh();
        TextAsset jsonFile = Resources.Load<TextAsset>(resourceName);
        if (jsonFile == null)
        {
            Debug.LogError("JSON not found in Resources: " + resourceName);
            return new List<ReferenceSymbol>();
        }

        // Deserializa
        ReferenceSymbolWrapper wrapper = JsonUtility.FromJson<ReferenceSymbolWrapper>(jsonFile.text);
        return wrapper.symbols ?? new List<ReferenceSymbol>();
    }

    public static List<ReferenceSymbol> LoadSymbols(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("File not found: " + path);
            return new List<ReferenceSymbol>();
        }

        string json = File.ReadAllText(path);
        ReferenceSymbolWrapper wrapper = JsonUtility.FromJson<ReferenceSymbolWrapper>(json);

        return wrapper.symbols ?? new List<ReferenceSymbol>();
    }


    public static void AppendSymbol(ReferenceSymbol newSymbol, string path)
    {
        List<ReferenceSymbol> current = LoadSymbols(path);
        current.Add(newSymbol);
        SaveSymbols(current, path); // reusa la función de arriba
        Debug.Log("Appended symbol: " + newSymbol.symbolName + " to " + path);
    }

}
