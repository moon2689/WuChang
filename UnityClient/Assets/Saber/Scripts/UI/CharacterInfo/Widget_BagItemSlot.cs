using System;
using System.Collections;
using DuloGames.UI;
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
        public interface IHandler
        {
            void OnEndDrag(Widget_BagItemSlot self);
        }

        public Action<Widget_BagItemSlot> OnSelected;

        [SerializeField] private Button m_ButtonSelf;
        [SerializeField] private GameObject m_Root;
        [SerializeField] private Image m_ImageIcon;
        [SerializeField] private Image m_ImageIconBake;
        [SerializeField] private Text m_TextCount;
        [SerializeField] private GameObject m_Selected;
        [SerializeField] private UIDragableObject m_DragObject;

        private PlayerBag.Item m_Item;
        private IHandler m_IHandler;

        public PlayerBag.Item Item => m_Item;
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
            m_DragObject.StopMovement();
        }

        private void OnClickSelf()
        {
            OnSelected?.Invoke(this);
        }

        public void Reset(PlayerBag.Item item, SpriteAtlas atlas, IHandler handler)
        {
            m_Item = item;
            m_IHandler = handler;
            m_Root.SetActive(item != null);
            if (item == null)
            {
                return;
            }

            ResetIconLocation();
            m_ImageIconBake.sprite = m_ImageIcon.sprite = atlas.GetSprite(item.Config.m_Icon);
            m_ImageIconBake.gameObject.SetActive(false);
            m_TextCount.text = item.Count.ToString();
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