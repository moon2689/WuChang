using System.Collections.Generic;
using System.Text;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using System;
using System.IO;

public class ShaderPrePost : IPreprocessShaders
{
    public int callbackOrder => 0;

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        List<List<string>> shaderSV = new();
        foreach (var sv in data)
        {
            var keywords = sv.shaderKeywordSet.GetShaderKeywords();
            List<string> list = new();
            foreach (var k in keywords)
                list.Add(k.name);
            shaderSV.Add(list);
        }

        string shaderName = shader.name;

        if (shaderName == "Saber/Universal Render Pipeline/Lit")
        {
            for (int i = data.Count - 1; i >= 0; --i)
            {
                bool validSV =
                    IsSVContainsAllKeyword(shaderSV[i],
                        "_MAIN_LIGHT_SHADOWS",
                        "_ADDITIONAL_LIGHTS")
                    &&
                    !IsSVContainsAnyKeyword(shaderSV[i],
                        "_ADDITIONAL_LIGHT_SHADOWS",
                        "_PARALLAXMAP",
                        "LOD_FADE_CROSSFADE",
                        "_DETAIL_MULX2",
                        "_LIGHT_COOKIES");

                if (!validSV)
                    data.RemoveAt(i);
            }
        }
        else if (shaderName == "Saber/Universal Render Pipeline/Particles/Lit")
        {
            for (int i = data.Count - 1; i >= 0; --i)
            {
                bool validSV =
                    IsSVContainsAllKeyword(shaderSV[i],
                        "_MAIN_LIGHT_SHADOWS",
                        "_ADDITIONAL_LIGHTS")
                    &&
                    !IsSVContainsAnyKeyword(shaderSV[i],
                        "_ADDITIONAL_LIGHT_SHADOWS",
                        "_PARALLAXMAP",
                        "LOD_FADE_CROSSFADE",
                        "_DETAIL_MULX2",
                        "_LIGHT_COOKIES");

                if (!validSV)
                    data.RemoveAt(i);
            }
        }
        else if (shaderName == "Saber/Universal Render Pipeline/Simple Lit")
        {
            for (int i = data.Count - 1; i >= 0; --i)
            {
                bool validSV =
                    IsSVContainsAllKeyword(shaderSV[i],
                        "_MAIN_LIGHT_SHADOWS",
                        "_ADDITIONAL_LIGHTS")
                    &&
                    !IsSVContainsAnyKeyword(shaderSV[i],
                        "_ADDITIONAL_LIGHT_SHADOWS",
                        "_LIGHT_COOKIES");

                if (!validSV)
                    data.RemoveAt(i);
            }
        }
        else if (shaderName == "Saber/Human/Skin Lit")
        {
            for (int i = data.Count - 1; i >= 0; --i)
            {
                bool validSV =
                    IsSVContainsAllKeyword(shaderSV[i],
                        "_MAIN_LIGHT_SHADOWS",
                        "_SSS_ON",
                        "_PCFSHADOW_ON",
                        "_DETAILNORMAL_ON")
                    &&
                    !IsSVContainsAnyKeyword(shaderSV[i],
                        "_ADDITIONAL_LIGHT_SHADOWS",
                        "_LIGHT_COOKIES");

                if (!validSV)
                    data.RemoveAt(i);
            }
        }
        else if (shaderName == "Saber/Human/Hair Lit")
        {
            for (int i = data.Count - 1; i >= 0; --i)
            {
                bool validSV =
                    IsSVContainsAllKeyword(shaderSV[i],
                        "_MAIN_LIGHT_SHADOWS",
                        "_ADDITIONAL_LIGHTS")
                    &&
                    !IsSVContainsAnyKeyword(shaderSV[i],
                        "_ADDITIONAL_LIGHT_SHADOWS",
                        "_LIGHT_COOKIES");

                if (!validSV)
                    data.RemoveAt(i);
            }
        }
        else if (shaderName == "Saber/PBR/Monster Skin")
        {
            for (int i = data.Count - 1; i >= 0; --i)
            {
                bool validSV =
                    IsSVContainsAllKeyword(shaderSV[i],
                        "_MAIN_LIGHT_SHADOWS",
                        "_ADDITIONAL_LIGHTS")
                    &&
                    !IsSVContainsAnyKeyword(shaderSV[i],
                        "_ADDITIONAL_LIGHT_SHADOWS",
                        "_LIGHT_COOKIES");

                if (!validSV)
                    data.RemoveAt(i);
            }
        }
        else if (shaderName == "Saber/Human/Cloth Metal Lit Transparent" || shaderName == "Saber/Human/Cloth Metal Lit")
        {
            for (int i = data.Count - 1; i >= 0; --i)
            {
                bool validSV =
                    IsSVContainsAllKeyword(shaderSV[i],
                        "_MAIN_LIGHT_SHADOWS",
                        "_ADDITIONAL_LIGHTS")
                    &&
                    !IsSVContainsAnyKeyword(shaderSV[i],
                        "_ADDITIONAL_LIGHT_SHADOWS",
                        "_LIGHT_COOKIES");

                if (!validSV)
                    data.RemoveAt(i);
            }
        }
        else if (shaderName == "Saber/PBR/Monster Common Lit")
        {
            for (int i = data.Count - 1; i >= 0; --i)
            {
                bool validSV =
                    IsSVContainsAllKeyword(shaderSV[i],
                        "_MAIN_LIGHT_SHADOWS",
                        "_ADDITIONAL_LIGHTS")
                    &&
                    !IsSVContainsAnyKeyword(shaderSV[i],
                        "_ADDITIONAL_LIGHT_SHADOWS",
                        "_LIGHT_COOKIES");

                if (!validSV)
                    data.RemoveAt(i);
            }
        }
        else if (shaderName == "Saber/Human/Eye Irises Lit")
        {
            for (int i = data.Count - 1; i >= 0; --i)
            {
                bool validSV =
                    IsSVContainsAllKeyword(shaderSV[i],
                        "_MAIN_LIGHT_SHADOWS",
                        "_ADDITIONAL_LIGHTS")
                    &&
                    !IsSVContainsAnyKeyword(shaderSV[i],
                        "_ADDITIONAL_LIGHT_SHADOWS",
                        "_LIGHT_COOKIES");

                if (!validSV)
                    data.RemoveAt(i);
            }
        }
        else if (shaderName == "Saber/Human/Cloth Fur Lit")
        {
            for (int i = data.Count - 1; i >= 0; --i)
            {
                bool validSV =
                    IsSVContainsAllKeyword(shaderSV[i],
                        "_MAIN_LIGHT_SHADOWS",
                        "_ADDITIONAL_LIGHTS")
                    &&
                    !IsSVContainsAnyKeyword(shaderSV[i],
                        "_ADDITIONAL_LIGHT_SHADOWS",
                        "_LIGHT_COOKIES");

                if (!validSV)
                    data.RemoveAt(i);
            }
        }
        else if (shaderName == "Saber/Human/Eyebrow Lit")
        {
            for (int i = data.Count - 1; i >= 0; --i)
            {
                bool validSV =
                    IsSVContainsAllKeyword(shaderSV[i],
                        "_MAIN_LIGHT_SHADOWS",
                        "_ADDITIONAL_LIGHTS")
                    &&
                    !IsSVContainsAnyKeyword(shaderSV[i],
                        "_ADDITIONAL_LIGHT_SHADOWS",
                        "_LIGHT_COOKIES");

                if (!validSV)
                    data.RemoveAt(i);
            }
        }


