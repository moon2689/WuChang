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
        [SerializeField] private Slider m_HealthSlider;
        [SerializeField] private Transform m_SliderParent;
        [SerializeField] private Text m_TextName;
        [SerializeField] private float m_SliderSpaceY = 45;

        private SActor m_LockEnemy;
        private List<Slider> m_Sliders = new();
        private List<Slider> m_SmoothSliders = new();
        private int m_SliderCount;
        private float m_HPOfSmoothSlider;
        private float m_TimerHealthSmoothChange;
        private float m_OldHpRatio;

        bool IsShow
        {
            set => m_Root.SetActive(value);
        }


        protected override void Start()
        {
            base.Start();
            m_HealthSlider.gameObject.SetActive(false);
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
                Slider slider, smoothSlider;
                if (i < m_Sliders.Count)
                {
                    slider = m_Sliders[i];
                    smoothSlider = m_SmoothSliders[i];
                }
                else
                {
                    GameObject go = GameObject.Instantiate(m_HealthSlider.gameObject);
                    go.SetActive(true);
                    go.transform.SetParent(m_SliderParent);
                    go.transform.localPosition = new Vector3(0, -m_SliderSpaceY * i);
                    go.transform.localScale = Vector3.one;
                    go.name = i.ToString();

                    slider = go.GetComponent<Slider>();
                    m_Sliders.Add(slider);

                    smoothSlider = go.transform.Find("SmoothSliderHealth").GetComponent<Slider>();
                    m_SmoothSliders.Add(smoothSlider);
                }

                RectTransform rectTransform = slider.GetComponent<RectTransform>();
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

                slider.maxValue = thisSliderMaxHP;
                smoothSlider.maxValue = thisSliderMaxHP;
                rectTransform.sizeDelta = new Vector2(thisSliderMaxHP * 3, rectTransform.sizeDelta.y);
            }

            for (int i = m_SliderCount; i < m_Sliders.Count; i++)
            {
                m_Sliders[i].gameObject.SetActive(false);
            }

            m_TextName.text = m_LockEnemy.BaseInfo.m_Name;
            m_HPOfSmoothSlider = m_LockEnemy.CStats.CurrentHp;
            m_OldHpRatio = m_LockEnemy.CStats.CurrentHPRatio;
        }

        void UpdateSliders()
        {
            float currentHP = m_LockEnemy.CStats.CurrentHp;
            for (int i = 0; i < m_SliderCount; i++)
            {
                Slider slider = m_Sliders[i];
                float hpThisSlider = currentHP - k_HPPerSlider * i;
                hpThisSlider = Mathf.Clamp(hpThisSlider, 0, k_HPPerSlider);
                slider.value = hpThisSlider;
            }

            // 第二血条延迟，平滑跟随
            if (m_HPOfSmoothSlider > currentHP)
            {
                if (m_TimerHealthSmoothChange > 1.5f)
                {
                    float target = m_HPOfSmoothSlider - 10 * Time.deltaTime;
                    if (target < currentHP)
                    {
                        target = currentHP;
                    }

                    m_HPOfSmoothSlider = target;
                }
                else
                {
                    m_TimerHealthSmoothChange += Time.deltaTime;
                    if (m_OldHpRatio != m_LockEnemy.CStats.CurrentHPRatio)
                    {
                        m_TimerHealthSmoothChange = 0;
                        m_OldHpRatio = m_LockEnemy.CStats.CurrentHPRatio;
                    }
                }
            }
            else
            {
                m_TimerHealthSmoothChange = 0;
                if (m_HPOfSmoothSlider < currentHP)
                {
                    m_HPOfSmoothSlider = currentHP;
                }
            }

            for (int i = 0; i < m_SliderCount; i++)
            {
                Slider slider = m_SmoothSliders[i];
                float hpThisSlider = m_HPOfSmoothSlider - k_HPPerSlider * i;
                hpThisSlider = Mathf.Clamp(hpThisSlider, 0, k_HPPerSlider);
                slider.value = hpThisSlider;
            }
        }
    }
}