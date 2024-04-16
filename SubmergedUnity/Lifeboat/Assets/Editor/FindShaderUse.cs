using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

public class FindShaderUse : EditorWindow {
    [MenuItem("Window/Find Shader Use")]
    public static void ShowWindow() {
        GetWindow(typeof(FindShaderUse));
    }

    public string ShaderInfo;
    
    public void OnGUI()
    {
        if (GUILayout.Button("Find Materials"))
        {
            FindMaterials();
        }
        
        GUILayout.Label(ShaderInfo);
    }

    public void FindMaterials()
    {
        Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
        
        foreach (string allAssetPath in AssetDatabase.GetAllAssetPaths())
        {
            if (!allAssetPath.Contains("AmongUs")) continue;
            var asset = AssetDatabase.GetMainAssetTypeAtPath(allAssetPath);
            if (asset == typeof(Material))
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(allAssetPath);
                var shaderName = mat.shader.name;
                if (!dictionary.ContainsKey(shaderName)) dictionary[shaderName] = new List<string>();
                dictionary[shaderName].Add(Path.GetFileName(allAssetPath));
            }
        }

        var builder = new StringBuilder();
        foreach (var pair in dictionary)
        {
            builder.Append(pair.Key);
            builder.Append(": ");
            foreach (var matNames in pair.Value)
            {
                builder.Append(matNames);
                builder.Append(", ");
            }

            builder.AppendLine();
        }

        ShaderInfo = builder.ToString();
    }
}