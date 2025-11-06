using System;
using System.Collections;
using System.Linq;
using Saber.Frame;
using Saber.CharacterController;
using Saber.Config;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Widget_SlotObject : WidgetBase
    {
        private const float k_UsePropCDTime = 1;

        public enum ESlotDataType
        {
            None,
            PropItem,
            TheurgyItem,
        }

        public interface IHandler
        {
            void OnClickSlot(MainWndSlotData slotData);
        }

        [SerializeField] public Button m_Button;
        [SerializeField] private Image m_Icon;
        [SerializeField] private Image m_IconGray;

        [SerializeField] private GameObject m_RootCount;
        [SerializeField] private Text m_TextCount;

        [SerializeField] private GameObject m_RootPowerPoints;
        [SerializeField] private GameObject[] m_PowerPoints;

        [SerializeField] private GameObject m_RootCoolTime;
        [SerializeField] private Text m_TextCooldown;
        [SerializeField] private Image m_ImageCooldown;

        private MainWndSlotData m_MainWndSlotData;
        private BaseSkill m_SkillObj;
        private TheurgyItemInfo m_TheurgyItemInfo;
        private bool m_ToUpdateCD;
        private IHandler m_Handler;
        private SpriteAtlas m_AtlasProp, m_AtlasSkill;
        private float m_TimerUseProp;

        private bool IsValid => m_MainWndSlotData.m_SlotType != ESlotDataType.None;


        protected override void Awake()
        {
            base.Awake();
            m_Button.onClick.AddListener(ClickSlot);
        }

        public void ClickSlot()
        {
            if (IsValid)
                m_Handler.OnClickSlot(m_MainWndSlotData);
        }

        public void Init(MainWndSlotData slotData, IHandler handler, SpriteAtlas atlasProp, SpriteAtlas atlasSkill)
        {
            m_MainWndSlotData = slotData;
            m_Handler = handler;
            m_AtlasProp = atlasProp;
            m_AtlasSkill = atlasSkill;
            InitItor().StartCoroutine();
        }

        IEnumerator InitItor()
        {
            while (!GameApp.Entry.Game.Player)
            {
                yield return null;
            }

            Reset();
        }

        public void Reset()
        {
            m_Button.gameObject.SetActive(IsValid);

            if (m_MainWndSlotData.m_SlotType == ESlotDataType.TheurgyItem)
            {
                InitSkillData();
            }
            else if (m_MainWndSlotData.m_SlotType == ESlotDataType.PropItem)
            {
                InitPropData();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_MainWndSlotData != null)
            {
                if (m_MainWndSlotData.m_SlotType == ESlotDataType.TheurgyItem)
                {
                    if (GameApp.Entry.Game.Player)
                    {
                        GameApp.Entry.Game.Player.CStats.EventOnPowerChange -= OnPowerChanged;
                    }

                    if (m_SkillObj != null)
                    {
                        m_SkillObj.OnSkillTrigger -= OnSkillTrigger;
                    }
                }
                else if (m_MainWndSlotData.m_SlotType == ESlotDataType.PropItem)
                {
                    GameApp.Entry.Game.Bag.OnItemChange -= OnPropItemChange;
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            if (m_ToUpdateCD)
            {
                if (m_MainWndSlotData.m_SlotType == ESlotDataType.TheurgyItem)
                {
                    if (m_SkillObj != null)
                        UpdateSkillCD();
                }
                else if (m_MainWndSlotData.m_SlotType == ESlotDataType.PropItem)
                {
                    UpdateUsePropCD();
                }
            }
        }

        #region Prop

        void InitPropData()
        {
            m_RootCount.SetActive(true);
            m_RootPowerPoints.SetActive(false);
            m_RootCoolTime.SetActive(false);
            GameApp.Entry.Game.Bag.OnItemChange += OnPropItemChange;
            GameApp.Entry.Game.Bag.OnItemUse += OnUsePropItem;
            OnPropItemChange(null);

            var propConfig = GameApp.Entry.Game.Bag.GetItemConfig(m_MainWndSlotData.m_ID);
            m_Icon.sprite = m_IconGray.sprite = m_AtlasProp.GetSprite(propConfig.m_Icon);
        }

        private void OnUsePropItem(PlayerBag.Item obj)
        {
            if (obj.ID == m_MainWndSlotData.m_ID)
            {
                m_ToUpdateCD = true;
                m_TimerUseProp = k_UsePropCDTime;
            }
        }

        private void OnPropItemChange(PlayerBag.Item obj)
        {
            int count = GameApp.Entry.Game.Bag.GetItemCount(m_MainWndSlotData.m_ID);
            m_Icon.gameObject.SetActive(count > 0);
            m_IconGray.gameObject.SetActive(count <= 0);
            m_TextCount.text = count.ToString();
        }

        void UpdateUsePropCD()
        {
            if (m_TimerUseProp <= 0)
            {
                m_RootCoolTime.SetActive(false);
                m_ToUpdateCD = false;
                return;
            }

            m_TimerUseProp -= Time.deltaTime;

            m_RootCoolTime.SetActive(true);
            float fillAmount = m_TimerUseProp / k_UsePropCDTime;
            m_ImageCooldown.fillAmount = fillAmount;
            m_TextCooldown.text = Mathf.CeilToInt(1 - fillAmount).ToString();
        }

        #endregion

        #region Skill

        void InitSkillData()
        {
            m_SkillObj = GameApp.Entry.Game.Player.CMelee.GetTheurgy(m_MainWndSlotData.m_ID, out m_TheurgyItemInfo);
            if (m_SkillObj == null)
            {
                Debug.LogError($"m_SkillObj == null,id:{m_MainWndSlotData.m_ID}");
                return;
            }

            m_RootCount.SetActive(false);
            m_RootPowerPoints.SetActive(true);

            m_SkillObj.OnSkillTrigger += OnSkillTrigger;
            OnSkillTrigger();

            GameApp.Entry.Game.Player.CStats.EventOnPowerChange += OnPowerChanged;
            OnPowerChanged();

            for (int i = 0; i < m_PowerPoints.Length; i++)
            {
                m_PowerPoints[i].SetActive(i < m_SkillObj.SkillConfig.m_CostPower);
            }

            m_Icon.sprite = m_IconGray.sprite = m_AtlasSkill.GetSprite(m_TheurgyItemInfo.m_Icon);
        }

        private void OnSkillTrigger()
        {
            m_ToUpdateCD = true;
        }

        private void OnPowerChanged()
        {
            int curPower = GameApp.Entry.Game.Player.CStats.CurrentPower;
            bool canTrigger = m_SkillObj.SkillConfig.m_CanTriggerWhenPowerNotEnough || curPower >= m_SkillObj.SkillConfig.m_CostPower;
            m_Icon.gameObject.SetActive(canTrigger);
            m_IconGray.gameObject.SetActive(!canTrigger);

            for (int i = 0; i < m_PowerPoints.Length; i++)
            {
                bool active = i < m_SkillObj.SkillConfig.m_CostPower;
                m_PowerPoints[i].SetActive(active);
                if (active)
                {
                    m_PowerPoints[i].transform.GetChild(0).gameObject.SetActive(canTrigger);
                }
            }
        }


        void UpdateSkillCD()
        {
            if (m_SkillObj.IsCDCooldown)
            {
                m_RootCoolTime.SetActive(false);
                m_ToUpdateCD = false;
                return;
            }

            m_RootCoolTime.SetActive(true);
            m_ImageCooldown.fillAmount = m_SkillObj.CDProgress;
            m_TextCooldown.text = Mathf.CeilToInt(m_SkillObj.CDLeftSeconds).ToString();
        }

        #endregion
    }
}