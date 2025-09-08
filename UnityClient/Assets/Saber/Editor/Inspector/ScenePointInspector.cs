using System.Collections.Generic;
using System.Linq;
using Saber.AI;
using Saber.Config;
using Saber.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(ScenePoint))]
public class ScenePointInspector : Editor
{
    private ActorInfo m_ActorInfo;

    private ScenePoint Obj => (ScenePoint)target;

    public override void OnInspectorGUI()
    {
        if (m_ActorInfo == null)
        {
            m_ActorInfo = AssetDatabase.LoadAssetAtPath<ActorInfo>("Assets/Saber/Resources_/Config/ActorInfo.asset");
        }

        EScenePointType oldType = Obj.m_PointType;
        Obj.m_PointType = (EScenePointType)EditorGUILayout.EnumPopup("类型：", Obj.m_PointType);

        if (Obj.m_PointType == EScenePointType.MonsterBornPosition)
        {
            EditorGUILayout.LabelField($"怪物ID：{Obj.m_ID}");
            Obj.m_AIType = (EAIType)EditorGUILayout.EnumPopup("AI：", Obj.m_AIType);
            EditorUtility.SetDirty(Obj.transform);
            if (GUILayout.Button("选择怪物"))
            {
                ShowSelectMonsterWnd();
            }
        }
        else if (Obj.m_PointType == EScenePointType.Portal || Obj.m_PointType == EScenePointType.Idol)
        {
            Obj.m_ID = EditorGUILayout.IntField("ID:", Obj.m_ID);
            Obj.m_PointName = EditorGUILayout.TextField("名字：", Obj.m_PointName);
            EditorUtility.SetDirty(Obj.transform);

            if (GUILayout.Button("同步至配置"))
            {
                SyncToSceneConfig();
            }
        }

        // refresh name
        if (oldType != Obj.m_PointType || string.IsNullOrEmpty(Obj.m_PointName))
        {
            RefreshName();
        }

        if (GUILayout.Button("刷新"))
        {
            RefreshName();
        }
    }

    void SyncToSceneConfig()
    {
        SceneInfo sceneInfo = AssetDatabase.LoadAssetAtPath<SceneInfo>("Assets/Saber/Resources_/Config/SceneInfo.asset");
        ScenePoint[] points = GameObject.FindObjectsOfType<ScenePoint>();
        List<IdolInfo> listIdols = new();
        foreach (var p in points)
        {
            if (p.m_PointType == EScenePointType.Idol)
            {
                IdolInfo idolInfo = new()
                {
                    m_ID = p.m_ID,
                    m_Name = p.m_PointName,
                };
                listIdols.Add(idolInfo);
            }
        }

        List<int> listPortal = new();
        foreach (var p in points)
        {
            if (p.m_PointType == EScenePointType.Portal)
            {
                listPortal.Add(p.m_ID);
            }
        }

        var sceneItem = sceneInfo.m_Scenes.First(a => a.m_ResName == SceneManager.GetActiveScene().name);
        sceneItem.m_Idols = listIdols.ToArray();
        sceneItem.m_Portals = listPortal.ToArray();
        EditorUtility.SetDirty(sceneInfo);

        Debug.Log("SyncToSceneConfig finished", sceneInfo);
    }

    void ShowSelectMonsterWnd()
    {
        GenericMenu Menu = new GenericMenu();
        foreach (var a in m_ActorInfo.m_Actors)
        {
            if (a.m_ActorType == EActorType.Monster || a.m_ActorType == EActorType.Boss)
            {
                Menu.AddItem(new GUIContent(a.m_Name), false, OnSelectActor, a);
            }
        }

        Menu.ShowAsContext();
    }

    private void OnSelectActor(object userdata)
    {
        ActorItemInfo info = (ActorItemInfo)userdata;
        Obj.m_ID = info.m_ID;
        EditorUtility.SetDirty(Obj.transform);
    }

    void RefreshName()
    {
        if (Obj.m_PointType == EScenePointType.PlayerBornPosition)
        {
            var info = m_ActorInfo.m_Actors.FirstOrDefault(a => a.m_ActorType == EActorType.Player);
            Obj.m_PointName = info != null ? info.m_Name : null;
        }
        else if (Obj.m_PointType == EScenePointType.MonsterBornPosition)
        {
            var info = m_ActorInfo.m_Actors.FirstOrDefault(a => a.m_ID == Obj.m_ID);
            Obj.m_PointName = info != null ? info.m_Name : null;
        }


        EditorUtility.SetDirty(Obj.transform);
    }
}