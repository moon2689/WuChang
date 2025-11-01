using System;
using Saber.CharacterController;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Wnd_MainCity : WndBase
    {
        public interface IHandler : IWndHandler, Widget_SlotObject.IHandler
        {
            void OnClickMenu();
        }

        [SerializeField] private Button m_BtnMenu;
        [SerializeField] private RectTransform m_IconLockScreen;
        [SerializeField] private RectTransform m_IconLock;
        [SerializeField] private Image m_IconLockFill;
        [SerializeField] private GameObject m_IconLockDecapitate;

        [SerializeField] private GameObject[] m_SlotsArray;
        [SerializeField] private Widget_SlotObject m_SlotObject;
        [SerializeField] private SpriteAtlas m_AtlasPropIcons;
        [SerializeField] private SpriteAtlas m_AtlasSkillIcons;

        private IHandler m_Handler;
        private Widget_SlotObject[] m_SlotObjects;


        protected override void OnAwake()
        {
            base.OnAwake();
            m_Handler = base.m_WndHandler as IHandler;
            m_BtnMenu.onClick.AddListener(OnClickMenu);

            m_SlotObject.gameObject.SetActive(false);
            m_SlotObjects = new Widget_SlotObject[m_SlotsArray.Length];
            for (int i = 0; i < m_SlotObjects.Length; i++)
            {
                GameObject go = GameObject.Instantiate(m_SlotObject.gameObject, m_SlotsArray[i].transform);
                go.transform.localPosition = Vector3.zero;
                m_SlotObjects[i] = go.GetComponent<Widget_SlotObject>();
                m_SlotObjects[i].Init(GameApp.Entry.Game.ProgressMgr.SlotsArray[i], m_Handler, m_AtlasPropIcons, m_AtlasSkillIcons);
            }
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
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_IconLockScreen, screenPos, worldCamera, out Vector2 localPoint);
                m_IconLock.anchoredPosition = localPoint;
                m_IconLockDecapitate.SetActive(lockEnemy.IsBlockBrokenWaitExecute);
                m_IconLockFill.fillAmount = 1 - lockEnemy.CStats.CurrentUnbalanceValue / lockEnemy.CStats.MaxUnbalanceValue;
            }

#if UNITY_EDITOR
            UpdatePCInput();
#endif
        }


#if UNITY_EDITOR
        private void UpdatePCInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                m_SlotObjects[0].ClickSlot();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                m_SlotObjects[1].ClickSlot();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                m_SlotObjects[2].ClickSlot();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                m_SlotObjects[3].ClickSlot();
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                m_SlotObjects[4].ClickSlot();
            }

            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                m_SlotObjects[5].ClickSlot();
            }

            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                m_SlotObjects[6].ClickSlot();
            }

            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                m_SlotObjects[7].ClickSlot();
            }

            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                m_SlotObjects[8].ClickSlot();
            }

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                m_SlotObjects[9].ClickSlot();
            }
        }
#endif
    }
}