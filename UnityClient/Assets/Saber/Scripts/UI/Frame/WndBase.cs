using System;
using UnityEngine;

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


        public static T Create<T>(WndContent content, IWndHandler handler) where T : WndBase, new()
        {
            string name = typeof(T).Name;
            string path = $"UI/{name}";
            GameObject prefab = Resources.Load<GameObject>(path);
            GameObject go = GameObject.Instantiate(prefab, RootUI.Instance.transform);
            T wnd = go.GetComponent<T>();
            wnd.m_WndContent = content;
            wnd.m_WndHandler = handler;
            wnd.OnAwake();
            return wnd;
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