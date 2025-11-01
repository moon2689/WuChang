using System;
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
        [SerializeField] private Widget_HPBar m_HpBar;
        [SerializeField] private Image m_ImageStamina;
        [SerializeField] private RawImage m_RawImagePower;
        [SerializeField] private Image m_ImageDamage;

        //[SerializeField] private RawImage m_playerIcon;

        private SActor m_Actor;


        void Init()
        {
            m_Actor.Event_OnDead += OnDead;
            m_Actor.Event_OnDamage += EnableDamageSprite;
            m_ImageDamage.color = new Color(0f, 0f, 0f, 0f);
            m_HpBar.Init(m_Actor.CStats.MaxHp, m_Actor.CStats.CurrentHp);
            //m_OldHpRatio = m_Actor.CStats.CurrentHPRatio;

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
            m_HpBar.CurHp = m_Actor.CStats.CurrentHp;
            m_ImageStamina.fillAmount = m_Actor.CStats.CurrentStamina / m_Actor.CStats.MaxStamina;

            // m_HealthText.text = m_Actor.CStats.CurrentHPInt.ToString();
            // m_StaminaText.text = m_Actor.CStats.CurrentStaminaInt.ToString();
        }

        void RefreshPowerPoints()
        {
            int curPower = m_Actor.CStats.CurrentPower;
            int maxPower = m_Actor.CStats.MaxPower;

            int index; //从左下角开始计算
            if (maxPower == 1)
            {
                index = curPower switch
                {
                    0 => 15,
                    1 => 10,
                };
            }
            else if (maxPower == 2)
            {
                index = curPower switch
                {
                    0 => 16,
                    1 => 11,
                    2 => 5,
                };
            }
            else if (maxPower == 3)
            {
                index = curPower switch
                {
                    0 => 17,
                    1 => 12,
                    2 => 6,
                    3 => 9,
                };
            }
            else if (maxPower == 4)
            {
                index = curPower switch
                {
                    0 => 18,
                    1 => 13,
                    2 => 7,
                    3 => 0,
                    4 => 2,
                };
            }
            else if (maxPower == 5)
            {
                index = curPower switch
                {
                    0 => 19,
                    1 => 14,
                    2 => 8,
                    3 => 1,
                    4 => 3,
                    5 => 4,
                };
            }
            else
            {
                throw new InvalidOperationException("最多只支持5格能量");
            }

            int rowCount = 4;
            int columnCount = 5;
            float cellWidth = 1f / columnCount;
            float cellHeight = 1f / rowCount;
            int cellX = index % columnCount;
            int cellY = Mathf.FloorToInt(index / columnCount);
            float x = cellX * cellWidth;
            float y = cellY * cellHeight;
            m_RawImagePower.uvRect = new Rect(x, y, cellWidth, cellHeight);
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