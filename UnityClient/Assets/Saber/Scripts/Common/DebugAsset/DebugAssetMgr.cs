using System.Collections;
using System.Collections.Generic;
using Saber;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using YooAsset;

public class DebugAssetMgr : MonoBehaviour
{
    private static DebugAssetMgr s_Instance;
    private GUIStyle m_StyleButton;
    private GUIStyle m_LabelStyle;
    private MobileThirdPersonCamera m_CamCtrl;
    private GameObject m_GoAsset;
    private string m_Msg;

    public static DebugAssetMgr Create()
    {
        if (s_Instance == null)
        {
            GameObject go = new GameObject(nameof(DebugAssetMgr));
            s_Instance = go.AddComponent<DebugAssetMgr>();
        }

        return s_Instance;
    }

    void Awake()
    {
        StartCoroutine(InitItor());

        m_LabelStyle = new()
        {
            fontSize = 40,
            normal =
            {
                textColor = Color.white
            }
        };
    }

    IEnumerator InitItor()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        yield return YooAssetManager.Instance.Init();

        GameObject goLight = new GameObject("Light");
        goLight.transform.rotation = Quaternion.Euler(20, -180, 0);
        Light light = goLight.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.8f;

        GameObject goCamera = new GameObject("Camera");
        goCamera.AddComponent<Camera>();
        m_CamCtrl = goCamera.AddComponent<MobileThirdPersonCamera>();
        m_CamCtrl.rotationSpeed = 5;

        yield return null;

        LoadGameObject("player");
    }

    void OnGUI()
    {
        if (m_StyleButton == null)
        {
            m_StyleButton = new GUIStyle(GUI.skin.button);
            m_StyleButton.fontSize = 40;
        }

        if (GUI.Button(new Rect(20, 30, 250, 100), "加载player", m_StyleButton))
        {
            LoadGameObject("player");
        }

        if (GUI.Button(new Rect(20, 140, 250, 100), "加载cloth", m_StyleButton))
        {
            LoadGameObject("cloth");
        }

        if (!string.IsNullOrEmpty(m_Msg))
        {
            GUI.Box(new Rect(0, 250, 500, 200), "");
            GUI.Label(new Rect(0, 260, 500, 40), m_Msg, m_LabelStyle);
        }
    }

    void LoadGameObject(string assetName)
    {
        GameApp.Entry.Asset.LoadGameObject(assetName, a =>
        {
            if (m_GoAsset)
            {
                GameObject.Destroy(m_GoAsset);
            }

            m_GoAsset = a;
            m_CamCtrl.target = m_GoAsset.transform;
        });

        m_Msg = $"成功加载：{assetName}";
    }
}