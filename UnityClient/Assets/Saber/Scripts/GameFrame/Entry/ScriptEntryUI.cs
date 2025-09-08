using System;
using Saber.UI;
using UnityEngine;
using YooAsset;


namespace Saber.Frame
{
    public class ScriptEntryUI
    {
        public RootUI RootUIObj => RootUI.Instance;

        public AssetHandle CreateWnd<T>(WndContent content, IWndHandler handler, Action<T> onCreated) where T : WndBase, new()
        {
            T t = RootUI.Instance.GetShowingWnd<T>();
            if (t != null)
            {
                onCreated?.Invoke(t);
                return null;
            }

            return WndBase.Create<T>(content, handler, onCreated);
        }

        public AssetHandle CreateWnd<T>(Action<T> onCreated) where T : WndBase, new()
        {
            return CreateWnd<T>(null, null, onCreated);
        }

        public AssetHandle CreateMsgBox(string msg, Action onConfirm)
        {
            return CreateWnd<Wnd_MsgBox>(w => w.Reset(msg, onConfirm));
        }

        public AssetHandle CreateMsgBox(string msg, Action onConfirm, Action onCancel)
        {
            return CreateWnd<Wnd_MsgBox>(w => w.Reset(msg, onConfirm, onCancel));
        }

        public AssetHandle ShowTips(string msg)
        {
            return CreateWnd<Wnd_Tips>(w => w.ShowText(msg));
        }

        public AssetHandle ShowTips(string msg, float textTime)
        {
            return CreateWnd<Wnd_Tips>(w => w.ShowText(msg, textTime));
        }

        public T GetWnd<T>() where T : WndBase, new()
        {
            return RootUI.Instance.GetShowingWnd<T>();
        }
    }
}