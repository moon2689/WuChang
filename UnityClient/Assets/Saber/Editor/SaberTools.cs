using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Saber.CharacterController;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;

public static class SaberTools
{
    [MenuItem("Saber/Asset/Fix FBX Anim Name")]
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
    /*
    [MenuItem("Saber/Anim/Fix Animation Clip")]
    static void FixAnimationClip()
    {
        foreach (var selectedObj in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedObj);
            ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (modelImporter == null)
            {
                continue;
            }

            foreach (var clip in modelImporter.defaultClipAnimations)
            {
                clip.loopTime = false;
                clip.keepOriginalOrientation = true;
                clip.lockRootRotation = true;
                clip.keepOriginalPositionY = true;
                clip.lockRootHeightY = true;
                clip.keepOriginalPositionXZ = true;
                clip.lockRootPositionXZ = false;
            }
            
            //modelImporter.SaveAndReimport();
            Debug.Log($"Fixed {selectedObj.name}", selectedObj);
        }

        // AssetDatabase.SaveAssets();
        // AssetDatabase.Refresh();

        Debug.Log("All done");
    }
    */

    [MenuItem("Saber/Asset/Fix Anim State Machine")]
    static void FixAnimStateMachine()
    {
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            if (Selection.objects[i] is AnimatorState state)
            {
                FixAnimState(state);
                EditorUtility.SetDirty(state);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Fix all anim state done");
    }

    static void FixAnimState(AnimatorState state)
    {
        Debug.Log(state.name);
        // var list = new List<AudioClip>();
        // string path = "Assets/Saber/Art/Sounds/Human/Roll.wav";
        // list.Add(AssetDatabase.LoadAssetAtPath<AudioClip>(path));
        foreach (var b in state.behaviours)
        {
            if (b is AnimEvent_CommonPointTime pointEvent)
            {
                if (pointEvent.EventType == EAnimTriggerEvent.AnimCanExit)
                    pointEvent.m_TriggerTime = 0.52f;
            }

            if (b is AnimEvent_CommonRangeTime rangeEvent)
            {
                if (rangeEvent.EventType == EAnimRangeEvent.CanTriggerSkill)
                    rangeEvent.m_RangeTime = new RangedFloat(0.42f, 0.52f);
                else if (rangeEvent.EventType == EAnimRangeEvent.Invincible)
                    rangeEvent.m_RangeTime = new RangedFloat(0, 0.42f);
            }
        }

        /*
        AnimEvent_CommonRangeTime triggerSkillEvent = state.AddStateMachineBehaviour<AnimEvent_CommonRangeTime>();
        triggerSkillEvent.m_EventType = EAnimRangeEvent.CanTriggerSkill;
        triggerSkillEvent.m_RangeTime = new RangedFloat(0.5f, 0.58f);
        */

        //state.name = state.name.Replace("Armed 0", "Unarmed");
        /*
        // 销毁脚本
        foreach (var b in state.behaviours)
            GameObject.DestroyImmediate(b, true);
        */

        // 添加事件脚本
        //state.AddStateMachineBehaviour<AnimEvent_AnimCanExit>().m_TriggerTime = 0.8f;
        //state.AddStateMachineBehaviour<AnimEvent_Invincible>().m_RangeTime = new RangedFloat(0.1f, 0.7f);
        //state.AddStateMachineBehaviour<AnimEvent_CanTriggerSkill>().m_RangeTime = new RangedFloat(0.7f, 0.8f);

        /*
        // 添加播放声音事件脚本
        AnimEvent_PlaySound eventObj = state.AddStateMachineBehaviour<AnimEvent_PlaySound>();
        eventObj.m_TriggerTime = 0;
        eventObj.m_Volume = 1;
        var list = new List<AudioClip>();
        string path = "Assets/Saber/Art/Sounds/Human/Jump.wav";
        list.Add(AssetDatabase.LoadAssetAtPath<AudioClip>(path));
        eventObj.m_Clips = list;
        */
    }

    [MenuItem("Saber/Asset/Find Anim Behaviour")]
    static void FindAnimBehaviour()
    {
        if (Selection.activeObject is AnimatorController c)
        {
            for (int i = 0; i < c.layers.Length; i++)
            {
                foreach (var s in c.layers[i].stateMachine.stateMachines)
                    FindAnimTargetBehaviour(s);
            }
        }
    }

    static void FindAnimTargetBehaviour(ChildAnimatorStateMachine layer)
    {
        foreach (var state in layer.stateMachine.states)
        {
            foreach (var b in state.state.behaviours)
            {
                // Debug.Log($"{state.state.name}");

                /*
                if (b is AnimEvent_AnimCanExit old)
                {
                    var common = state.state.AddStateMachineBehaviour<AnimEvent_CommonPointTime>();
                    common.m_TriggerTime = old.m_TriggerTime;
                    common.m_EventType = old.EventType;
                    GameObject.DestroyImmediate(b, true);
                }
               
                
                
                else if (b is AnimEvent_CanTriggerSkill old2)
                {
                    var common = state.state.AddStateMachineBehaviour<AnimEvent_CommonRangeTime>();
                    common.m_RangeTime = old2.m_RangeTime;
                    common.m_EventType = old2.EventType;
                    GameObject.DestroyImmediate(b, true);
                }
                */
            }
        }

        foreach (var stateMachine in layer.stateMachine.stateMachines)
        {
            FindAnimTargetBehaviour(stateMachine);
        }
    }

    [MenuItem("Saber/运行游戏 %#J")]
    static void LauncherGame()
    {
        OpenScene_Launcher();
        EditorApplication.isPlaying = true;
    }

    [MenuItem("Saber/打开场景 Launcher")]
    static void OpenScene_Launcher()
    {
        EditorSceneManager.OpenScene("Assets/Saber/Scenes/SLauncher.unity");
    }

    [MenuItem("Saber/打开场景 Debug")]
    static void OpenScene_Debug()
    {
        EditorSceneManager.OpenScene("Assets/Saber/Scenes/Debug/Debug.unity");
    }

    [MenuItem("Saber/游戏设置")]
    static void SelectFile_GameSetting()
    {
        var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Saber/Resources/Config/GameSetting.asset");
        Selection.activeObject = obj;
    }

    [MenuItem("Saber/拷贝Resources路径 %#U")]
    public static void CopyResPath()
    {
        if (!Selection.activeObject)
            return;

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string ext = Path.GetExtension(path);

        string[] words = path.Split("Resources/");
        string target = words[1].Substring(0, words[1].Length - ext.Length);

        Debug.Log(target);
        CopyString(target);
    }

    [MenuItem("Saber/Asset/DebugAnimHashName")]
    public static void DebugAnimHashName()
    {
        string name = "StandUpFromLie";
        int id = Animator.StringToHash(name);
        Debug.Log($"{name}  {id}");
    }

    private static void CopyString(string str)
    {
        var te = new TextEditor
        {
            text = str
        };
        te.SelectAll();
        te.Copy();
    }
}