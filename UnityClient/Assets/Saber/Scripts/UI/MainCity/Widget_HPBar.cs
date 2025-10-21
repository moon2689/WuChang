using UnityEngine;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Widget_HPBar : WidgetBase
    {
        [SerializeField] private Image m_ImageHp;
        [SerializeField] private Image m_ImageHpSmooth;

        private float m_TimerHealthSmoothChange;
        private float m_OldHp;
        private float m_MaxHp;
        private float m_CurHp;
        private Image m_FastImage;
        private Image m_FollowImage;
        private float m_FollowSpeed;

        public float CurHp
        {
            get => m_CurHp;
            set => m_CurHp = Mathf.Clamp(value, 0, m_MaxHp);
        }

        public void Init(float maxHp, float curHp)
        {
            m_MaxHp = maxHp;
            CurHp = curHp;
            m_OldHp = curHp;
            m_ImageHp.fillAmount = m_ImageHpSmooth.fillAmount = CurHp / m_MaxHp;
        }

        protected override void Update()
        {
            base.Update();

            if (CurHp > m_OldHp)
            {
                // hp增加
                m_FastImage = m_ImageHpSmooth;
                m_FollowImage = m_ImageHp;
                m_FastImage.fillAmount = CurHp / m_MaxHp;
                m_OldHp = CurHp;
                if (m_FollowImage.fillAmount == m_FastImage.fillAmount)
                    m_TimerHealthSmoothChange = 0.5f;
                m_FollowSpeed = Mathf.Abs(m_FastImage.fillAmount - m_FollowImage.fillAmount) / 0.3f;
            }
            else if (CurHp < m_OldHp)
            {
                // hp减少
                m_FastImage = m_ImageHp;
                m_FollowImage = m_ImageHpSmooth;
                m_FastImage.fillAmount = CurHp / m_MaxHp;
                m_OldHp = CurHp;
                m_TimerHealthSmoothChange = 1f;
                m_FollowSpeed = Mathf.Abs(m_FastImage.fillAmount - m_FollowImage.fillAmount) / 0.5f;
            }

            if (m_FastImage == null)
            {
                return;
            }

            float tar = m_FastImage.fillAmount;
            if (m_FollowImage.fillAmount != tar)
            {
                if (m_TimerHealthSmoothChange < 0)
                {
                    float cur = m_FollowImage.fillAmount;
                    float offset = m_FollowSpeed * Time.deltaTime;
                    if (cur < tar)
                    {
                        cur += offset;
                        if (cur > tar)
                        {
                            cur = tar;
                        }
                    }
                    else if (cur > tar)
                    {
                        cur -= offset;
                        if (cur < tar)
                        {
                            cur = tar;
                        }
                    }

                    m_FollowImage.fillAmount = cur;
                }
                else
                {
                    m_TimerHealthSmoothChange -= Time.deltaTime;
                }
            }
        }
    }
}