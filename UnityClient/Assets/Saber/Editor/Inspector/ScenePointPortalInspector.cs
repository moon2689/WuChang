using System.Collections.Generic;
using System.Linq;
using Saber.AI;
using Saber.Config;
using Saber.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(ScenePointPortal))]
public class ScenePointPortalInspector : Editor
{
    private SceneInfo m_SceneInfo;
    private ScenePointPortal m_ScenePointPortal;

    public override void OnInspectorGUI()
    {
        if (m_SceneInfo == null)
        {
            m_SceneInfo = AssetDatabase.LoadAssetAtPath<SceneInfo>("Assets/Saber/Resources_/Config/SceneInfo.asset");
        }

        m_ScenePointPortal = (ScenePointPortal)target;
        base.OnInspectorGUI();

        if (GUILayout.Button("选择目标场景"))
        {
            ShowSelectSceneWnd();
        }
    }

    void ShowSelectSceneWnd()
    {
        GenericMenu Menu = new GenericMenu();
        foreach (var s in m_SceneInfo.m_Scenes)
        {
            Menu.AddItem(new GUIContent(s.m_ID + " " + s.m_Name), false, OnSelectScene, s);
        }

        Menu.ShowAsContext();
    }

    private void OnSelectScene(object userdata)
    {
        SceneBaseInfo info = userdata as SceneBaseInfo;
        m_ScenePointPortal.m_TargetSceneID = info.m_ID;
        m_ScenePointPortal.m_TargetSceneName = info.m_Name;
        m_ScenePointPortal.m_TargetPortalID = 1;
        EditorUtility.SetDirty(m_ScenePointPortal.transform);
    }
}