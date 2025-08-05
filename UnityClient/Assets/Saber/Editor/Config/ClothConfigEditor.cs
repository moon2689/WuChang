using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Config;
using UnityEditor;
using UnityEngine;

public class ClothConfigEditor : EditorWindow
{
    private string m_Keyword;

    [MenuItem("Saber/Config/Search Cloth")]
    private static void CreateWnd()
    {
        EditorWindow.CreateWindow<ClothConfigEditor>("ClothConfigEditor");
    }

    private void OnGUI()
    {
        m_Keyword = GUILayout.TextField(m_Keyword);
        if (GUILayout.Button("Search"))
        {
            Search();
        }
    }

    void Search()
    {
        string path = "Assets/Saber/Resources/Config/ClothInfo.asset";
        ClothInfo clothInfo = AssetDatabase.LoadAssetAtPath<ClothInfo>(path);

        int.TryParse(m_Keyword, out int tarID);

        foreach (var c in clothInfo.m_Clothes)
        {
            if (c.m_PrefabName.Contains(m_Keyword, StringComparison.OrdinalIgnoreCase) ||
                c.m_Name.Contains(m_Keyword, StringComparison.OrdinalIgnoreCase) ||
                c.m_ID == tarID)
            {
                Debug.Log($"{c.m_ID} {c.m_Name} {c.m_PrefabName}");
            }
        }
    }
}