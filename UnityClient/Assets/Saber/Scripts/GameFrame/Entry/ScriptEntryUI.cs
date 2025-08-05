using System;
using Saber.UI;
using UnityEngine;


namespace Saber.Frame
{
    public class ScriptEntryUI
    {
        public RootUI RootUIObj => RootUI.Instance;

        public T CreateWnd<T>(WndContent content, IWndHandler handler) where T : WndBase, new()
        {
            T t = RootUI.Instance.GetShowingWnd<T>();
            if (t != null)
            {
                return t;
            }

            return WndBase.Create<T>(content, handler);
        }

        public T CreateWnd<T>() where T : WndBase, new()
        {
            return CreateWnd<T>(null, null);
        }

        public void CreateMsgBox(string msg, Action onConfirm)
        {
            var w = CreateWnd<Wnd_MsgBox>();
            w.Reset(msg, onConfirm);
        }

        public void CreateMsgBox(string msg, Action onConfirm, Action onCancel)
        {
            var w = CreateWnd<Wnd_MsgBox>();
            w.Reset(msg, onConfirm, onCancel);
        }

        public void ShowTips(string msg)
        {
            var w = CreateWnd<Wnd_Tips>();
            w.ShowText(msg);
        }

        public void ShowTips(string msg, float textTime)
        {
            var w = CreateWnd<Wnd_Tips>();
            w.ShowText(msg, textTime);
        }
    }
}