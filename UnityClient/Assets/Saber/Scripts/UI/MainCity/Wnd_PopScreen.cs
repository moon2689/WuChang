using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Wnd_PopScreen : WndBase
    {
        public enum EStyle
        {
            PlayerDead,
            BossDead,
        }

        enum EState
        {
            Showing,
            FadingZi,
            FadingFu,
            Hide,
        }


        [SerializeField] private Image m_ImagePlayerDead;
        [SerializeField] private Image m_ImageBossDead;

        private float m_Timer;
        private EState m_State;
        private Image m_ImageZi;
        private Image m_ImageFu;


        protected override void Awake()
        {
            base.Awake();
            HideAll();
        }

        public void Reset(EStyle style)
        {
            gameObject.SetActive(true);
            m_ImagePlayerDead.gameObject.SetActive(style == EStyle.PlayerDead);
            m_ImageBossDead.gameObject.SetActive(style == EStyle.BossDead);
            m_Timer = 3;
            m_State = EState.Showing;

            if (style == EStyle.PlayerDead)
            {
                m_ImageZi = m_ImagePlayerDead;
            }
            else if (style == EStyle.BossDead)
            {
                m_ImageZi = m_ImageBossDead;
            }

            m_ImageFu = m_ImageZi.transform.GetChild(0).GetComponent<Image>();

            m_ImageZi.color = new Color(1, 1, 1, 1);
            m_ImageFu.color = new Color(1, 1, 1, 1);
            m_ImageZi.gameObject.SetActive(true);
            m_ImageFu.gameObject.SetActive(true);
        }

        protected override void Update()
        {
            base.Update();

            if (m_State == EState.Showing)
            {
                if (m_Timer > 0)
                {
                    m_Timer -= Time.deltaTime;
                }
                else
                {
                    m_State = EState.FadingFu;
                    m_Timer = 1;
                }
            }
            else if (m_State == EState.FadingFu)
            {
                m_Timer -= Time.deltaTime * 1.5f;
                if (m_Timer > 0)
                {
                    m_ImageFu.color = new Color(1, 1, 1, m_Timer);
                }
                else
                {
                    m_Timer = 1;
                    m_State = EState.FadingZi;
                    m_ImageFu.gameObject.SetActive(false);
                }
            }
            else if (m_State == EState.FadingZi)
            {
                m_Timer -= Time.deltaTime * 0.9f;
                if (m_Timer > 0)
                {
                    m_ImageZi.color = new Color(1, 1, 1, m_Timer);
                }
                else
                {
                    m_State = EState.Hide;
                    HideAll();
                }
            }
            else if (m_State == EState.Hide)
            {
            }
        }

        void HideAll()
        {
            gameObject.SetActive(false);
            m_ImagePlayerDead.gameObject.SetActive(false);
            m_ImageBossDead.gameObject.SetActive(false);
        }
    }
}