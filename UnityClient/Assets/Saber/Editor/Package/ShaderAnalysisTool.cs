using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

public static class ShaderAnalysisTool
{
    const int k_MinInterestingVariantCount = 20;


    [MenuItem("Saber/Asset/Shader/Shader的变体数量统计")]
    static void CalcAllShadersVariants()
    {
        System.DateTime now = System.DateTime.Now;
        string strDate = $"{now.Year}{now.Month:d2}{now.Day:d2}";
        string savePath = EditorUtility.SaveFilePanel("存储分析报告", "E:/", $"Shader的变体数量统计_{strDate}", "txt");
        if (string.IsNullOrEmpty(savePath))
            return;

        var unityEditor = Assembly.LoadFile(EditorApplication.applicationContentsPath + "/Managed/UnityEditor.dll");
        var shaderUtilType = unityEditor.GetType("UnityEditor.ShaderUtil");
        List<string> files = GetSelectedAssets(".shader");
        var shaderDic = new Dictionary<Object, int>();
        int progress = files.Count;
        int current = 0;
        foreach (var path in files)
        {
            current++;
            bool isCancel =
                EditorUtility.DisplayCancelableProgressBar("处理Shader中", path, (float)current / (float)progress);
            if (isCancel)
            {
                EditorUtility.ClearProgressBar();
                break;
            }

            string resPath = FileUtil.GetProjectRelativePath(path);
            var shaderAsset = AssetDatabase.LoadAssetAtPath<Shader>(resPath);
            if (shaderAsset != null)
            {
                MethodInfo setSearchType = shaderUtilType.GetMethod("GetVariantCount",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                object[] parameters = new System.Object[] { shaderAsset, true };
                var comboCount = setSearchType.Invoke(null, parameters);
                int varientCount = int.Parse(comboCount.ToString());
                shaderDic.Add(shaderAsset, varientCount);
            }
        }

        EditorUtility.ClearProgressBar();
        var sortArray = (from objDic in shaderDic
            orderby objDic.Value descending
            select objDic).ToDictionary(pair => pair.Key, pair => pair.Value);

        StringBuilder sbLog = new();
        int count = 0;
        foreach (var kvp in sortArray)
        {
            Shader mshader = kvp.Key as Shader;
            string assetPath = AssetDatabase.GetAssetPath(kvp.Key);
            int varientCount = kvp.Value;
            if (varientCount >= k_MinInterestingVariantCount)
            {
                sbLog.AppendLine($"{assetPath}:\t\t\t{varientCount}");
                Debug.Log(string.Format(assetPath + ": {0}", varientCount), mshader);
                ++count;
            }
        }

        string strResult = $"变体大于{k_MinInterestingVariantCount}的 shader 共有 {count} 个。";
        sbLog.AppendLine();
        sbLog.AppendLine(strResult);
        File.WriteAllText(savePath, sbLog.ToString());

        Debug.Log(strResult);
    }

    public static List<string> GetSelectedAssets(string tarExt)
    {
        List<string> list = new();
        foreach (var obj in Selection.objects)
        {
            string a = AssetDatabase.GetAssetPath(obj);
            bool isFolder = obj is DefaultAsset && Directory.Exists(a);

            if (isFolder)
            {
                string[] files = Directory.GetFiles(a, $"*{tarExt}", SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    string p = GetAssetPath(f);
                    if (!list.Contains(p))
                        list.Add(p);
                }
            }
            else
            {
                string ext = Path.GetExtension(a);
                if (ext == tarExt)
                {
                    if (!list.Contains(a))
                        list.Add(a);
                }
            }
        }

        return list;
    }

    // full path 转 asset path
    public static string GetAssetPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
            return "";

        fullPath = fullPath.Replace("\\", "/");
        return fullPath.StartsWith("Assets/")
            ? fullPath
            : "Assets" + fullPath.Substring(Application.dataPath.Length);
    }

    [MenuItem("Saber/Asset/Shader/找出使用内置URP内置shader的材质")]
    static void FindMat_UseBuildinURPShader()
    {
        List<string> matPaths = GetSelectedAssets(".mat");
        List<string> shaderNames = new();
        int count = 0;
        foreach (var matPath in matPaths)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) continue;
            if (mat.shader.name.StartsWith("Universal Render Pipeline/"))
            {
                Debug.Log(matPath, mat);

                if (!shaderNames.Contains(mat.shader.name))
                {
                    shaderNames.Add(mat.shader.name);
                }

                ++count;
            }
        }

        Debug.Log(count);
        Debug.Log("shader:");
        foreach (var shaderName in shaderNames)
        {
            Debug.Log(shaderName);
        }
    }

    [MenuItem("Saber/Asset/Shader/替换材质的URP内置shader")]
    static void OptimizeMat_ReplaceBuildinURPShader()
    {
        List<string> matPaths = GetSelectedAssets(".mat");
        int count = 0;
        foreach (var matPath in matPaths)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat.shader.name.StartsWith("Universal Render Pipeline/"))
            {
                Debug.Log(matPath, mat);
                Shader tarShader = Shader.Find($"Saber/{mat.shader.name}");
                if (tarShader != null)
                {
                    mat.shader = tarShader;
                    ++count;
                }
                else
                {
                    Debug.LogError("Not found shader:" + mat.shader.name);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(count);
    }
}