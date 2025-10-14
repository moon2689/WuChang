using System;
using System.Collections.Generic;
using System.Linq;
using Saber.Frame;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using YooAsset;

namespace Saber.UI
{
    public class RootUI : MonoBehaviour
    {
        static RootUI s_Instance;

        [SerializeField] private Image m_ImageBackground;

        private List<WndBase> m_Wnds = new();
        private Camera m_UICamera;
        private Canvas m_CanvasObj;
        private CanvasScaler m_CanvasScaler;
        private CanvasGroup m_CanvasGroupObj;
        private EventSystem m_EventSystemObj;


        public static RootUI Instance => s_Instance;
        public Camera UICamera => m_UICamera ??= transform.GetComponentInChildren<Camera>();
        public Canvas CanvasObj => m_CanvasObj ??= transform.GetComponent<Canvas>();
        public CanvasScaler CanvasScalerObj => m_CanvasScaler ??= transform.GetComponent<CanvasScaler>();
        public CanvasGroup CanvasGroupObj => m_CanvasGroupObj ??= transform.GetComponent<CanvasGroup>();
        public EventSystem EventSystemObj => m_EventSystemObj ??= transform.GetComponentInChildren<EventSystem>();
        public Vector2 ScaleRatio => new Vector2(CanvasScalerObj.referenceResolution.x / Screen.width,
            CanvasScalerObj.referenceResolution.y / Screen.height);


        public static AssetHandle Create()
        {
            if (s_Instance == null)
            {
                return GameApp.Entry.Asset.LoadGameObject("Game/Canvas", null);
            }

            return null;
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

        public void HideBackground()
        {
            m_ImageBackground.gameObject.SetActive(false);
        }
    }
}