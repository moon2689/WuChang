using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RootMotion.FinalIK;
using Saber.CharacterController;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;

public static class WuChangTools
{
    private static string UEProjectFolder = @"E:/1/WuChangUnity/Exports";


    static MD5 s_md5;

    static MD5 MD5Obj => s_md5 ??= MD5.Create();

    [MenuItem("Saber/WuCH/Add Hurt Box")]
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

    [MenuItem("Saber/WuCH/Delete repeat wav")]
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


    [MenuItem("Saber/WuCH/Fix Material")]
    static void FixWuChangMaterials()
    {
        foreach (TextAsset json in Selection.objects)
        {
            FixWuChMaterial(json);
        }

        Debug.Log("FixWuChangMaterials all done");
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
        string normalPath, maskPath;
        if (texPath.Contains("_D.tga"))
        {
            normalPath = texPath.Replace("_D.tga", "_N.tga");
            maskPath = texPath.Replace("_D.tga", "_R.tga");
        }
        else if (texPath.Contains("_Albedo.tga"))
        {
            normalPath = texPath.Replace("_Albedo.tga", "_Normal.tga");
            maskPath = texPath.Replace("_Albedo.tga", "_Reflection.tga");
        }
        else
        {
            Debug.LogError("Unknown tex path:" + texPath);
            return;
        }

        Texture2D diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

        if (diffuse == null)
        {
            Debug.LogError($"diffuse is null:{fileName},texPath:{texPath}");
            return;
        }

        // normal
        TextureImporter tiNormal = AssetImporter.GetAtPath(normalPath) as TextureImporter;
        tiNormal.textureType = TextureImporterType.NormalMap;
        tiNormal.SaveAndReimport();
        Texture2D texNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);

        // mask
        TextureImporter tiMask = AssetImporter.GetAtPath(maskPath) as TextureImporter;
        tiMask.sRGBTexture = false;
        tiMask.SaveAndReimport();
        Texture2D texMask = AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);

        mat.shader = Shader.Find("Saber/WuChang/WuChang Common Lit");
        mat.SetTexture("_BaseMap", diffuse);
        mat.SetColor("_BaseColor", Color.white);
        mat.SetTexture("_BumpMap", texNormal);
        mat.SetTexture("_MaskMROMap", texMask);

        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("FixWuChMaterial done:" + mat.name, mat);
    }


    [MenuItem("Saber/WuCH/Fix FBX Anim Name")]
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

    [MenuItem("Saber/WuCH/Import material images from ue")]
    static void ImportAllMaterialImageFromUE()
    {
        List<string> listMaterial = new();
        foreach (var obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (obj is DefaultAsset)
            {
                string[] files = Directory.GetFiles(path, "*.mat", SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    if (!listMaterial.Contains(f))
                        listMaterial.Add(f);
                }
            }
            else if (obj is Material)
            {
                if (!listMaterial.Contains(path))
                    listMaterial.Add(path);
            }
        }

        string[] allJsonFiles = Directory.GetFiles(UEProjectFolder + "/Project_Plague/Content", "*.json", SearchOption.AllDirectories);
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
            string diffuseUE = GetUEDiffuseTGAPath(jsonFile, out string normalUE, out string maskUE);
            if (diffuseUE == null)
            {
                continue;
            }

            string matFolder = Path.GetDirectoryName(m);
            string matParentFolder = Path.GetDirectoryName(matFolder);
            string textureSaveFolder = matParentFolder + "/Textures";
            if (!AssetDatabase.IsValidFolder(textureSaveFolder))
            {
                AssetDatabase.CreateFolder(matParentFolder, "Textures");
                AssetDatabase.Refresh();
            }

            string newDiffuse = ImportOrLoadFromLocal(diffuseUE, dicAllLocalTga, textureSaveFolder);
            Texture2D diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(newDiffuse);

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(m);
            mat.shader = Shader.Find("Saber/WuChang/WuChang Common Lit");
            mat.SetTexture("_BaseMap", diffuse);
            mat.SetColor("_BaseColor", Color.white);

            if (!string.IsNullOrEmpty(normalUE))
            {
                string newNormal = ImportOrLoadFromLocal(normalUE, dicAllLocalTga, textureSaveFolder);
                TextureImporter tiNormal = AssetImporter.GetAtPath(newNormal) as TextureImporter;
                tiNormal.textureType = TextureImporterType.NormalMap;
                tiNormal.SaveAndReimport();
                Texture2D texNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(newNormal);
                mat.SetTexture("_BumpMap", texNormal);
            }

            if (!string.IsNullOrEmpty(maskUE))
            {
                string newMask = ImportOrLoadFromLocal(maskUE, dicAllLocalTga, textureSaveFolder);
                TextureImporter tiMask = AssetImporter.GetAtPath(newMask) as TextureImporter;
                tiMask.sRGBTexture = false;
                tiMask.SaveAndReimport();
                Texture2D texMask = AssetDatabase.LoadAssetAtPath<Texture2D>(newMask);
                mat.SetTexture("_MaskMROMap", texMask);
            }

            Debug.Log($"Fix material done:{m}");
        }

        Debug.Log("All done");
    }

    static string ImportOrLoadFromLocal(string tgaUE, Dictionary<string, string> dicAllLocalTga, string saveFolder)
    {
        if (dicAllLocalTga.TryGetValue(tgaUE, out string localPath))
        {
            return localPath;
        }
        else
        {
            string tgaName = Path.GetFileName(tgaUE);
            string savePath = saveFolder + "/" + tgaName;
            if (!File.Exists(savePath))
            {
                File.Copy(tgaUE, savePath);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }

            return savePath;
        }
    }

    static string GetUEDiffuseTGAPath(string jsonFile, out string normalUE, out string maskUE)
    {
        normalUE = null;
        maskUE = null;

        string[] lines = File.ReadAllLines(jsonFile);
        string diffuseUE = null;
        foreach (var line in lines)
        {
            string[] words = line.Split(new string[] { ":", "\"", ",", " " }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 1 && words[0] == "PM_Diffuse")
            {
                diffuseUE = words[1];
                break;
            }
        }

        if (string.IsNullOrEmpty(diffuseUE))
        {
            return null;
        }

        string ext = Path.GetExtension(diffuseUE);
        diffuseUE = diffuseUE.Replace(ext, ".tga");
        diffuseUE = UEProjectFolder + "/" + diffuseUE;

        if (diffuseUE.Contains("_D.tga"))
        {
            normalUE = diffuseUE.Replace("_D.tga", "_N.tga");
            maskUE = diffuseUE.Replace("_D.tga", "_R.tga");
        }
        else if (diffuseUE.Contains("_Albedo.tga"))
        {
            normalUE = diffuseUE.Replace("_Albedo.tga", "_Normal.tga");
            maskUE = diffuseUE.Replace("_Albedo.tga", "_Reflection.tga");
        }
        else
        {
            Debug.LogError("Unknown tex path:" + diffuseUE);
        }

        return diffuseUE;
    }
}