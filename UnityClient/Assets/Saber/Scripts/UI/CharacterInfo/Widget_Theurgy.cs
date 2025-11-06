using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Saber.Config;
using UnityEngine;
using Saber.Frame;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Widget_Theurgy : WidgetBase
    {
        [SerializeField] private Button m_ButtonUse;
        [SerializeField] private SpriteAtlas m_AtlasIcons;
        [SerializeField] private Widget_TheurgyItemSlot m_TempSlot;
        [SerializeField] private Text m_TextDescription;

        private static int s_LastSelectedID;
        private Widget_TheurgyItemSlot[] m_Slots;
        private Widget_TheurgyItemSlot m_SelctedSlot;
        private Action m_ActionCloseWnd;


        protected override void Awake()
        {
            base.Awake();
            m_ButtonUse.onClick.AddListener(OnClickUse);

            //InitSlots();
            //Reset();
        }

        void InitSlots()
        {
            m_Slots = new Widget_TheurgyItemSlot[25];
            for (int i = 0; i < m_Slots.Length; i++)
            {
                GameObject go = GameObject.Instantiate<GameObject>(m_TempSlot.gameObject);
                go.transform.SetParent(m_TempSlot.transform.parent);
                Widget_TheurgyItemSlot slot = go.GetComponent<Widget_TheurgyItemSlot>();
                slot.name = (i + 1).ToString();
                slot.OnSelected = OnSelectedSlot;
                slot.gameObject.SetActive(true);
                m_Slots[i] = slot;
            }

            m_TempSlot.gameObject.SetActive(false);
        }

        private void OnSelectedSlot(Widget_TheurgyItemSlot slot)
        {
            m_SelctedSlot = slot;
            if (slot.IsNotEmpty)
            {
                var itemConfig = slot.Item;
                s_LastSelectedID = itemConfig.m_ID;
                m_TextDescription.text = itemConfig.m_Name + "\n" + itemConfig.m_Description;
            }
            else
            {
                m_TextDescription.text = "";
            }

            for (int i = 0; i < m_Slots.Length; i++)
            {
                var s = m_Slots[i];
                if (s.IsNotEmpty)
                    s.SetSelected(slot == s);
            }

            m_ButtonUse.gameObject.SetActive(slot.IsNotEmpty);
        }

        private void OnClickUse()
        {
            if (m_SelctedSlot != null && m_SelctedSlot.IsNotEmpty)
            {
                var skillObj = GameApp.Entry.Game.Player.CMelee[m_SelctedSlot.Item.m_SkillID];
                if (skillObj != null)
                {
                    GameApp.Entry.Game.PlayerAI.TryTriggerSkill(skillObj.SkillConfig.m_SkillType);
                }
            }

            m_ActionCloseWnd?.Invoke();
        }

        public void Init(Widget_TheurgyItemSlot.IHandler slotHandler, Action closeWnd)
        {
            m_ActionCloseWnd = closeWnd;
            InitSlots();
            Reset(slotHandler);
        }

        void Reset(Widget_TheurgyItemSlot.IHandler slotHandler)
        {
            Widget_TheurgyItemSlot selectedSlot = null;
            for (int i = 0; i < m_Slots.Length; i++)
            {
                TheurgyItemInfo item = null;
                if (i < GameApp.Entry.Config.TheurgyInfo.m_TheurgyArray.Length)
                {
                    item = GameApp.Entry.Config.TheurgyInfo.m_TheurgyArray[i];
                }

                m_Slots[i].Reset(item, m_AtlasIcons, slotHandler);

                if (item != null && item.m_ID == s_LastSelectedID)
                {
                    selectedSlot = m_Slots[i];
                }
            }

            if (selectedSlot == null)
            {
                selectedSlot = m_Slots.FirstOrDefault();
            }

            if (selectedSlot != null)
            {
                OnSelectedSlot(selectedSlot);
            }
        }

#if UNITY_EDITOR
        protected override void Update()
        {
            base.Update();
            UpdatePCInput();
        }

        void UpdatePCInput()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (m_ButtonUse.gameObject.activeSelf)
                    m_ButtonUse.OnSubmit(null);
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                int curIndex = Array.FindIndex(m_Slots, a => a == m_SelctedSlot);
                --curIndex;
                int maxIndex = Mathf.Min(GameApp.Entry.Game.Bag.Items.Count - 1, m_Slots.Length - 1);
                if (curIndex < 0)
                {
                    curIndex = maxIndex;
                }

                OnSelectedSlot(m_Slots[curIndex]);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                int curIndex = Array.FindIndex(m_Slots, a => a == m_SelctedSlot);
                ++curIndex;
                int maxIndex = Mathf.Min(GameApp.Entry.Game.Bag.Items.Count - 1, m_Slots.Length - 1);
                if (curIndex > maxIndex)
                {
                    curIndex = 0;
                }

                OnSelectedSlot(m_Slots[curIndex]);
            }
        }
#endif
    }
}