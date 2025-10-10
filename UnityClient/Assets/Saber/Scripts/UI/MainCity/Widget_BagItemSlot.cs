using System;
using System.Collections;
using Saber.Frame;
using Saber.CharacterController;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Widget_BagItemSlot : WidgetBase
    {
        public Action<Widget_BagItemSlot> OnSelected;

        [SerializeField] private Button m_ButtonSelf;
        [SerializeField] private GameObject m_Root;
        [SerializeField] private Image m_ImageIcon;
        [SerializeField] private Text m_TextCount;
        [SerializeField] private GameObject m_Selected;

        private PlayerBag.Item m_Item;

        public PlayerBag.Item Item => m_Item;
        public bool IsNotEmpty => Item != null;

        protected override void Awake()
        {
            base.Awake();
            m_ButtonSelf.onClick.AddListener(OnClickSelf);
        }

        private void OnClickSelf()
        {
            OnSelected?.Invoke(this);
        }

        public void Reset(PlayerBag.Item item, SpriteAtlas atlas)
        {
            m_Item = item;
            m_Root.SetActive(item != null);
            if (item == null)
            {
                return;
            }

            m_ImageIcon.sprite = atlas.GetSprite(item.Config.m_Icon);
            m_TextCount.text = item.Count.ToString();
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (IsNotEmpty)
                m_Selected.SetActive(selected);
        }
    }
}