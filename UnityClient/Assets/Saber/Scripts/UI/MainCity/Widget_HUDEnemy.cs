using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;
using UnityEngine.UI;
using Saber.CharacterController;

namespace Saber.UI
{
    public class Widget_HUDEnemy : WidgetBase
    {
        private const float k_HPPerSlider = 500;

        [SerializeField] private GameObject m_Root;
        [SerializeField] private Widget_HPBar m_HpBar;
        [SerializeField] private Transform m_SliderParent;
        [SerializeField] private Text m_TextName;
        [SerializeField] private float m_SliderSpaceY = 45;

        private SActor m_LockEnemy;
        private List<Widget_HPBar> m_HpBars = new();
        private int m_SliderCount;
        private float m_HPOfSmoothSlider;

        bool IsShow
        {
            set => m_Root.SetActive(value);
        }


        protected override void Start()
        {
            base.Start();
            m_HpBar.gameObject.SetActive(false);
        }

        SActor GetLockEnemy()
        {
            var p = GameApp.Entry.Game.Player;
            if (p != null && p.AI != null && p.AI.LockingEnemy != null)
            {
                return p.AI.LockingEnemy;
            }

            return null;
        }

        protected override void Update()
        {
            base.Update();
            SActor currentLockEnemy = GetLockEnemy();
            if (currentLockEnemy == null)
            {
                IsShow = false;
                m_LockEnemy = null;
                return;
            }

            IsShow = true;

            if (m_LockEnemy != currentLockEnemy)
            {
                m_LockEnemy = currentLockEnemy;
                InitEnemy();
            }

            UpdateSliders();
        }

        void InitEnemy()
        {
            m_SliderCount = Mathf.CeilToInt(m_LockEnemy.CStats.MaxHp / k_HPPerSlider);
            for (int i = 0; i < m_SliderCount; i++)
            {
                Widget_HPBar hpBar;
                if (i < m_HpBars.Count)
                {
                    hpBar = m_HpBars[i];
                }
                else
                {
                    GameObject go = GameObject.Instantiate(m_HpBar.gameObject);
                    go.SetActive(true);
                    go.transform.SetParent(m_SliderParent);
                    go.transform.localPosition = new Vector3(0, -m_SliderSpaceY * i);
                    go.transform.localScale = Vector3.one;
                    go.name = i.ToString();

                    hpBar = go.GetComponent<Widget_HPBar>();
                    m_HpBars.Add(hpBar);
                }

                RectTransform rectTransform = hpBar.GetComponent<RectTransform>();
                float thisSliderMaxHP;
                if (i == m_SliderCount - 1)
                {
                    float left = m_LockEnemy.CStats.MaxHp % k_HPPerSlider;
                    thisSliderMaxHP = left == 0 ? k_HPPerSlider : left;
                }
                else
                {
                    thisSliderMaxHP = k_HPPerSlider;
                }

                hpBar.Init(thisSliderMaxHP, thisSliderMaxHP);
                rectTransform.sizeDelta = new Vector2(thisSliderMaxHP * 3, rectTransform.sizeDelta.y);
            }

            for (int i = m_SliderCount; i < m_HpBars.Count; i++)
            {
                m_HpBars[i].gameObject.SetActive(false);
            }

            m_TextName.text = m_LockEnemy.BaseInfo.m_Name;
            m_HPOfSmoothSlider = m_LockEnemy.CStats.CurrentHp;
        }

        void UpdateSliders()
        {
            float currentHP = m_LockEnemy.CStats.CurrentHp;
            for (int i = 0; i < m_SliderCount; i++)
            {
                Widget_HPBar hpBar = m_HpBars[i];
                float hpThisSlider = currentHP - k_HPPerSlider * i;
                hpThisSlider = Mathf.Clamp(hpThisSlider, 0, k_HPPerSlider);
                hpBar.CurHp = hpThisSlider;
            }
        }
    }
}