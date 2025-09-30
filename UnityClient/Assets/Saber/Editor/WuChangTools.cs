using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CombatEditor;
using RootMotion.FinalIK;
using Saber.CharacterController;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class WuChangTools
{
    private const string k_UEProjectFolder = @"E:/1/WuChangUnity/Exports";
    private const string k_WuChCommonShader = "Saber/WuChang/Common Lit";

    static MD5 s_md5;

    static MD5 MD5Obj => s_md5 ??= MD5.Create();

    [MenuItem("Saber/WUCH/Add Hurt Box")]
    static void AddHurtBoxes()
    {
        GameObject go = Selection.activeObject as GameObject;
        if (!go)
        {
            return;
        }

        HurtBox[] hurtBoxes = go.GetComponentsInChildren<HurtBox>();
        foreach (var oldB in hurtBoxes)
        {
            GameObject.DestroyImmediate(oldB.gameObject);
        }

        Transform root = go.transform.GetChild(0).GetChild(0);
        string[] hurtBoxBones = new string[]
        {
            "Root_M",
            "Chest_M",
            "Head_M",

            "Hip_L",
            "Knee_L",
            "Hip_R",
            "Knee_R",

            "Shoulder_L",
            "Elbow_L",
            "Shoulder_R",
            "Elbow_R",
        };

        string[] otherBoneNames = new string[]
        {
            "HeadEnd_M",
            "Neck_M",
            "Ankle_L",
            "Ankle_R",
            "Wrist_L",
            "Wrist_R",
        };


        Transform[] allBones = root.GetComponentsInChildren<Transform>();
        Dictionary<string, Transform> dicAllBones = new();
        foreach (var t in allBones)
        {
            if (hurtBoxBones.Any(a => a == t.name) || otherBoneNames.Any(a => a == t.name))
            {
                dicAllBones[t.name] = t;
            }
        }

        HitReaction hitReaction = go.GetComponentInChildren<HitReaction>();
        foreach (var t in allBones)
        {
            if (hurtBoxBones.Any(a => a == t.name))
            {
                SetHurtBoxCollider(t, dicAllBones, hitReaction);
            }
        }
    }

    static void SetHitReaction(HitReaction hitReaction, CapsuleCollider collider, string name)
    {
        foreach (var item in hitReaction.effectorHitPoints)
        {
            if (item.name == name)
            {
                item.collider = collider;
                break;
            }
        }

        foreach (var item in hitReaction.boneHitPoints)
        {
            if (item.name == name)
            {
                item.collider = collider;
                break;
            }
        }
    }

    static void SetHurtBoxCollider(Transform bone, Dictionary<string, Transform> dicAllBones, HitReaction hitReaction)
    {
        GameObject hurtBoxObj = new GameObject();
        CapsuleCollider c = hurtBoxObj.AddComponent<CapsuleCollider>();
        c.direction = 0;

        HurtBox hurtBox = hurtBoxObj.AddComponent<HurtBox>();
        hurtBox.transform.SetParent(bone);
        hurtBox.transform.localPosition = Vector3.zero;
        hurtBox.transform.localRotation = Quaternion.identity;
        hurtBox.transform.localScale = Vector3.one;
        hurtBox.name = $"HurtBox_{bone.name}";
        Debug.Log($"Add box {hurtBox.name}", hurtBox);

        string boneName = bone.name;

        if (boneName == "Root_M")
        {
            Transform boneEnd = dicAllBones["Chest_M"];
            Vector3 disV3 = bone.transform.position - boneEnd.position;
            float dis = disV3.magnitude;
            c.radius = dis * 0.7f;
            c.height = c.radius * 2;
            c.center = new Vector3(0, 0, 0);

            SetHitReaction(hitReaction, c, "Hips");
        }
        else if (boneName == "Chest_M")
        {
            Transform boneEnd = dicAllBones["Neck_M"];
            Vector3 disV3 = bone.transform.position - boneEnd.position;
            float dis = disV3.magnitude;
            c.radius = dis * 0.8f;
            c.height = c.radius * 2;
            c.center = new Vector3(0, 0, 0);

            SetHitReaction(hitReaction, c, "Chest");
        }
        else if (boneName == "Head_M")
        {
            Transform boneEnd = dicAllBones["HeadEnd_M"];
            Vector3 disV3 = bone.transform.position - boneEnd.position;
            float dis = disV3.magnitude;
            c.radius = dis;
            c.height = dis * 2;
            c.center = new Vector3(0, 0, 0);

            SetHitReaction(hitReaction, c, "Head");
        }
        else if (boneName == "Hip_L")
        {
            Transform boneEnd = dicAllBones["Knee_L"];
            Vector3 disV3 = bone.transform.position - boneEnd.position;
            float dis = disV3.magnitude;
            c.radius = dis * 0.2f;
            c.height = dis;
            c.center = new Vector3(c.height / 2f, 0, 0);

            SetHitReaction(hitReaction, c, "L Thigh");
        }
        else if (boneName == "Knee_L")
        {
            Transform boneEnd = dicAllBones["Ankle_L"];
            Vector3 disV3 = bone.transform.position - boneEnd.position;
            float dis = disV3.magnitude;
            c.radius = dis * 0.15f;
            c.height = dis;
            c.center = new Vector3(c.height / 2f, 0, 0);

            SetHitReaction(hitReaction, c, "L Calf");
        }
        else if (boneName == "Hip_R")
        {
            Transform boneEnd = dicAllBones["Knee_R"];
            Vector3 disV3 = bone.transform.position - boneEnd.position;
            float dis = disV3.magnitude;
            c.radius = dis * 0.2f;
            c.height = dis;
            c.center = new Vector3(-c.height / 2f, 0, 0);

            SetHitReaction(hitReaction, c, "R Thigh");
        }
        else if (boneName == "Knee_R")
        {
            Transform boneEnd = dicAllBones["Ankle_R"];
            Vector3 disV3 = bone.transform.position - boneEnd.position;
            float dis = disV3.magnitude;
            c.radius = dis * 0.15f;
            c.height = dis;
            c.center = new Vector3(-c.height / 2f, 0, 0);

            SetHitReaction(hitReaction, c, "R Calf");
        }
        else if (boneName == "Shoulder_L")
        {
            Transform boneEnd = dicAllBones["Elbow_L"];
            Vector3 disV3 = bone.transform.position - boneEnd.position;
            float dis = disV3.magnitude;
            c.radius = dis * 0.25f;
            c.height = dis;
            c.center = new Vector3(c.height / 2f, 0, 0);

            SetHitReaction(hitReaction, c, "L Upper Arm");
        }
        else if (boneName == "Elbow_L")
        {
            Transform boneEnd = dicAllBones["Wrist_L"];
            Vector3 disV3 = bone.transform.position - boneEnd.position;
            float dis = disV3.magnitude;
            c.radius = dis * 0.25f;
            c.height = dis * 1.2f;
            c.center = new Vector3(c.height / 2f, 0, 0);

            SetHitReaction(hitReaction, c, "L Forearm");
        }
        else if (boneName == "Shoulder_R")
        {
            Transform boneEnd = dicAllBones["Elbow_R"];
            Vector3 disV3 = bone.transform.position - boneEnd.position;
            float dis = disV3.magnitude;
            c.radius = dis * 0.25f;
            c.height = dis;
            c.center = new Vector3(-c.height / 2f, 0, 0);

            SetHitReaction(hitReaction, c, "R Upper Arm");
        }
        else if (boneName == "Elbow_R")
        {
            Transform boneEnd = dicAllBones["Wrist_R"];
            Vector3 disV3 = bone.transform.position - boneEnd.position;
            float dis = disV3.magnitude;
            c.radius = dis * 0.25f;
            c.height = dis * 1.2f;
            c.center = new Vector3(-c.height / 2f, 0, 0);

            SetHitReaction(hitReaction, c, "R Forearm");
        }
    }

    [MenuItem("Saber/WUCH/Delete repeat wav")]
    static void DeleteRepeatWav()
    {
        DefaultAsset defaultAsset = Selection.activeObject as DefaultAsset;
        if (defaultAsset == null)
        {
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(defaultAsset);
        string[] wavFiles = Directory.GetFiles(folderPath, "*.wav");
        Dictionary<string, List<string>> repeatFiles = new();
        foreach (var wavFile in wavFiles)
        {
            byte[] fileBuffer = File.ReadAllBytes(wavFile);
            string md5Str = ComputeHash(fileBuffer);
            repeatFiles.TryGetValue(md5Str, out var list);
            if (list == null)
            {
                list = new();
                repeatFiles[md5Str] = list;
            }

            list.Add(wavFile);
        }

        foreach (var pair in repeatFiles)
        {
            if (pair.Value.Count > 1)
            {
                for (int i = 1; i < pair.Value.Count; i++)
                {
                    File.Delete(pair.Value[i]);
                    Debug.Log("Delete wav:" + pair.Value[i]);
                }
            }
        }

        Debug.Log("DeleteRepeatWav done");
    }


    public static string ComputeHash(byte[] buffer)
    {
        if (buffer == null || buffer.Length < 1)
            return "";

        byte[] hash = MD5Obj.ComputeHash(buffer);
        StringBuilder sb = new StringBuilder();

        foreach (var b in hash)
            sb.Append(b.ToString("x2"));

        return sb.ToString();
    }


    [MenuItem("Saber/WUCH/Fix Material")]
    static void FixWuChangMaterials()
    {
        foreach (TextAsset json in Selection.objects)
        {
            FixWuChMaterial(json);
        }

        Debug.Log("FixWuChangMaterials all done");
    }

    static bool GetUENormalMaskPathByAlbedoPath(string albedoPath, out string normalPath, out string maskPath)
    {
        normalPath = null;
        maskPath = null;
        if (albedoPath.Contains("_D.tga"))
        {
            normalPath = albedoPath.Replace("_D.tga", "_N.tga");
            maskPath = albedoPath.Replace("_D.tga", "_R.tga");
        }
        else if (albedoPath.Contains("_C.tga"))
        {
            normalPath = albedoPath.Replace("_C.tga", "_N.tga");
            maskPath = albedoPath.Replace("_C.tga", "_R.tga");
        }
        else if (albedoPath.Contains("_Albedo.tga"))
        {
            normalPath = albedoPath.Replace("_Albedo.tga", "_Normal.tga");
            maskPath = albedoPath.Replace("_Albedo.tga", "_Reflection.tga");
        }
        else if (albedoPath.Contains("_BaseColor_2K.tga"))
        {
            normalPath = albedoPath.Replace("_BaseColor_2K.tga", "_Normal_2K.tga");
            maskPath = albedoPath.Replace("_BaseColor_2K.tga", "_Reflective_2K.tga");
        }
        else
        {
            Debug.LogError("Unknown tex path:" + albedoPath);
            return false;
        }

        return true;
    }

    static void FixWuChMaterial(TextAsset jsonFile)
    {
        string path = AssetDatabase.GetAssetPath(jsonFile);
        string fileName = Path.GetFileNameWithoutExtension(path);
        string dirName = Path.GetDirectoryName(Path.GetDirectoryName(path));
        string matPath = Path.Combine(dirName + "/Materials", fileName + ".mat");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        string[] lines = jsonFile.text.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
        string texOldPath = null;
        foreach (var line in lines)
        {
            string[] words = line.Split(new string[] { ":", "\"", ",", " " }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 1 && words[0] == "PM_Diffuse")
            {
                texOldPath = words[1];
                break;
            }
        }

        if (string.IsNullOrEmpty(texOldPath))
        {
            Debug.LogError("diffuse is null:" + fileName);
            return;
        }

        string folderName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(texOldPath));
        string texName = Path.GetFileNameWithoutExtension(texOldPath);

        string texPath = $"{dirName}/{folderName}/{texName}.tga";
        if (!GetUENormalMaskPathByAlbedoPath(texPath, out string normalPath, out string maskPath))
            return;

        TextureImporter tiDiffuse = AssetImporter.GetAtPath(texPath) as TextureImporter;
        tiDiffuse.sRGBTexture = true;
        CompressTextureHalfSize(tiDiffuse);
        tiDiffuse.SaveAndReimport();

        Texture2D diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

        if (diffuse == null)
        {
            Debug.LogError($"diffuse is null:{fileName},texPath:{texPath}");
            return;
        }

        // normal
        TextureImporter tiNormal = AssetImporter.GetAtPath(normalPath) as TextureImporter;
        tiNormal.textureType = TextureImporterType.NormalMap;
        CompressTextureHalfSize(tiNormal);
        tiNormal.SaveAndReimport();
        Texture2D texNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);

        // mask
        TextureImporter tiMask = AssetImporter.GetAtPath(maskPath) as TextureImporter;
        tiMask.sRGBTexture = false;
        CompressTextureHalfSize(tiMask);
        tiMask.SaveAndReimport();
        Texture2D texMask = AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);

        mat.shader = Shader.Find(k_WuChCommonShader);
        mat.SetTexture("_BaseMap", diffuse);
        mat.SetColor("_BaseColor", Color.white);
        mat.SetTexture("_BumpMap", texNormal);
        mat.SetTexture("_MaskMROMap", texMask);

        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("FixWuChMaterial done:" + mat.name, mat);
    }


    [MenuItem("Saber/WUCH/Fix FBX Anim Name")]
    static void FixMonsterAnimName()
    {
        foreach (var selectedObj in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedObj);
            ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (modelImporter == null)
            {
                continue;
            }

            /*
            if (modelImporter.defaultClipAnimations.Length > 1)
            {
                Debug.LogError("modelImporter.defaultClipAnimations.Length>1,asset path:" + assetPath, modelImporter);
                continue;
            }

            var tempClip = modelImporter.defaultClipAnimations[0];
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            tempClip.name = fileName;
            modelImporter.clipAnimations = new[] { tempClip };
            */
            var newAnims = new ModelImporterClipAnimation[modelImporter.defaultClipAnimations.Length];
            for (int i = 0; i < modelImporter.defaultClipAnimations.Length; i++)
            {
                var oldClip = modelImporter.defaultClipAnimations[i];
                string[] words = oldClip.name.Split('|');
                string newName = words.LastOrDefault();

                Debug.Log($"{oldClip.name} -> {newName}");

                string l = newName.ToLower();
                bool isLoop = l.Contains("idle") || l.Contains("walk") || l.Contains("run");
                oldClip.name = newName;
                oldClip.loop = isLoop;
                newAnims[i] = oldClip;
            }

            modelImporter.clipAnimations = newAnims;
            modelImporter.SaveAndReimport();

            Debug.Log($"Fixed {selectedObj.name}", selectedObj);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("All done");
    }

    public static List<string> GetSelectedAssets<T>(string searchPattern) where T : UnityEngine.Object
    {
        List<string> list = new();
        foreach (var obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (obj is DefaultAsset)
            {
                string[] files = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    if (!list.Contains(f))
                        list.Add(f);
                }
            }
            else if (obj is T)
            {
                if (!list.Contains(path))
                    list.Add(path);
            }
        }

        return list;
    }

    [MenuItem("Saber/WUCH/Print UE Material Content")]
    static void PrintUEMaterialContent()
    {
        List<string> listMaterial = GetSelectedAssets<Material>("*.mat");

        string[] allJsonFiles = Directory.GetFiles(k_UEProjectFolder + "/Project_Plague/Content", "*.json",
            SearchOption.AllDirectories);
        Dictionary<string, string> dicAllJsonFiles = new();
        foreach (var jsonFile in allJsonFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(jsonFile);
            dicAllJsonFiles[fileName] = jsonFile;
        }

        foreach (var m in listMaterial)
        {
            string fileName = Path.GetFileNameWithoutExtension(m);
            string jsonFile = dicAllJsonFiles[fileName];
            Debug.Log($"Json: {fileName} {jsonFile},Content:");
            Debug.Log($"{File.ReadAllText(jsonFile)}");

            var listFiles = GetAllImagePath(jsonFile);
            if (listFiles.Count < 1)
            {
                continue;
            }

            foreach (var pair in listFiles)
            {
                Debug.Log($"{pair.Key}:{pair.Value}");
            }
        }
    }

    static Dictionary<string, string> GetAllImagePath(string jsonFile)
    {
        string[] lines = File.ReadAllLines(jsonFile);
        bool begin = false;
        Dictionary<string, string> filePaths = new();
        foreach (var line in lines)
        {
            if (begin)
            {
                if (line.Contains("Parameters"))
                {
                    break;
                }

                string[] words = line.Split(new string[] { "\": \"", "." }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 1 && words[1].Length > 0)
                {
                    string key = words[0].Split('\"')[1];
                    string v = $"{k_UEProjectFolder}/{words[1]}.tga";
                    filePaths[key] = v;
                }
            }
            else if (line.Contains("Textures"))
            {
                begin = true;
            }
        }

        return filePaths;
    }

    [MenuItem("Saber/WUCH/Import material all images from ue")]
    static void ImportMaterialAllImagesFromUE()
    {
        List<string> listMaterial = GetSelectedAssets<Material>("*.mat");

        string[] allJsonFiles = Directory.GetFiles(k_UEProjectFolder + "/Project_Plague/Content", "*.json",
            SearchOption.AllDirectories);
        Dictionary<string, string> dicAllJsonFiles = new();
        foreach (var jsonFile in allJsonFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(jsonFile);
            dicAllJsonFiles[fileName] = jsonFile;
        }

        string[] allTgaInProj = Directory.GetFiles("Assets/Saber/Art", "*.tga", SearchOption.AllDirectories);
        Dictionary<string, string> dicAllLocalTga = new();
        foreach (var tgaFile in allTgaInProj)
        {
            string fileName = Path.GetFileNameWithoutExtension(tgaFile);
            dicAllLocalTga[fileName] = tgaFile;
        }

        foreach (var m in listMaterial)
        {
            string fileName = Path.GetFileNameWithoutExtension(m);
            string jsonFile = dicAllJsonFiles[fileName];
            Debug.Log($"Json: {fileName} {jsonFile} {File.ReadAllText(jsonFile)}");

            var listFiles = GetAllImagePath(jsonFile);
            if (listFiles.Count < 1)
            {
                continue;
            }

            string matFolder = Path.GetDirectoryName(m);
            string matParentFolder = Path.GetDirectoryName(matFolder);
            string textureSaveFolder = $"{matParentFolder}/Textures";
            if (!AssetDatabase.IsValidFolder(textureSaveFolder))
            {
                AssetDatabase.CreateFolder(matParentFolder, "Textures");
                AssetDatabase.Refresh();
            }

            foreach (var pair in listFiles)
            {
                ImportOrLoadFromLocal(pair.Value, dicAllLocalTga, textureSaveFolder);
                Debug.Log($"{pair.Key}:{pair.Value}");
            }

            Debug.Log($"Import all images done:{m}");
        }

        Debug.Log("All done");
    }


    [MenuItem("Saber/WUCH/Import material images from ue, and set to material")]
    static void ImportMaterialImagesFromUEAndSetToMatUnForce()
    {
        ImportMaterialImagesFromUEAndSetToMat(false);
    }

    [MenuItem("Saber/WUCH/Import material images from ue, and force set to material")]
    static void ImportMaterialImagesFromUEAndSetToMatForce()
    {
        ImportMaterialImagesFromUEAndSetToMat(true);
    }

    static void ImportMaterialImagesFromUEAndSetToMat(bool force)
    {
        List<string> listMaterial = GetSelectedAssets<Material>("*.mat");

        string[] allJsonFiles = Directory.GetFiles(k_UEProjectFolder + "/Project_Plague/Content", "*.json",
            SearchOption.AllDirectories);
        Dictionary<string, string> dicAllJsonFiles = new();
        foreach (var jsonFile in allJsonFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(jsonFile);
            dicAllJsonFiles[fileName] = jsonFile;
        }

        string[] allTgaInProj = Directory.GetFiles("Assets/Saber/Art", "*.tga", SearchOption.AllDirectories);
        Dictionary<string, string> dicAllLocalTga = new();
        foreach (var tgaFile in allTgaInProj)
        {
            string fileName = Path.GetFileNameWithoutExtension(tgaFile);
            dicAllLocalTga[fileName] = tgaFile;
        }

        foreach (var m in listMaterial)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(m);
            if (!force && mat.shader.name.Contains("WuChang") && mat.shader.name != k_WuChCommonShader)
            {
                continue;
            }

            string fileName = Path.GetFileNameWithoutExtension(m);
            string jsonFile = dicAllJsonFiles[fileName];
            Debug.Log($"Json: {fileName} {jsonFile} {File.ReadAllText(jsonFile)}");
            string diffuseUE = GetUEDiffuseTGAPath(jsonFile, out string normalUE, out string maskUE, out var allFiles);
            if (allFiles.Count < 1)
            {
                continue;
            }

            string matFolder = Path.GetDirectoryName(m);
            string matParentFolder = Path.GetDirectoryName(matFolder);
            string textureSaveFolder = $"{matParentFolder}/Textures";
            if (!AssetDatabase.IsValidFolder(textureSaveFolder))
            {
                AssetDatabase.CreateFolder(matParentFolder, "Textures");
                AssetDatabase.Refresh();
            }

            if (!string.IsNullOrEmpty(diffuseUE))
            {
                mat.shader = Shader.Find(k_WuChCommonShader);
                mat.SetColor("_BaseColor", Color.white);

                string newDiffuse = ImportOrLoadFromLocal(diffuseUE, dicAllLocalTga, textureSaveFolder);
                if (!string.IsNullOrEmpty(newDiffuse))
                {
                    TextureImporter tiDiffuse = AssetImporter.GetAtPath(newDiffuse) as TextureImporter;
                    tiDiffuse.sRGBTexture = true;
                    CompressTextureHalfSize(tiDiffuse);
                    tiDiffuse.SaveAndReimport();

                    Texture2D diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(newDiffuse);
                    mat.SetTexture("_BaseMap", diffuse);
                }

                if (!string.IsNullOrEmpty(normalUE))
                {
                    string newNormal = ImportOrLoadFromLocal(normalUE, dicAllLocalTga, textureSaveFolder);
                    if (!string.IsNullOrEmpty(newNormal))
                    {
                        TextureImporter tiNormal = AssetImporter.GetAtPath(newNormal) as TextureImporter;
                        tiNormal.textureType = TextureImporterType.NormalMap;
                        CompressTextureHalfSize(tiNormal);
                        tiNormal.SaveAndReimport();
                        Texture2D texNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(newNormal);
                        mat.SetTexture("_BumpMap", texNormal);
                    }
                }

                if (!string.IsNullOrEmpty(maskUE))
                {
                    string newMask = ImportOrLoadFromLocal(maskUE, dicAllLocalTga, textureSaveFolder);
                    if (!string.IsNullOrEmpty(newMask))
                    {
                        TextureImporter tiMask = AssetImporter.GetAtPath(newMask) as TextureImporter;
                        tiMask.sRGBTexture = false;
                        CompressTextureHalfSize(tiMask);
                        tiMask.SaveAndReimport();
                        Texture2D texMask = AssetDatabase.LoadAssetAtPath<Texture2D>(newMask);
                        mat.SetTexture("_MaskMROMap", texMask);
                    }
                }
            }

            foreach (var pair in allFiles)
            {
                ImportOrLoadFromLocal(pair.Value, dicAllLocalTga, textureSaveFolder);
                Debug.Log($"{pair.Key}:{pair.Value}");
            }

            Debug.Log($"Fix material done:{m}");
        }

        Debug.Log("All done");
    }

    static string ImportOrLoadFromLocal(string tgaUE, Dictionary<string, string> dicAllLocalTga, string saveFolder)
    {
        string tgaNameNoExt = Path.GetFileNameWithoutExtension(tgaUE);
        if (dicAllLocalTga.TryGetValue(tgaNameNoExt, out string localPath))
        {
            return localPath;
        }
        else
        {
            string tgaName = Path.GetFileName(tgaUE);
            string savePath = saveFolder + "/" + tgaName;
            if (!File.Exists(savePath))
            {
                if (!File.Exists(tgaUE))
                {
                    return null;
                }

                File.Copy(tgaUE, savePath);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }

            return savePath;
        }
    }

    static string GetUEDiffuseTGAPath(string jsonFile, out string normalUE, out string maskUE,
        out Dictionary<string, string> allFiles)
    {
        allFiles = GetAllImagePath(jsonFile);

        normalUE = null;
        maskUE = null;

        allFiles.TryGetValue("PM_Diffuse", out string diffuseUE);
        if (string.IsNullOrEmpty(diffuseUE))
        {
            Debug.LogError("Cann't find ue diffuse from json");
            return null;
        }

        string ext = Path.GetExtension(diffuseUE);
        diffuseUE = diffuseUE.Replace(ext, ".tga");
        //diffuseUE = k_UEProjectFolder + "/" + diffuseUE;

        GetUENormalMaskPathByAlbedoPath(diffuseUE, out normalUE, out maskUE);

        return diffuseUE;
    }

    [MenuItem("Saber/WUCH/Fix image setting")]
    static void FixImageSetting()
    {
        List<string> listImages = GetSelectedAssets<Texture2D>("*.tga");
        foreach (var image in listImages)
        {
            string fileName = Path.GetFileNameWithoutExtension(image);
            if (fileName.Contains("_D"))
            {
            }
            else if (fileName.Contains("_N"))
            {
                TextureImporter tiNormal = AssetImporter.GetAtPath(image) as TextureImporter;
                tiNormal.textureType = TextureImporterType.NormalMap;
                CompressTextureHalfSize(tiNormal);
                tiNormal.SaveAndReimport();
            }
            else if (fileName.Contains("_R"))
            {
                TextureImporter tiMask = AssetImporter.GetAtPath(image) as TextureImporter;
                CompressTextureHalfSize(tiMask);
                tiMask.sRGBTexture = false;
                tiMask.SaveAndReimport();
            }
            else
            {
                Debug.LogError($"Unknown file name:{fileName}");
            }

            Debug.Log("Done:" + image);
        }

        Debug.Log("All done");
    }


    [MenuItem("Saber/WUCH/CompressTextureHalfSize")]
    static void CompressTextureHalfSize()
    {
        bool confirm = EditorUtility.DisplayDialog("提示", "确定压缩图片尺寸为原尺寸的一半？", "确定", "取消");
        if (!confirm)
        {
            return;
        }

        List<string> tgaFiles = GetSelectedAssets<Texture2D>("*.tga");
        foreach (var tgaFile in tgaFiles)
        {
            TextureImporter tiTGA = AssetImporter.GetAtPath(tgaFile) as TextureImporter;
            CompressTextureHalfSize(tiTGA);
            tiTGA.SaveAndReimport();

            Debug.Log($"Done:{tgaFile}", tiTGA);
        }

        Debug.Log("All done");
    }

    static void CompressTextureHalfSize(TextureImporter tiTGA)
    {
        tiTGA.GetSourceTextureWidthAndHeight(out int width, out int height);
        int maxSize = Mathf.Max(width / 2, height / 2);

        // Android 端单独设置
        TextureImporterPlatformSettings settingAndroid = tiTGA.GetPlatformTextureSettings("Android");
        if (settingAndroid == null)
            settingAndroid = new TextureImporterPlatformSettings();
        settingAndroid.name = "Android";
        settingAndroid.overridden = true;
        settingAndroid.maxTextureSize = maxSize;
        tiTGA.SetPlatformTextureSettings(settingAndroid);

        // IOS端单独设置
        TextureImporterPlatformSettings settingIOS = tiTGA.GetPlatformTextureSettings("iPhone");
        if (settingIOS == null)
            settingIOS = new TextureImporterPlatformSettings();
        settingIOS.name = "iPhone";
        settingIOS.overridden = true;
        settingIOS.maxTextureSize = maxSize;
        tiTGA.SetPlatformTextureSettings(settingIOS);
    }


    [MenuItem("Saber/WUCH/Scene/WEPMaterialUseSaberShader")]
    static void WEPMaterialUseSaberShader()
    {
        List<string> materials = GetSelectedAssets<Material>("*.mat");
        foreach (var path in materials)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat.shader.name.StartsWith("Saber"))
            {
                string newShader = $"Saber/{mat.shader.name}";
                Shader shader = Shader.Find(newShader);
                if (shader)
                {
                    mat.shader = shader;
                }
                else
                {
                    Debug.LogError($"Cann't find shader:{newShader}");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All done");
    }

    [MenuItem("Saber/WUCH/Scene/Revert_WEPMaterialUseSaberShader")]
    static void Revert_WEPMaterialUseSaberShader()
    {
        List<string> materials = GetSelectedAssets<Material>("*.mat");
        foreach (var path in materials)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat.shader.name.StartsWith("Saber"))
            {
                string newShader = mat.shader.name.Substring(5);
                Shader shader = Shader.Find(newShader);
                if (shader)
                {
                    mat.shader = shader;
                }
                else
                {
                    Debug.LogError($"Cann't find shader:{newShader}");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All done");
    }

    [MenuItem("Saber/WUCH/Skill/根据Animator skill信息，填充技能配置文件")]
    static void GenerateSkillItemsByAnimator()
    {
        GameObject obj = Selection.activeObject as GameObject;
        if (obj == null)
        {
            return;
        }

        SActor actor = obj.GetComponent<SActor>();
        if (actor == null)
        {
            return;
        }

        Animator animator = obj.GetComponent<Animator>();
        if (animator == null)
        {
            return;
        }

        List<string> clipsNames = GetSkillAnimClipNames(animator);
        SkillItem[] oldSkills = actor.SkillConfigs.m_SkillItems;
        List<SkillItem> newSkills = new();
        newSkills.AddRange(oldSkills);

        foreach (var clipName in clipsNames)
        {
            if (newSkills.Any(a => a.m_AnimStates.FirstOrDefault().m_Name == clipName))
            {
                continue;
            }

            SkillAnimStateMachine newAnimState = new()
            {
                m_Name = clipName,
            };
            SkillItem skillItem = new SkillItem()
            {
                m_ID = newSkills.Count + 1,
                m_AnimStates = new SkillAnimStateMachine[1] { newAnimState },
                CostStrength = 5,
                m_SkillType = ESkillType.LightAttack,
                m_TriggerCondition = ETriggerCondition.InGround,
                UseGravityWhenInAir = false,
                m_FirstSkillOfCombo = true,
                m_ChainSkills = new ChainSkill[0],
                m_AIPramAttackDistance = new RangedFloat(0f, 3f),
            };
            newSkills.Add(skillItem);

            Debug.Log($"Add skill item:{clipName}");
        }

        actor.SkillConfigs.m_SkillItems = newSkills.ToArray();
        EditorUtility.SetDirty(actor.SkillConfigs);
        AssetDatabase.SaveAssets();
        Debug.Log("Done");
    }

    private static List<string> GetSkillAnimClipNames(Animator animator)
    {
        if (animator == null)
            return null;
        AnimatorController ac = animator.runtimeAnimatorController as AnimatorController;
        var sm = ac.layers[0].stateMachine;
        ChildAnimatorState[] skillAnimStates = null;
        for (int i = 0; i < sm.stateMachines.Length; i++)
        {
            var stateMachine = sm.stateMachines[i].stateMachine;
            if (stateMachine.name.Equals("skill", StringComparison.OrdinalIgnoreCase))
            {
                skillAnimStates = stateMachine.states;
                break;
            }
        }

        List<string> clipsNames = new List<string>();
        if (skillAnimStates != null)
        {
            for (int i = 0; i < skillAnimStates.Length; i++)
            {
                clipsNames.Add(skillAnimStates[i].state.motion.name);
            }
        }

        return clipsNames;
    }

    [MenuItem("Saber/WUCH/Skill/RemoveUselessTracks")]
    static void RemoveSkillEventUselessTracks()
    {
        List<string> assets = GetSelectedAssets<AbilityScriptableObject>("*.asset");
        foreach (var asset in assets)
        {
            AbilityScriptableObject obj = AssetDatabase.LoadAssetAtPath<AbilityScriptableObject>(asset);
            for (int i = obj.events.Count - 1; i >= 0; --i)
            {
                var e = obj.events[i];
                //Debug.Log(e.Obj.name);
                if (e.Obj.name.Contains("TanDao", StringComparison.OrdinalIgnoreCase) ||
                    e.Obj.name.Contains("PerfectDodge", StringComparison.OrdinalIgnoreCase))
                {
                    obj.events.RemoveAt(i);
                }
            }

            EditorUtility.SetDirty(obj);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("all done");
    }
}