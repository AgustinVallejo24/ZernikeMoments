using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
public static class ReferenceSymbolStorage
{
    [Serializable]
    private class ReferenceSymbolWrapper
    {
        public List<ReferenceSymbolGroup> symbols;
    }




      
    public static void SaveSymbols(List<ReferenceSymbolGroup> symbols, string path)
    {

        ReferenceSymbolWrapper wrapper = new ReferenceSymbolWrapper { symbols = symbols };
        string json = JsonUtility.ToJson(wrapper, true);

        File.WriteAllText(path, json);
        Debug.Log("Symbols saved to: " + path);
    }

    public static List<ReferenceSymbolGroup> LoadFromResources(string resourceName)
    {
        // Carga el archivo como TextAsset desde Resources
        //      AssetDatabase.Refresh();
       #if UNITY_WEBGL && !UNITY_EDITOR
       #else
        AssetDatabase.Refresh();
       #endif
        TextAsset jsonFile = Resources.Load<TextAsset>(resourceName);
        if (jsonFile == null)
        {
            Debug.LogError("JSON not found in Resources: " + resourceName);
            return new List<ReferenceSymbolGroup>();
        }

        // Deserializa
        ReferenceSymbolWrapper wrapper = JsonUtility.FromJson<ReferenceSymbolWrapper>(jsonFile.text);
        return wrapper.symbols ?? new List<ReferenceSymbolGroup>();
    }

    public static List<ReferenceSymbolGroup> LoadSymbols(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("File not found: " + path);
            return new List<ReferenceSymbolGroup>();
        }

        string json = File.ReadAllText(path);
        ReferenceSymbolWrapper wrapper = JsonUtility.FromJson<ReferenceSymbolWrapper>(json);

        return wrapper.symbols ?? new List<ReferenceSymbolGroup>();
    }


    public static void AppendSymbol(ReferenceSymbol newSymbol, string path)
    {
        List<ReferenceSymbolGroup> current = LoadSymbols(path);
        ReferenceSymbolGroup newGroup = new ReferenceSymbolGroup();
        newGroup.symbolName = newSymbol.symbolName;
        newGroup.symbols.Add(newSymbol);
        current.Add(newGroup);
        SaveSymbols(current, path); // reusa la función de arriba
        Debug.Log("Appended symbol: " + newSymbol.symbolName + " to " + path);
    }



}
