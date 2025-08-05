using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UObj = UnityEngine.Object;

public class FurMeshGenerateWnd : EditorWindow
{
    public const int MAX_VERTICES_FOR_16BITS_MESH = 50000; //NOT change this

    private UObj m_SelectedObj;
    private int m_LayerCount = 10;
    private bool m_IsOptimize;
    private Mesh m_FurMesh;
    private string m_NewMeshSavePath;


    [MenuItem("美术/毛皮网格生成工具")]
    private static void CreateWnd()
    {
        EditorWindow.CreateWindow<FurMeshGenerateWnd>("毛皮网格生成工具");
    }

    private void OnGUI()
    {
        UObj oldSelectedObj = m_SelectedObj;
        m_SelectedObj = EditorGUILayout.ObjectField("模型：", m_SelectedObj, typeof(SkinnedMeshRenderer), true);
        bool selectedObjChange = m_SelectedObj != oldSelectedObj;

        m_LayerCount = EditorGUILayout.IntField("层数：", m_LayerCount);
        m_IsOptimize = EditorGUILayout.Toggle("是否优化mesh", m_IsOptimize);

        GUILayout.BeginHorizontal();
        if (m_SelectedObj != null && (string.IsNullOrEmpty(m_NewMeshSavePath) || selectedObjChange))
        {
            SkinnedMeshRenderer smr = m_SelectedObj as SkinnedMeshRenderer;
            m_NewMeshSavePath = GetFurMeshSavePath(smr.sharedMesh);
        }

        m_NewMeshSavePath = EditorGUILayout.TextField("毛皮网格保存路径：", m_NewMeshSavePath);
        if (GUILayout.Button("...", GUILayout.Width(40)))
        {
            string fileName = Path.GetFileNameWithoutExtension(m_NewMeshSavePath);
            m_NewMeshSavePath = EditorUtility.SaveFilePanelInProject("保存", fileName, "asset", "设置毛皮网格保存路径");
        }

        GUILayout.EndHorizontal();

        if (GUILayout.Button("生成毛皮"))
        {
            string error = GenerateFurMesh();
            if (!string.IsNullOrEmpty(error))
            {
                EditorUtility.DisplayDialog("错误", $"生成毛皮网格出错，信息：{error}", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("成功", $"生成毛皮网格成功！", "确定");
            }
        }

        if (m_FurMesh != null && GUILayout.Button("撤销生成毛皮"))
        {
            UndoGenerateFurMesh();
            EditorUtility.DisplayDialog("成功", $"撤销生成毛皮网格成功！", "确定");
        }
    }


    private void UndoGenerateFurMesh()
    {
        PrefabUtility.RevertObjectOverride(m_SelectedObj, InteractionMode.UserAction);
        string assetPath = AssetDatabase.GetAssetPath(m_FurMesh);
        AssetDatabase.DeleteAsset(assetPath);
    }


    private string GenerateFurMesh()
    {
        SkinnedMeshRenderer smr = m_SelectedObj as SkinnedMeshRenderer;
        if (smr == null)
        {
            return "请选择SkinnedMeshRenderer";
        }

        if (smr.sharedMesh.subMeshCount > 1)
        {
            return "目标不能有多个子网格";
        }

        Mesh newMesh = GenerateFurMesh(smr, out var bonesToMerge);
        SaveMesh(smr, newMesh);

        smr.sharedMesh = newMesh;
        smr.bones = bonesToMerge.ToArray();
        EditorUtility.SetDirty(smr);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"生成毛皮网格：{smr.name}", smr);
        return null;
    }

