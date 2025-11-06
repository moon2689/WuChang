using System;
using System.Collections;
using System.Linq;
using DuloGames.UI;
using Saber.Frame;
using Saber.CharacterController;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Widget_SlotPreview : WidgetBase
    {
        public interface IHandler
        {
            void OnEndDrag(Widget_SlotPreview self);
        }

        [SerializeField] private Image m_Icon;
        [SerializeField] private UIDragableObject m_DragObject;
        [SerializeField] private Transform m_IconParent;

        private IHandler m_IHandler;
        private MainWndSlotData m_MainWndSlotData;
        private bool m_ToUpdateCD;
        private SpriteAtlas m_AtlasProp, m_AtlasSkill;
        private float m_TimerUseProp;

        private bool IsValid => m_MainWndSlotData.m_SlotType != Widget_SlotObject.ESlotDataType.None;
        public Image Icon => m_Icon;
        public MainWndSlotData SlotData => m_MainWndSlotData;
        public Vector3 IconPosition => m_IconParent.position;
        public float MaxAttackDistance => m_Icon.rectTransform.rect.width * 1.414f;


        protected override void Awake()
        {
            base.Awake();
            m_DragObject.onBeginDrag.AddListener(OnBeginDrag);
            m_DragObject.onEndDrag.AddListener(OnEndDrag);
        }

        private void OnBeginDrag(BaseEventData arg0)
        {
            m_Icon.transform.SetParent(ParentWnd.transform);
        }

        private void OnEndDrag(BaseEventData arg0)
        {
            m_DragObject.StopMovement();
            m_IHandler.OnEndDrag(this);
        }


        public void Init(IHandler handler, MainWndSlotData slotData, SpriteAtlas atlasProp, SpriteAtlas atlasSkill)
        {
            m_IHandler = handler;
            m_MainWndSlotData = slotData;
            m_AtlasProp = atlasProp;
            m_AtlasSkill = atlasSkill;
            Reset();
        }

        public void Reset()
        {
            m_Icon.gameObject.SetActive(IsValid);
            m_Icon.transform.SetParent(m_IconParent);
            m_Icon.rectTransform.offsetMin = Vector2.zero;
            m_Icon.rectTransform.offsetMax = Vector2.zero;

            if (m_MainWndSlotData.m_SlotType == Widget_SlotObject.ESlotDataType.TheurgyItem)
            {
                var config = GameApp.Entry.Config.TheurgyInfo.m_TheurgyArray.FirstOrDefault(a => a.m_ID == m_MainWndSlotData.m_ID);
                if (config != null)
                    m_Icon.sprite = m_AtlasSkill.GetSprite(config.m_Icon);
            }
            else if (m_MainWndSlotData.m_SlotType == Widget_SlotObject.ESlotDataType.PropItem)
            {
                var propConfig = GameApp.Entry.Game.Bag.GetItemConfig(m_MainWndSlotData.m_ID);
                if (propConfig != null)
                    m_Icon.sprite = m_AtlasProp.GetSprite(propConfig.m_Icon);
            }
        }
    }
}