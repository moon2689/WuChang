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
    public class Widget_TheurgyItemSlot : WidgetBase
    {
        public interface IHandler
        {
            void OnEndDrag(Widget_TheurgyItemSlot self);
        }

        public Action<Widget_TheurgyItemSlot> OnSelected;

        [SerializeField] private Button m_ButtonSelf;
        [SerializeField] private GameObject m_Root;
        [SerializeField] private Image m_ImageIcon;
        [SerializeField] private Image m_ImageIconBake;
        [SerializeField] private GameObject m_Selected;
        [SerializeField] private UIDragableObject m_DragObject;

        private TheurgyItemInfo m_Item;
        private IHandler m_IHandler;

        public TheurgyItemInfo Item => m_Item;
        public bool IsNotEmpty => Item != null;
        public Image Icon => m_ImageIcon;


        protected override void Awake()
        {
            base.Awake();
            m_ButtonSelf.onClick.AddListener(OnClickSelf);
            m_DragObject.onBeginDrag.AddListener(OnBeginDrag);
            m_DragObject.onEndDrag.AddListener(OnEndDrag);
        }

        private void OnBeginDrag(BaseEventData arg0)
        {
            m_ImageIcon.transform.SetParent(ParentWnd.transform);
            m_ImageIconBake.gameObject.SetActive(true);
        }

        private void OnEndDrag(BaseEventData arg0)
        {
            m_IHandler.OnEndDrag(this);
            m_ImageIconBake.gameObject.SetActive(false);
        }

        private void OnClickSelf()
        {
            OnSelected?.Invoke(this);
        }

        public void Reset(TheurgyItemInfo item, SpriteAtlas atlas, IHandler handler)
        {
            m_Item = item;
            m_IHandler = handler;
            m_Root.SetActive(item != null);
            if (item == null)
            {
                return;
            }

            ResetIconLocation();
            m_ImageIconBake.sprite = m_ImageIcon.sprite = atlas.GetSprite(item.m_Icon);
            m_ImageIconBake.gameObject.SetActive(false);
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