using System.Collections.Generic;
using DuloGames.UI;
using Saber.Frame;
using UnityEngine;
using UnityEngine.UI;
using Saber.CharacterController;

namespace Saber.UI
{
    public class Widget_HUD : WidgetBase
    {
        [SerializeField] private UIProgressBar m_BarHealth;
        [SerializeField] private Text m_HealthText;
        [SerializeField] private Image m_ImageHealthSmooth;
        [SerializeField] private UIProgressBar m_BarStamina;
        [SerializeField] private Text m_StaminaText;

        [SerializeField] private Image m_ImageDamage;
        [SerializeField] private GameObject m_PowerPoint;
        [SerializeField] private Transform m_PowerPointParent;

        private float m_TimerHealthSmoothChange;
        //[SerializeField] private RawImage m_playerIcon;

        private SActor m_Actor;
        private List<GameObject> m_PowerPoints = new();


        void Init()
        {
            m_Actor.Event_OnDead += OnDead;
            m_Actor.Event_OnDamage += EnableDamageSprite;
            m_ImageDamage.color = new Color(0f, 0f, 0f, 0f);

            // icon
            //m_playerIcon.texture = m_Actor.BaseInfo.LoadIcon();

            /*
            RectTransform rectTransHP = m_BarHealth.GetComponent<RectTransform>();
            rectTransHP.sizeDelta = new Vector2(m_Actor.CStats.MaxHp * 3, rectTransHP.sizeDelta.y);

            RectTransform rectTransStamina = m_BarStamina.GetComponent<RectTransform>();
            rectTransStamina.sizeDelta = new Vector2(m_Actor.CStats.MaxStamina, rectTransStamina.sizeDelta.y);
            */
        }

        void OnDead(SActor owner)
        {
            m_Actor = null;
        }

        protected override void Update()
        {
            base.Update();
            var p = GameApp.Entry.Game.Player;
            if (p != null && (m_Actor == null || m_Actor != p))
            {
                m_Actor = p;
                Init();
            }

            UpdateHUD();
        }

        void UpdateHUD()
        {
            if (m_Actor == null)
                return;

            UpdateSliders();
            ShowDamageSprite();
            RefreshPowerPoints();
        }

        void UpdateSliders()
        {
            m_BarHealth.fillAmount = m_Actor.CStats.CurrentHPRatio;
            m_BarStamina.fillAmount = m_Actor.CStats.CurrentStaminaRatio;

            m_HealthText.text = m_Actor.CStats.CurrentHPInt.ToString();
            m_StaminaText.text = m_Actor.CStats.CurrentStaminaInt.ToString();

            // 第二血条延迟，平滑跟随
            if (m_ImageHealthSmooth.fillAmount > m_BarHealth.fillAmount)
            {
                if (m_TimerHealthSmoothChange > 1.5f)
                {
                    float target = m_ImageHealthSmooth.fillAmount - 0.5f * Time.deltaTime;
                    if (target < m_BarHealth.fillAmount)
                    {
                        target = m_BarHealth.fillAmount;
                    }

                    m_ImageHealthSmooth.fillAmount = target;
                }
                else
                {
                    m_TimerHealthSmoothChange += Time.deltaTime;
                }
            }
            else
            {
                m_TimerHealthSmoothChange = 0;
                if (m_ImageHealthSmooth.fillAmount < m_BarHealth.fillAmount)
                {
                    m_ImageHealthSmooth.fillAmount = m_BarHealth.fillAmount;
                }
            }
        }

        void RefreshPowerPoints()
        {
            for (int i = 0; i < this.m_Actor.CStats.MaxPowerPointCount; i++)
            {
                GameObject go;
                if (i < m_PowerPoints.Count)
                {
                    go = m_PowerPoints[i];
                }
                else
                {
                    go = GameObject.Instantiate(m_PowerPoint);
                    m_PowerPoints.Add(go);
                    go.transform.SetParent(m_PowerPointParent);
                    go.SetActive(true);
                }

                go.transform.GetChild(0).gameObject.SetActive(i < this.m_Actor.CStats.CurrentPowerPointCount);
            }
        }

        void ShowDamageSprite()
        {
            if (m_ImageDamage.color != Color.clear)
                m_ImageDamage.color = Color.Lerp(m_ImageDamage.color, Color.clear, 3 * Time.deltaTime);
        }

        void EnableDamageSprite(SActor owner, float damage)
        {
            m_ImageDamage.enabled = true;
            m_ImageDamage.color = Color.white;
        }
    }
}