using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Windows;

public class SubmeshSplitterForSaber
{
    //http://answers.unity3d.com/questions/1213025/separating-submeshes-into-unique-meshes.html
    [MenuItem("美术/Submesh Splitter for saber", false, 7002)]
    public static void BuildWindowsAssetBundle()
    {
        GameObject[] objects = Selection.gameObjects;
        for (int i = 0; i < objects.Length; i++)
        {
            ProcessGameObject(objects[i]);
        }

        UnityEngine.Debug.Log("Done splitting meshes into submeshes!  " + System.DateTime.Now);
    }

    private static void ProcessGameObject(GameObject go)
    {
        SkinnedMeshRenderer meshRendererComponent = go.GetComponent<SkinnedMeshRenderer>();
        if (!meshRendererComponent)
        {
            UnityEngine.Debug.LogError("MeshRenderer null for '" + go.name + "'!");
            return;
        }

        Mesh mesh = go.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        if (!mesh)
        {
            UnityEngine.Debug.LogError("Mesh null for '" + go.name + "'!");
            return;
        }

        List<SubmeshSplitter.MeshFromSubmesh> meshFromSubmeshes = GetAllSubMeshAsIsolatedMeshes(mesh);
        if (meshFromSubmeshes == null || meshFromSubmeshes.Count == 0)
        {
            UnityEngine.Debug.LogError("List<MeshFromSubmesh> empty or null for '" + go.name + "'!");
            return;
        }

        // string goName = go.name;
        var rootPath = AssetDatabase.GetAssetPath(mesh).Replace('.', '_') + "_SubMesh";
        if (!Directory.Exists(rootPath))
        {
            Directory.CreateDirectory(rootPath);
            AssetDatabase.Refresh();
        }

        for (int i = 0; i < meshFromSubmeshes.Count; i++)
        {
            //string meshFromSubmeshName = goName + "_sub_" + i;
            string meshFromSubmeshName = meshRendererComponent.sharedMaterials[i].name;
            GameObject meshFromSubmeshGameObject = new GameObject();
            meshFromSubmeshGameObject.name = meshFromSubmeshName;
            meshFromSubmeshGameObject.transform.SetParent(meshRendererComponent.transform);
            meshFromSubmeshGameObject.transform.localPosition = Vector3.zero;
            meshFromSubmeshGameObject.transform.localRotation = Quaternion.identity;
            SkinnedMeshRenderer meshFromSubmeshMeshRendererComponent = meshFromSubmeshGameObject.AddComponent<SkinnedMeshRenderer>();
            meshFromSubmeshMeshRendererComponent.sharedMesh = meshFromSubmeshes[i].mesh;
            // Don't forget to save the newly created mesh in the asset database (on disk)
            string path = $"{rootPath}/{meshFromSubmeshName}.asset"; //mehs_path + meshFromSubmeshName + ".asset";
            AssetDatabase.CreateAsset(meshFromSubmeshes[i].mesh, path);
            UnityEngine.Debug.Log("Created: " + path);
            // To use the same mesh renderer properties of the initial mesh
            EditorUtility.CopySerialized(meshRendererComponent, meshFromSubmeshMeshRendererComponent);
            // We just need the only one material used by the sub mesh in its renderer
            Material material = meshFromSubmeshMeshRendererComponent.sharedMaterials[meshFromSubmeshes[i].id];
            meshFromSubmeshMeshRendererComponent.sharedMaterials = new[] { material };
            var newMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            meshFromSubmeshMeshRendererComponent.sharedMesh = newMesh;
        }
    }

    private static List<SubmeshSplitter.MeshFromSubmesh> GetAllSubMeshAsIsolatedMeshes(Mesh mesh)
    {
        List<SubmeshSplitter.MeshFromSubmesh> meshesToReturn = new List<SubmeshSplitter.MeshFromSubmesh>();
        if (!mesh)
        {
            UnityEngine.Debug.LogError("No mesh passed into GetAllSubMeshAsIsolatedMeshes!");
            return meshesToReturn;
        }

        int submeshCount = mesh.subMeshCount;
        if (submeshCount < 2)
        {
            UnityEngine.Debug.LogError("Only " + submeshCount + " submeshes in mesh passed to GetAllSubMeshAsIsolatedMeshes");
            return meshesToReturn;
        }

        SubmeshSplitter.MeshFromSubmesh m1;
        for (int i = 0; i < submeshCount; i++)
        {
            m1 = new SubmeshSplitter.MeshFromSubmesh();
            m1.id = i;
            m1.mesh = mesh.GetSubmesh(i);
            meshesToReturn.Add(m1);
        }

        return meshesToReturn;
    }
}