using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Saber.UI
{
    public class RootUI : MonoBehaviour
    {
        static RootUI s_Instance;

        public List<WndBase> m_Wnds;
        Camera m_UICamera;
        Canvas m_CanvasObj;
        CanvasScaler m_CanvasScaler;
        CanvasGroup m_CanvasGroupObj;
        EventSystem m_EventSystemObj;


        public static RootUI Instance => s_Instance;
        public Camera UICamera => m_UICamera ??= transform.GetComponentInChildren<Camera>();
        public Canvas CanvasObj => m_CanvasObj ??= transform.GetComponent<Canvas>();
        public CanvasScaler CanvasScalerObj => m_CanvasScaler ??= transform.GetComponent<CanvasScaler>();
        public CanvasGroup CanvasGroupObj => m_CanvasGroupObj ??= transform.GetComponent<CanvasGroup>();
        public EventSystem EventSystemObj => m_EventSystemObj ??= transform.GetComponentInChildren<EventSystem>();

        public Vector2 ScaleRatio => new Vector2(CanvasScalerObj.referenceResolution.x / Screen.width,
            CanvasScalerObj.referenceResolution.y / Screen.height);


        public static void Create(Action onCreated)
        {
            if (s_Instance != null)
            {
                onCreated?.Invoke();
                return;
            }

            GameObject prefab = Resources.Load<GameObject>("Game/Canvas");
            GameObject obj = GameObject.Instantiate(prefab);
            obj.AddComponent<RootUI>();
            onCreated?.Invoke();
        }

        public static void RegisterWnd(WndBase wnd)
        {
            if (s_Instance != null)
                s_Instance.RegisterWndIns(wnd);
        }

        public static void UnRegisterWnd(WndBase wnd)
        {
            if (s_Instance != null)
                s_Instance.UnRegisterWndIns(wnd);
        }


        void Awake()
        {
            s_Instance = this;
            DontDestroyOnLoad(this);
            m_Wnds = new List<WndBase>();

            /*
            // ui camera overlay to main camera
            if (Root3D.Instance != null)
                Root3D.Instance.MainCamera.GetUniversalAdditionalCameraData().cameraStack.Add(UICamera);
            */
        }

        void OnDestroy()
        {
            s_Instance = null;
        }

        void RegisterWndIns(WndBase wnd)
        {
            if (wnd != null && !m_Wnds.Contains(wnd))
            {
                m_Wnds.Add(wnd);
            }
        }

        void UnRegisterWndIns(WndBase wnd)
        {
            m_Wnds.Remove(wnd);
        }

        public void HideAllWnd(params WndBase[] except)
        {
            for (int i = 0; i < m_Wnds.Count; i++)
            {
                var w = m_Wnds[i];
                int index = Array.FindIndex(except, e => e == w);
                bool isExcept = index > -1;
                if (!isExcept)
                    w.IsShow = false;
            }
        }

        public void RevertHideAllWnd()
        {
            for (int i = 0; i < m_Wnds.Count; i++)
            {
                var w = m_Wnds[i];
                w.IsShow = true;
            }
        }

        public T GetShowingWnd<T>() where T : WndBase
        {
            for (int i = 0; i < m_Wnds.Count; i++)
            {
                if (m_Wnds[i] is T w)
                    return w;
            }

            return null;
        }

        public void DestroyWnd<T>() where T : WndBase
        {
            T wnd = GetShowingWnd<T>();
            if (wnd)
            {
                wnd.Destroy();
            }
        }
    }
}