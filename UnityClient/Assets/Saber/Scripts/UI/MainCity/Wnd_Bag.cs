using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Saber.Frame;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Wnd_Bag : WndBase
    {
        [SerializeField] private Button m_ButtonMask;
        [SerializeField] private Button m_ButtonClose;
        [SerializeField] private Button m_ButtonUse;
        [SerializeField] private SpriteAtlas m_AtlasIcons;
        [SerializeField] private Widget_BagItemSlot m_TempSlot;
        [SerializeField] private Text m_TextDescription;
        [SerializeField] private BackAndForthWindow m_BackAndForthWindow;

        private static int s_LastSelectedPropID;
        private Widget_BagItemSlot[] m_Slots;

        private Widget_BagItemSlot m_SelctedSlot;
        //protected override bool PauseGame => true;

        protected override void Awake()
        {
            base.Awake();
            m_BackAndForthWindow.Show();
            m_BackAndForthWindow.OnHide = OnHide;
        }

        private void OnHide()
        {
            Destroy();
        }

        protected override void Start()
        {
            base.Start();
            m_ButtonMask.onClick.AddListener(AnimHide);
            m_ButtonClose.onClick.AddListener(AnimHide);
            m_ButtonUse.onClick.AddListener(OnClickUse);

            m_TextDescription.text = "";
            m_ButtonUse.gameObject.SetActive(false);

            InitSlots();
            Reset();
        }

        void AnimHide()
        {
            m_BackAndForthWindow.Hide();
        }

        void InitSlots()
        {
            m_Slots = new Widget_BagItemSlot[36];
            for (int i = 0; i < m_Slots.Length; i++)
            {
                GameObject go = GameObject.Instantiate<GameObject>(m_TempSlot.gameObject);
                go.transform.SetParent(m_TempSlot.transform.parent);
                Widget_BagItemSlot slot = go.GetComponent<Widget_BagItemSlot>();
                slot.name = (i + 1).ToString();
                slot.OnSelected = OnSelectedSlot;
                slot.gameObject.SetActive(true);
                m_Slots[i] = slot;
            }

            m_TempSlot.gameObject.SetActive(false);
        }

        private void OnSelectedSlot(Widget_BagItemSlot slot)
        {
            m_SelctedSlot = slot;
            if (slot.IsNotEmpty)
            {
                var itemConfig = slot.Item.Config;
                s_LastSelectedPropID = itemConfig.m_ID;
                m_TextDescription.text = itemConfig.m_Name + " " + itemConfig.m_Description;
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
                GameApp.Entry.Game.Bag.UseItem(m_SelctedSlot.Item.ID);
            }

            AnimHide();
        }

        void Reset()
        {
            Widget_BagItemSlot selectedSlot = null;
            for (int i = 0; i < m_Slots.Length; i++)
            {
                PlayerBag.Item item = GameApp.Entry.Game.Bag.GetItemByIndex(i);
                m_Slots[i].Reset(item, m_AtlasIcons);

                if (item != null && item.ID == s_LastSelectedPropID)
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
                else
                    m_ButtonClose.OnSubmit(null);
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                m_ButtonClose.OnSubmit(null);
            }

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.W))
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
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
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