    private void SaveMesh(SkinnedMeshRenderer smr, Mesh newMesh)
    {
        string newPath = GetFurMeshSavePath(smr.sharedMesh);
        string saveFolder = Path.GetDirectoryName(newPath);
        if (!AssetDatabase.IsValidFolder(saveFolder))
        {
            string parentFolder = Path.GetDirectoryName(saveFolder);
            string folderName = Path.GetFileNameWithoutExtension(saveFolder);
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        AssetDatabase.CreateAsset(newMesh, newPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        m_FurMesh = AssetDatabase.LoadAssetAtPath<Mesh>(newPath);
    }

    private static string GetFurMeshSavePath(Mesh originMesh)
    {
        string path = AssetDatabase.GetAssetPath(originMesh);
        string newPath;
        if (path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
        {
            string folderPath = Path.GetDirectoryName(path).Replace("\\", "/");
            newPath = $"{folderPath}/{originMesh.name}_Fur.asset";
        }
        else if (path.EndsWith(".asset"))
        {
            newPath = path.Replace(".asset", "_Fur.asset");
        }
        else
        {
            throw new InvalidOperationException("Unknown mesh path:" + path);
        }

        return newPath;
    }

    private Mesh GenerateFurMesh(SkinnedMeshRenderer smr, out List<Transform> bonesToMerge)
    {
        CombineInstance[] combinesToMerge = new CombineInstance[m_LayerCount];
        //Matrix4x4[] bindPosesToMerge = new Matrix4x4[smr.bones.Length * m_layerCount];
        //bonesToMerge = new Transform[smr.bones.Length * m_layerCount];
        Vector2[] newUVs = new Vector2[smr.sharedMesh.uv.Length * m_LayerCount];
        Color[] newColors = new Color[smr.sharedMesh.vertexCount * m_LayerCount];
        //BoneWeight[] boneWeights = new BoneWeight[smr.sharedMesh.boneWeights.Length * m_layerCount];
        int indexUV = 0, indexColor = 0;
        bonesToMerge = new();
        List<Matrix4x4> bindPosesToMerge = new();
        List<BoneWeight> boneWeights = new();

        for (int i = 0; i < m_LayerCount; i++)
        {
            combinesToMerge[i] = new()
            {
                mesh = smr.sharedMesh,
                subMeshIndex = 0,
                transform = smr.transform.localToWorldMatrix,
            };

            for (int j = 0; j < smr.bones.Length; j++)
            {
                var curBone = smr.bones[j];
                if (!bonesToMerge.Contains(curBone))
                {
                    bonesToMerge.Add(curBone);
                    bindPosesToMerge.Add(smr.sharedMesh.bindposes[j] * smr.transform.worldToLocalMatrix);
                }
            }

            for (int j = 0; j < smr.sharedMesh.uv.Length; j++)
            {
                newUVs[indexUV] = smr.sharedMesh.uv[j];
                ++indexUV;
            }

            Color tarColor = new Color((float)i / m_LayerCount, 0, 0);
            for (int j = 0; j < smr.sharedMesh.vertexCount; j++)
            {
                newColors[indexColor] = tarColor;
                ++indexColor;
            }

            for (int j = 0; j < smr.sharedMesh.boneWeights.Length; j++)
            {
                BoneWeight bw = smr.sharedMesh.boneWeights[j];
                bw.boneIndex0 = bonesToMerge.FindIndex(a => a == smr.bones[bw.boneIndex0]);
                bw.boneIndex1 = bonesToMerge.FindIndex(a => a == smr.bones[bw.boneIndex1]);
                bw.boneIndex2 = bonesToMerge.FindIndex(a => a == smr.bones[bw.boneIndex2]);
                bw.boneIndex3 = bonesToMerge.FindIndex(a => a == smr.bones[bw.boneIndex3]);
                boneWeights.Add(bw);
            }
        }

        // Create mesh
        int verticesCount = smr.sharedMesh.vertexCount * m_LayerCount;
        Mesh finalMesh = new();
        finalMesh.indexFormat = verticesCount > MAX_VERTICES_FOR_16BITS_MESH ? IndexFormat.UInt32 : IndexFormat.UInt16;
        finalMesh.name = $"{smr.sharedMesh.name}_Fur";
        finalMesh.CombineMeshes(combinesToMerge, true, true);
        finalMesh.RecalculateBounds();
        finalMesh.bindposes = bindPosesToMerge.ToArray();
        finalMesh.boneWeights = boneWeights.ToArray();
        finalMesh.uv = newUVs;
        finalMesh.colors = newColors;

        if (m_IsOptimize)
            finalMesh.Optimize();
        return finalMesh;
    }
}