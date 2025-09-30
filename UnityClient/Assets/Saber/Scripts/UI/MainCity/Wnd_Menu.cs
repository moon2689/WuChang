using System;
using Saber.Frame;
using UnityEngine;
using UnityEngine.UI;


namespace Saber.UI
{
    public class Wnd_Menu : WndBase
    {
        [SerializeField] Button m_BtnResume,
            m_BtnBackToLastGodStatue,
            m_BtnToMainWnd,
            m_BtnWait;


        public interface IHandler : IWndHandler
        {
            void OnClickBackToLastGodStatue();
            void OnClickToMainWnd();
            void OnClickWait();
        }

        private IHandler m_Handler;


        protected override void OnAwake()
        {
            base.OnAwake();
            m_Handler = base.m_WndHandler as IHandler;

            m_BtnResume.onClick.AddListener(OnClickClose);
            m_BtnBackToLastGodStatue.onClick.AddListener(OnClickBackToLastGodStatue);
            m_BtnToMainWnd.onClick.AddListener(OnClickToMainWnd);
            m_BtnWait.onClick.AddListener(OnClickWait);

            GameApp.Entry.Game.Audio.Play2DSound("Sound/UI/ActorInfoWndOpen");
        }

        void OnClickWait()
        {
            Destroy();
            GameApp.Entry.Game.Audio.PlayCommonClick();
            m_Handler.OnClickWait();
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

        void OnClickBackToLastGodStatue()
        {
            Destroy();
            GameApp.Entry.Game.Audio.PlayCommonClick();
            m_Handler.OnClickBackToLastGodStatue();
        }
    }
}