        if (data.Count > 100)
        {
            LogShader(shaderName, data);
        }
    }

    static bool IsSVContainsAllKeyword(List<string> svData, params string[] keywords)
    {
        if (svData.Count == 0)
            return false;

        foreach (var keyword in keywords)
        {
#if !UNITY_ANDROID
            if (keyword == "_SHADOWS_SOFT") 
                continue;
#endif
            if (!svData.Contains(keyword))
                return false;
        }

        return true;
    }

    static bool IsSVContainsAnyKeyword(List<string> svData, params string[] keywords)
    {
        foreach (var k in keywords)
        {
            if (svData.Contains(k))
                return true;
        }

        return false;
    }

    void LogShader(string shaderName, IList<ShaderCompilerData> data)
    {
        StringBuilder sb = new();

        sb.AppendLine($"shader:{shaderName}");
        int vCount = 0;
        foreach (var item in data)
        {
            ++vCount;
            var svKeywords = item.shaderKeywordSet.GetShaderKeywords();
            sb.AppendLine($"{vCount}");
            foreach (var keyword in svKeywords)
            {
                sb.AppendLine($"\t{keyword.name}");
            }
        }

        string shaderName2 = Path.GetFileNameWithoutExtension(shaderName);
        string dir = $"Assets/_Debug/ShaderPrePost";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        string path =
            $"{dir}/log_{shaderName2}_{DateTime.Now.Month}_{DateTime.Now.Day}_{DateTime.Now.Hour}_{DateTime.Now.Minute}.txt";
        File.WriteAllText(path, sb.ToString());
    }
}