using System.Collections;
using System.Collections.Generic;
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

        private Widget_BagItemSlot[] m_Slots;
        private Widget_BagItemSlot m_SelctedSlot;
        protected override bool PauseGame => true;

        protected override void Start()
        {
            base.Start();
            m_ButtonMask.onClick.AddListener(Destroy);
            m_ButtonClose.onClick.AddListener(Destroy);
            m_ButtonUse.onClick.AddListener(OnClickUse);

            m_TextDescription.text = "";
            m_ButtonUse.gameObject.SetActive(false);

            InitSlots();
            Reset();
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

            Destroy();
        }

        void Reset()
        {
            for (int i = 0; i < m_Slots.Length; i++)
            {
                PlayerBag.Item item = GameApp.Entry.Game.Bag.GetItemByIndex(i);
                m_Slots[i].Reset(item, m_AtlasIcons);
            }
        }
    }
}