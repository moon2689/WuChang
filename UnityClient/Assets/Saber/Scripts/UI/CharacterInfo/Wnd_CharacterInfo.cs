using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Saber.Frame;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Wnd_CharacterInfo : WndBase
        , Widget_SlotPreview.IHandler
        , Widget_BagItemSlot.IHandler
        , Widget_TheurgyItemSlot.IHandler
    {
        public interface IHandler : IWndHandler
        {
            void OnClickExitGame();
            void OnSlotChange();
        }


        [SerializeField] private GameObject m_Background;
        [SerializeField] private Button m_ButtonClose;
        [SerializeField] private Button m_ButtonExitGame;
        [SerializeField] private Widget_Bag m_Bag;
        [SerializeField] private ToggleGroup m_ToggleGroup;
        [SerializeField] private GameObject m_SlotRoot;
        [SerializeField] private GameObject m_TempSlotPreview;
        [SerializeField] private SpriteAtlas m_AtlasPropIcons;
        [SerializeField] private SpriteAtlas m_AtlasSkillIcons;
        [SerializeField] private Widget_Theurgy m_Theurgy;


        private IHandler m_Handler;
        private Widget_SlotPreview[] m_SlotObjects;


        public override EWindowMode WindowMode => EWindowMode.Exclusive;


        protected override void OnAwake()
        {
            base.OnAwake();
            m_Handler = base.m_WndHandler as IHandler;

            m_ButtonExitGame.onClick.AddListener(OnClickExitGameButton);
            m_ButtonClose.onClick.AddListener(Destroy);

            GameApp.Entry.Game.Audio.Play2DSound("Sound/UI/ActorInfoWndOpen");

            // 槽位预览
            m_TempSlotPreview.gameObject.SetActive(false);
            m_SlotObjects = new Widget_SlotPreview[10];
            for (int i = 0; i < m_SlotObjects.Length; i++)
            {
                GameObject go = GameObject.Instantiate(m_TempSlotPreview, m_TempSlotPreview.transform.parent);
                go.transform.localPosition = Vector3.zero;
                go.SetActive(true);
                m_SlotObjects[i] = go.GetComponent<Widget_SlotPreview>();
                var slotData = GameApp.Entry.Game.ProgressMgr.SlotsArray[i];
                m_SlotObjects[i].Init(this, slotData, m_AtlasPropIcons, m_AtlasSkillIcons);
            }

            // 初始化物品，法术页
            m_Bag.Init(this, Destroy);
            m_Theurgy.Init(this, Destroy);

            // 当Toggle切换
            Toggle[] toggles = m_ToggleGroup.GetComponentsInChildren<Toggle>();
            for (int i = 0; i < toggles.Length; i++)
            {
                int index = i;
                toggles[i].onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        m_SlotRoot.SetActive(index == 0 || index == 1);
                        m_Background.SetActive(index != 2);
                    }
                });
            }
        }

        public override void Destroy()
        {
            GameApp.Entry.Game.Audio.Play2DSound("Sound/UI/ActorInfoWndClose");
            base.Destroy();
        }

        void OnClickExitGameButton()
        {
            //Destroy();
            GameApp.Entry.Game.Audio.PlayCommonClick();
            m_Handler.OnClickExitGame();
        }

        void Widget_SlotPreview.IHandler.OnEndDrag(Widget_SlotPreview self)
        {
            Widget_SlotPreview tarSlot = null;
            float minDistance = float.MaxValue;
            foreach (var s in m_SlotObjects)
            {
                float distance = (self.Icon.transform.position - s.IconPosition).magnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    tarSlot = s;
                }
            }

            if (tarSlot != null && minDistance < tarSlot.MaxAttackDistance)
            {
                if (tarSlot != self)
                {
                    self.SlotData.SwitchData(tarSlot.SlotData);
                    tarSlot.Reset();
                }
            }
            else
            {
                float distance = (self.Icon.transform.position - self.IconPosition).magnitude;
                if (distance > self.MaxAttackDistance)
                {
                    self.SlotData.Clear();
                }
            }

            self.Reset();

            m_Handler.OnSlotChange();
        }

        void Widget_BagItemSlot.IHandler.OnEndDrag(Widget_BagItemSlot self)
        {
            Widget_SlotPreview tarSlot = null;
            float minDistance = float.MaxValue;
            foreach (var s in m_SlotObjects)
            {
                float distance = (self.Icon.transform.position - s.IconPosition).magnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    tarSlot = s;
                }
            }

            if (tarSlot != null && minDistance < tarSlot.MaxAttackDistance)
            {
                tarSlot.SlotData.m_SlotType = Widget_SlotObject.ESlotDataType.PropItem;
                tarSlot.SlotData.m_ID = self.Item.ID;
                tarSlot.Reset();

                foreach (var s in m_SlotObjects)
                {
                    if (s != tarSlot && s.SlotData.IsEqual(tarSlot.SlotData))
                    {
                        s.SlotData.Clear();
                        s.Reset();
                    }
                }
            }

            self.ResetIconLocation();

            m_Handler.OnSlotChange();
        }

        void Widget_TheurgyItemSlot.IHandler.OnEndDrag(Widget_TheurgyItemSlot self)
        {
            Widget_SlotPreview tarSlot = null;
            float minDistance = float.MaxValue;
            foreach (var s in m_SlotObjects)
            {
                float distance = (self.Icon.transform.position - s.IconPosition).magnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    tarSlot = s;
                }
            }

            if (tarSlot != null && minDistance < tarSlot.MaxAttackDistance)
            {
                tarSlot.SlotData.m_SlotType = Widget_SlotObject.ESlotDataType.TheurgyItem;
                tarSlot.SlotData.m_ID = self.Item.m_ID;
                tarSlot.Reset();

                foreach (var s in m_SlotObjects)
                {
                    if (s != tarSlot && s.SlotData.IsEqual(tarSlot.SlotData))
                    {
                        s.SlotData.Clear();
                        s.Reset();
                    }
                }
            }

            self.ResetIconLocation();

            m_Handler.OnSlotChange();
        }

#if UNITY_EDITOR
        protected override void Update()
        {
            base.Update();
            UpdatePCInput();
        }

        void UpdatePCInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                m_ButtonClose.OnSubmit(null);
            }
        }
#endif
    }
}