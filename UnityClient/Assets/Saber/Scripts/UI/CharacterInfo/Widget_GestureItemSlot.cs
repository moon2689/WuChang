using System;
using System.Collections;
using DuloGames.UI;
using Saber.Frame;
using Saber.CharacterController;
using Saber.Config;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Widget_GestureItemSlot : WidgetBase
    {
        public Action<Widget_GestureItemSlot> OnSelected;

        [SerializeField] private Button m_ButtonSelf;
        [SerializeField] private GameObject m_Root;
        [SerializeField] private Image m_ImageIcon;
        [SerializeField] private GameObject m_Selected;
        [SerializeField] private SpriteAtlas m_AtlasIcons;

        private GestureItemInfo m_Item;

        public GestureItemInfo Item => m_Item;
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

        public void Reset(GestureItemInfo item)
        {
            m_Item = item;
            m_Root.SetActive(item != null);
            if (item == null)
            {
                return;
            }

            ResetIconLocation();
            m_ImageIcon.sprite = m_AtlasIcons.GetSprite(item.m_Icon);
            SetSelected(false);
        }

        public void ResetIconLocation()
        {
            m_ImageIcon.transform.SetParent(m_Root.transform);
            m_ImageIcon.rectTransform.offsetMin = Vector2.zero;
            m_ImageIcon.rectTransform.offsetMax = Vector2.zero;
        }

        public void SetSelected(bool selected)
        {
            if (IsNotEmpty)
                m_Selected.SetActive(selected);
        }
    }
}