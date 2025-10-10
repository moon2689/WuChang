using System;
using Saber.Frame;
using UnityEngine;
using UnityEngine.UI;


namespace Saber.UI
{
    public class Wnd_Menu : WndBase
    {
        [SerializeField] Button m_BtnResume,
            m_BtnToMainWnd,
            m_BtnSave;


        public interface IHandler : IWndHandler
        {
            void OnClickToMainWnd();
            void OnClickSave();
        }

        private IHandler m_Handler;


        protected override bool PauseGame => true;


        protected override void OnAwake()
        {
            base.OnAwake();
            m_Handler = base.m_WndHandler as IHandler;

            m_BtnResume.onClick.AddListener(OnClickClose);
            m_BtnToMainWnd.onClick.AddListener(OnClickToMainWnd);
            m_BtnSave.onClick.AddListener(OnClickSave);

            GameApp.Entry.Game.Audio.Play2DSound("Sound/UI/ActorInfoWndOpen");
        }

        void OnClickSave()
        {
            Destroy();
            GameApp.Entry.Game.Audio.PlayCommonClick();
            m_Handler.OnClickSave();
        }

        void OnClickToMainWnd()
        {
            Destroy();
            GameApp.Entry.Game.Audio.PlayCommonClick();
            m_Handler.OnClickToMainWnd();
        }

        void OnClickClose()
        {
            GameApp.Entry.Game.Audio.Play2DSound("Sound/UI/ActorInfoWndClose");
            Destroy();
        }
    }
}