using System;
using Saber.CharacterController;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Wnd_MainCity : WndBase
    {
        public interface IHandler : IWndHandler
        {
            void OnClickMenu();
        }

        [SerializeField] private Button m_BtnMenu;
        [SerializeField] private RectTransform m_IconLockScreen;
        [SerializeField] private RectTransform m_IconLock;
        [SerializeField] private GameObject m_IconLockDecapitate;

        private IHandler m_Handler;
        

        protected override void OnAwake()
        {
            base.OnAwake();
            m_Handler = base.m_WndHandler as IHandler;
            m_BtnMenu.onClick.AddListener(OnClickMenu);
        }

        void OnClickMenu()
        {
            m_Handler.OnClickMenu();
        }

        protected override void Update()
        {
            base.Update();

            var player = GameApp.Entry.Game.Player;
            var lockEnemy = player.AI.LockingEnemy;
            m_IconLock.gameObject.SetActive(lockEnemy != null);

            if (lockEnemy != null)
            {
                Vector3 lockPos = lockEnemy.GetNodeTransform(ENodeType.LockUIPos).position;
                Vector3 screenPos = GameApp.Entry.Game.PlayerCamera.Cam.WorldToScreenPoint(lockPos);
                var worldCamera = GameApp.Entry.UI.RootUIObj.CanvasObj.worldCamera;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_IconLockScreen, screenPos, worldCamera,
                    out Vector2 localPoint);
                m_IconLock.anchoredPosition = localPoint;

                m_IconLockDecapitate.SetActive(lockEnemy.IsBlockBrokenWaitExecute);
            }
        }
    }
}