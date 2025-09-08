using System;
using Saber.Frame;
using UnityEngine;
using YooAsset;

namespace Saber.UI
{
    public abstract class WndBase : UIItem
    {
        protected abstract bool PauseGame { get; }

        protected WndContent m_WndContent;
        protected IWndHandler m_WndHandler;

        public bool IsShow
        {
            set => gameObject.SetActive(value);
        }


        public static AssetHandle Create<T>(WndContent content, IWndHandler handler, Action<T> onCreated) where T : WndBase, new()
        {
            string name = typeof(T).Name;
            string path = $"UI/{name}";
            return GameApp.Entry.Asset.LoadGameObject(path, go =>
            {
                go.transform.SetParent(RootUI.Instance.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                RectTransform rectTrans = go.GetComponent<RectTransform>();
                rectTrans.offsetMin = Vector2.zero;
                rectTrans.offsetMax = Vector2.zero;

                T wnd = go.GetComponent<T>();
                wnd.m_WndContent = content;
                wnd.m_WndHandler = handler;
                wnd.OnAwake();
                onCreated?.Invoke(wnd);
            });
        }

        protected override void Awake()
        {
            base.Awake();
            RootUI.RegisterWnd(this);
        }

        protected virtual void OnAwake()
        {
        }

        protected override void Start()
        {
            base.Start();

            if (PauseGame)
            {
                Time.timeScale = 0;
            }
        }

        public override void Destroy()
        {
            base.Destroy();

            if (PauseGame)
            {
                Time.timeScale = 1;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            RootUI.UnRegisterWnd(this);
        }
    }
}