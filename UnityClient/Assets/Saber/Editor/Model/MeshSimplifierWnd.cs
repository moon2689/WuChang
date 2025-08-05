using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityMeshSimplifier;
using UObj = UnityEngine.Object;

public class MeshSimplifierWnd : EditorWindow
{
    private const string SimpleMeshEndString = "_Simple";

    private int m_Quality = 50;
    private UObj m_SelectedObj;

    private SimplificationOptions m_Options = new()
    {
        PreserveBorderEdges = true,
        PreserveUVSeamEdges = false,
        PreserveUVFoldoverEdges = false,
        PreserveSurfaceCurvature = false,
        EnableSmartLink = false,
        VertexLinkDistance = double.Epsilon,
        MaxIterationCount = 100,
        Agressiveness = 7.0,
        ManualUVComponentCount = false,
        UVComponentCount = 2,
    };


    [MenuItem("美术/简化网格")]
    private static void CreateWnd()
    {
        CreateWindow<MeshSimplifierWnd>("减面工具");
    }

    private void OnGUI()
    {
        m_SelectedObj = EditorGUILayout.ObjectField("模型：", m_SelectedObj, typeof(SkinnedMeshRenderer), true);
        m_Quality = EditorGUILayout.IntField("质量", m_Quality);
        m_Options.PreserveBorderEdges = EditorGUILayout.Toggle("保留边界边缘", m_Options.PreserveBorderEdges);
        m_Options.PreserveUVSeamEdges = EditorGUILayout.Toggle("保留UV接缝边缘", m_Options.PreserveUVSeamEdges);
        m_Options.PreserveUVFoldoverEdges = EditorGUILayout.Toggle("保留UV折叠边缘", m_Options.PreserveUVFoldoverEdges);
        m_Options.PreserveSurfaceCurvature = EditorGUILayout.Toggle("保留曲面曲率", m_Options.PreserveSurfaceCurvature);
        m_Options.EnableSmartLink = EditorGUILayout.Toggle("启用SmartLink", m_Options.EnableSmartLink);
        m_Options.VertexLinkDistance = EditorGUILayout.DoubleField("VertexLinkDistance", m_Options.VertexLinkDistance);
        m_Options.MaxIterationCount = EditorGUILayout.IntField("最大迭代次数", m_Options.MaxIterationCount);
        m_Options.Agressiveness = EditorGUILayout.DoubleField("Agressiveness", m_Options.Agressiveness);
        m_Options.ManualUVComponentCount = EditorGUILayout.Toggle("ManualUVComponentCount", m_Options.ManualUVComponentCount);
        m_Options.UVComponentCount = EditorGUILayout.IntField("UVComponentCount", m_Options.UVComponentCount);

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("简化网格", GUILayout.Width(120)))
        {
            SimplifySMRMesh(m_SelectedObj as SkinnedMeshRenderer);
        }
        else if (GUILayout.Button("撤销简化网格", GUILayout.Width(120)))
        {
            UndoSimplifySMRMesh(m_SelectedObj as SkinnedMeshRenderer);
        }

