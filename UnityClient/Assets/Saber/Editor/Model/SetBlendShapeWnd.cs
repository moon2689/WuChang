using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class SetBlendShapeWnd :EditorWindow// OdinEditorWindow
{
    [Serializable]
    public class MeshVector3
    {
        public float m_X;
        public float m_Y;
        public float m_Z;

        public Vector3 ToVector3()
        {
            return new Vector3(m_X, m_Y, m_Z);
        }

        public MeshVector3(Vector3 v3)
        {
            m_X = v3.x;
            m_Y = v3.y;
            m_Z = v3.z;
        }
    }

    [Serializable]
    public class BlendShapeFrame
    {
        public string m_Name;
        public float m_Weight;
        public MeshVector3[] m_DeltaVertex;
        public MeshVector3[] m_DeltaNormal;
        public MeshVector3[] m_DeltaTangent;

        public static Vector3[] ToVertexArray(MeshVector3[] data)
        {
            Vector3[] array = new Vector3[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                array[i] = data[i].ToVector3();
            }

            return array;
        }
    }

    [Serializable]
    public class BlendShapeData
    {
        public int m_SubMeshIndex;
        public string m_SubMeshName;
        public List<BlendShapeFrame> m_Frames = new();
    }

    public class SubMeshGUIData
    {
        public int m_SubmeshIndex;
        public string m_SubmeshName;
        public Mesh m_TarMesh;
    }

     private SkinnedMeshRenderer m_SourceBlendShapeSMR;
     private SubMeshGUIData[] m_SubMeshGUIData;

     private string[] m_FrameNameRemoveStrings;
     private string[] m_FrameNames;

    private List<BlendShapeData> m_ListBlendShapeData;


    public static BlendShapeData GetBlendShapeData(Mesh aMesh, int aSubMeshIndex)
    {
        if (aSubMeshIndex < 0 || aSubMeshIndex >= aMesh.subMeshCount)
            return null;

        int[] indices = aMesh.GetTriangles(aSubMeshIndex);
        Dictionary<int, int> dicOriginIndexToNewIndex = new Dictionary<int, int>();
        int vertexCount = 0;
        for (int i = 0; i < indices.Length; i++)
        {
            int originIndex = indices[i];
            if (!dicOriginIndexToNewIndex.TryGetValue(originIndex, out int newIndex))
            {
                newIndex = vertexCount;
                dicOriginIndexToNewIndex.Add(originIndex, newIndex);
                ++vertexCount;
            }
        }

        Dictionary<int, int> dicNewIndexToOriginIndex = new();
        foreach (var pair in dicOriginIndexToNewIndex)
        {
            dicNewIndexToOriginIndex.Add(pair.Value, pair.Key);
        }

        BlendShapeData data = new();
        data.m_SubMeshIndex = aSubMeshIndex;
        GetBlendShapeData(aMesh, vertexCount, dicNewIndexToOriginIndex, data);
        return data;
    }

    static void GetBlendShapeData(Mesh from, int toMeshVertexCount, Dictionary<int, int> dicNewIndexToOriginIndex, BlendShapeData data)
    {
        Vector3[] vertexFrom = new Vector3[from.vertexCount];
        Vector3[] normalFrom = new Vector3[from.vertexCount];
        Vector3[] tangentFrom = new Vector3[from.vertexCount];

        for (int i = 0; i < from.blendShapeCount; i++)
        {
            string name = from.GetBlendShapeName(i);
            int frameCount = from.GetBlendShapeFrameCount(i);

            for (int j = 0; j < frameCount; j++)
            {
                float weight = from.GetBlendShapeFrameWeight(i, j);
                from.GetBlendShapeFrameVertices(i, j, vertexFrom, normalFrom, tangentFrom);

                MeshVector3[] vertexTo = new MeshVector3[toMeshVertexCount];
                MeshVector3[] normalTo = new MeshVector3[toMeshVertexCount];
                MeshVector3[] tangentTo = new MeshVector3[toMeshVertexCount];
                for (int k = 0; k < toMeshVertexCount; k++)
                {
                    int originIndex = dicNewIndexToOriginIndex[k];
                    vertexTo[k] = new MeshVector3(vertexFrom[originIndex]);
                    normalTo[k] = new MeshVector3(normalFrom[originIndex]);
                    tangentTo[k] = new MeshVector3(tangentFrom[originIndex]);
                }

                BlendShapeFrame frame = new()
                {
                    m_Name = name,
                    m_Weight = weight,
                    m_DeltaVertex = vertexTo,
                    m_DeltaNormal = normalTo,
                    m_DeltaTangent = tangentTo,
                };
                data.m_Frames.Add(frame);
            }
        }
    }

    [MenuItem("美术/变形数据")]
    static void CreateWnd()
    {
        CreateWindow<SetBlendShapeWnd>();
    }

   // [ShowInInspector]
    private void LoadBlendShapeData()
    {
        Mesh mesh = m_SourceBlendShapeSMR.sharedMesh;
        Material[] materials = m_SourceBlendShapeSMR.sharedMaterials;

        m_ListBlendShapeData = new();
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            var data = GetBlendShapeData(mesh, i);
            data.m_SubMeshName = materials[i].name;
            m_ListBlendShapeData.Add(data);
        }

        m_SubMeshGUIData = new SubMeshGUIData[m_ListBlendShapeData.Count];
        for (int i = 0; i < m_SubMeshGUIData.Length; i++)
        {
            var data = m_ListBlendShapeData[i];
            m_SubMeshGUIData[i] = new()
            {
                m_SubmeshIndex = i,
                m_SubmeshName = data.m_SubMeshName,
            };
        }
    }

//    [ShowInInspector]
    void LoadFrameNames()
    {
        var data = m_ListBlendShapeData[0];
        m_FrameNames = new string[data.m_Frames.Count];
        for (int j = 0; j < data.m_Frames.Count; j++)
        {
            string frameName = data.m_Frames[j].m_Name;
            m_FrameNames[j] = frameName;

            if (m_FrameNameRemoveStrings != null)
            {
                foreach (var removeString in m_FrameNameRemoveStrings)
                {
                    m_FrameNames[j] = m_FrameNames[j].Replace(removeString, "");
                }
            }

            m_FrameNames[j] = $"{j}\t{m_FrameNames[j]}";
        }
    }

   // [ShowInInspector]
    void SetMeshBlendShapes()
    {
        foreach (var guiData in m_SubMeshGUIData)
        {
            if (guiData.m_TarMesh)
                SetMeshBlendShape(guiData);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("SetMeshBlendShape done!");
    }


    void SetMeshBlendShape(SubMeshGUIData guiData)
    {
        BlendShapeData shapeData = m_ListBlendShapeData[guiData.m_SubmeshIndex];

        guiData.m_TarMesh.ClearBlendShapes();

        foreach (var frameName in m_FrameNames)
        {
            string[] words = frameName.Split('\t');
            int frameIndex = int.Parse(words[0]);
            string newFrameName = words[1];
            var frameData = shapeData.m_Frames[frameIndex];

            Vector3[] deltaVertex = BlendShapeFrame.ToVertexArray(frameData.m_DeltaVertex);
            Vector3[] deltaNormal = BlendShapeFrame.ToVertexArray(frameData.m_DeltaNormal);
            Vector3[] deltaTangent = BlendShapeFrame.ToVertexArray(frameData.m_DeltaTangent);
            guiData.m_TarMesh.AddBlendShapeFrame(newFrameName, frameData.m_Weight, deltaVertex, deltaNormal, deltaTangent);
        }

        EditorUtility.SetDirty(guiData.m_TarMesh);
    }
}