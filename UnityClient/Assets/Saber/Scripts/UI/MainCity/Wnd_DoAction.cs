using Saber.Frame;
using UnityEngine;
using UnityEngine.UI;


namespace Saber.UI
{
    public class Wnd_DoAction : WndBase
    {
        [SerializeField] Button m_btnClose, m_btnDance, m_btnAction1;


        public interface IHandler:IWndHandler
        {
            void OnClickDance();
            void OnClickAction(int id);
        }

        private IHandler m_Handler;
        

        protected override void OnAwake()
        {
            base.OnAwake();

            m_btnClose.onClick.AddListener(OnClickClose);
            m_btnDance.onClick.AddListener(OnClickDance);
            m_btnAction1.onClick.AddListener(OnClickAction1);
        }

        void OnClickClose()
        {
            Destroy();
        }

        void OnClickDance()
        {
            GameApp.Entry.Game.Audio.PlayCommonClick();
            m_Handler.OnClickDance();
            Destroy();
        }

        void OnClickAction1()
        {
            GameApp.Entry.Game.Audio.PlayCommonClick();
            m_Handler.OnClickAction(1);
            Destroy();
        }

        protected override void OnDestroy()
        {
            GameApp.Entry.Game.Audio.Play2DSound("ActorInfoWndClose");
            base.OnDestroy();
        }
    }
}