        GUILayout.EndHorizontal();
    }

    /*
    private List<SkinnedMeshRenderer> GetSelectedSMRs()
    {
        List<SkinnedMeshRenderer> list = new();
        foreach (var go in Selection.gameObjects)
        {
            SkinnedMeshRenderer[] array = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var item in array)
            {
                if (item.enabled)
                    list.Add(item);
            }
        }

        return list;
    }
    */

    private void SimplifySMRMesh(SkinnedMeshRenderer smr)
    {
        if (smr == null)
        {
            Debug.LogError("smr == null", smr);
            return;
        }

        if (smr.sharedMesh.subMeshCount > 1)
        {
            Debug.LogError("originMesh.subMeshCount > 1", smr);
            return;
        }

        TryCopyMeshFromFBX(smr.sharedMesh);
        UndoSimplifySMRMesh(smr);

        Mesh newMesh = SimplifyMesh(smr.sharedMesh, m_Quality / 100f, m_Options);

        string newPath = GetSimpleMeshSavePath(smr.sharedMesh);
        AssetDatabase.CreateAsset(newMesh, newPath);
        smr.sharedMesh = newMesh;
        EditorUtility.SetDirty(smr);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        smr.sharedMesh = newMesh;

        Debug.Log($"简化网格成功：{smr.name}", smr);
    }

    private void TryCopyMeshFromFBX(Mesh originMesh)
    {
        string path = AssetDatabase.GetAssetPath(originMesh);
        if (!path.EndsWith(".FBX"))
            return;

        string folderPath = Path.GetDirectoryName(path).Replace("\\", "/");
        string newPath = folderPath.Replace("Role/Cloth/", "TA/CombinedCloth/");
        newPath = newPath.TrimEnd('/') + '/';

        if (!AssetDatabase.IsValidFolder(newPath + "SubMesh"))
            AssetDatabase.CreateFolder(newPath, "SubMesh");

        newPath += $"SubMesh/{originMesh.name}.asset";

        Mesh subMesh = originMesh.GetSubmesh(0);
        AssetDatabase.CreateAsset(subMesh, newPath);
    }

    private string GetSimpleMeshSavePath(Mesh originMesh)
    {
        string path = AssetDatabase.GetAssetPath(originMesh);

        string newPathWithOutExt;
        if (path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
        {
            // Assets/FashionBeat_ArtSVN/Asset/NewRoleAsset/Role/Cloth_H/g_cloth_5000004/g_cloth_5000004.FBX
            // Assets/FashionBeat_ArtSVN/Asset/NewRoleAsset/TA/CombinedCloth/g_cloth_5000004/SubMesh
            string folderPath = Path.GetDirectoryName(path).Replace("\\", "/");
            // newPath = folderPath.Replace("Role/Cloth/", "TA/CombinedCloth/");
            // newPath += $"/SubMesh/{originMesh.name}{SimpleMeshEndString}{quality}.asset";

            newPathWithOutExt = folderPath.Replace("Role/Cloth/", "TA/CombinedCloth/");
            newPathWithOutExt += $"/{originMesh.name}{SimpleMeshEndString}{m_Quality}";
        }
        else if (path.EndsWith(".asset"))
        {
            // newPath = path.Replace(".asset", $"{SimpleMeshEndString}{quality}.asset");
            newPathWithOutExt = path.Replace(".asset", $"{SimpleMeshEndString}{m_Quality}");
        }
        else
        {
            throw new InvalidOperationException("Unknown mesh path:" + path);
        }

        string newPath = newPathWithOutExt + ".asset";
        int count = 1;
        while (File.Exists(newPath))
        {
            newPath = newPathWithOutExt + $"({count}).asset";
            count++;
        }

        return newPath;
    }

    public static Mesh SimplifyMesh(Mesh mesh, float quality, SimplificationOptions options)
    {
        var meshSimplifier = new MeshSimplifier();
        meshSimplifier.SimplificationOptions = options;
        meshSimplifier.Initialize(mesh);
        meshSimplifier.SimplifyMesh(quality);

        var simplifiedMesh = meshSimplifier.ToMesh();
        simplifiedMesh.bindposes = mesh.bindposes;
        return simplifiedMesh;
    }

    private void UndoSimplifySMRMesh(SkinnedMeshRenderer smr)
    {
        Mesh mesh = smr.sharedMesh;
        string path = AssetDatabase.GetAssetPath(mesh);
        if (!path.Contains($"{SimpleMeshEndString}"))
            return;

        AssetDatabase.DeleteAsset(path);

        string[] words = path.Split(SimpleMeshEndString);
        string originMeshPath = $"{words[0]}.asset";
        Mesh originMesh = AssetDatabase.LoadAssetAtPath<Mesh>(originMeshPath);
        smr.sharedMesh = originMesh;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"撤消简化网格成功：{smr.name}", smr);
    }
}