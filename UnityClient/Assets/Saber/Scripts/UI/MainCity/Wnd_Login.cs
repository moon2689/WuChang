using System;
using System.Collections.Generic;
using Saber.Config;
using Saber.Director;
using Saber.Frame;
using UnityEngine;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Wnd_Login : WndBase
    {
        public interface IHandler : IWndHandler
        {
            void NewGame();
            void ContinueGame();
        }

        [SerializeField] Button m_ButtonNewGame;
        [SerializeField] Button m_ButtonContinue;
        [SerializeField] GameObject m_ButtonContinueGray;
        [SerializeField] Button m_ButtonExit;

        private IHandler m_Handler;



        public bool EnableContinueGameButton
        {
            set
            {
                m_ButtonContinue.gameObject.SetActive(value);
                m_ButtonContinueGray.SetActive(!value);
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            m_Handler = base.m_WndHandler as IHandler;

            m_ButtonNewGame.onClick.AddListener(OnClickNewGame);
            m_ButtonContinue.onClick.AddListener(OnClickContinueGame);
            m_ButtonExit.onClick.AddListener(OnClickExit);

            EnableContinueGameButton = false;
        }

        private void OnClickExit()
        {
            GameApp.Entry.Game.ExitGame();
        }

        private void OnClickContinueGame()
        {
            m_Handler?.ContinueGame();
            GameApp.Entry.Game.Audio.PlayCommonClick();
            Destroy();
        }

        private void OnClickNewGame()
        {
            m_Handler?.NewGame();
            GameApp.Entry.Game.Audio.PlayCommonClick();
            Destroy();
        }
    }
}