using System;
using System.Collections;
using Saber.Frame;
using Saber.CharacterController;
using UnityEngine;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Widget_SkillButton : WidgetBase
    {
        public interface IHandler
        {
            void OnClickSkillButton(ESkillType type);
        }

        [SerializeField] public Button m_Button;
        [SerializeField] private GameObject m_Icon;
        [SerializeField] private GameObject m_IconGray;
        [SerializeField] private GameObject m_Cooldown;
        [SerializeField] private Text m_TextCooldown;
        [SerializeField] private Image m_ImageCooldown;

        private BaseSkill m_SkillObj;
        private ESkillType m_Type;
        private bool m_ToUpdateCD;
        private IHandler m_Handler;


        bool IsPowerEnough => GameApp.Entry.Game.Player.CStats.CurrentPower >= m_SkillObj.SkillConfig.m_CostPower;

        public void Init(ESkillType type, IHandler handler)
        {
            m_Type = type;
            m_Handler = handler;
            GameApp.Entry.Unity.StartCoroutine(InitItor());
        }

        IEnumerator InitItor()
        {
            while (true)
            {
                if (GameApp.Entry.Game.Player)
                {
                    m_SkillObj = GameApp.Entry.Game.Player.CMelee.GetSkillObject(m_Type);
                    if (m_SkillObj == null)
                    {
                        gameObject.SetActive(false);
                        yield break;
                    }

                    m_SkillObj.OnSkillTrigger += OnSkillTrigger;
                    OnSkillTrigger();

                    GameApp.Entry.Game.Player.CStats.OnPowerChange += OnPowerChange;
                    OnPowerChange();
                    yield break;
                }

                yield return null;
            }
        }

        private void OnSkillTrigger()
        {
            m_ToUpdateCD = true;
        }

        private void OnClickSkill()
        {
            if (!IsPowerEnough)
            {
                GameApp.Entry.Game.Audio.PlaySoundSkillFailed();
                GameApp.Entry.UI.ShowTips("能量不足", 0.1f);
                return;
            }

            if (!m_SkillObj.IsCDCooldown)
            {
                GameApp.Entry.Game.Audio.PlaySoundSkillFailed();
                GameApp.Entry.UI.ShowTips("技能正在冷却中", 0.1f);
                return;
            }

            m_Handler.OnClickSkillButton(m_SkillObj.SkillConfig.m_SkillType);
        }

        private void OnPowerChange()
        {
            bool powerEnough = IsPowerEnough;
            m_Icon.SetActive(powerEnough);
            m_IconGray.SetActive(!powerEnough);
        }

        protected override void Awake()
        {
            base.Awake();
            m_Button.onClick.AddListener(OnClickSkill);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (GameApp.Entry.Game.Player)
            {
                GameApp.Entry.Game.Player.CStats.OnPowerChange -= OnPowerChange;
            }

            if (m_SkillObj != null)
            {
                m_SkillObj.OnSkillTrigger -= OnSkillTrigger;
            }
        }

        protected override void Update()
        {
            base.Update();
            UpdateCD();
        }

        void UpdateCD()
        {
            if (m_SkillObj == null)
            {
                return;
            }

            if (!m_ToUpdateCD)
                return;

            if (m_SkillObj.IsCDCooldown)
            {
                m_Cooldown.SetActive(false);
                m_ToUpdateCD = false;
                return;
            }

            m_Cooldown.SetActive(true);
            m_ImageCooldown.fillAmount = m_SkillObj.CDProgress;
            m_TextCooldown.text = Mathf.CeilToInt(m_SkillObj.CDLeftSeconds).ToString();
        }
    }
}