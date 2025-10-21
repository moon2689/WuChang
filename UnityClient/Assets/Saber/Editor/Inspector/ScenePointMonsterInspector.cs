using System.Collections.Generic;
using System.Linq;
using Saber.AI;
using Saber.Config;
using Saber.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(ScenePointMonster))]
public class ScenePointMonsterInspector : Editor
{
    private ActorInfo m_ActorInfo;
    private ScenePointMonster m_ScenePointMonster;

    public override void OnInspectorGUI()
    {
        if (m_ActorInfo == null)
        {
            m_ActorInfo = AssetDatabase.LoadAssetAtPath<ActorInfo>("Assets/Saber/Resources_/Config/ActorInfo.asset");
        }

        m_ScenePointMonster = (ScenePointMonster)target;
        base.OnInspectorGUI();

        if (GUILayout.Button("选择怪物"))
        {
            ShowSelectMonsterWnd();
        }

        if (GUILayout.Button("同步至配置"))
        {
            SyncToSceneConfig();
        }
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
        m_ScenePointMonster.m_ID = info.m_ID;
        m_ScenePointMonster.m_PointName = info.m_Name;
        EditorUtility.SetDirty(m_ScenePointMonster.transform);
    }


    void SyncToSceneConfig()
    {
        SceneInfo sceneInfo = AssetDatabase.LoadAssetAtPath<SceneInfo>("Assets/Saber/Resources_/Config/SceneInfo.asset");
        List<ShenKanInfo> listShenKans = new();
        foreach (ScenePointShenKan p in FindObjectsOfType<ScenePointShenKan>())
        {
            ShenKanInfo shenKanInfo = new()
            {
                m_ID = p.m_ID,
                m_Name = p.m_PointName,
            };
            listShenKans.Add(shenKanInfo);
        }

        List<int> listPortal = new();
        foreach (var p in FindObjectsOfType<ScenePointPortal>())
        {
            listPortal.Add(p.m_ID);
        }

        var sceneItem = sceneInfo.m_Scenes.First(a => a.m_ResName == SceneManager.GetActiveScene().name);
        sceneItem.m_ShenKans = listShenKans.ToArray();
        sceneItem.m_Portals = listPortal.ToArray();
        EditorUtility.SetDirty(sceneInfo);

        Debug.Log("SyncToSceneConfig finished", sceneInfo);
    }
}