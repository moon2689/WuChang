using System;
using UnityEngine;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Wnd_Tips : WndBase
    {
        public enum EState
        {
            Hide,
            ShowText,
            Fade,
        }

        private const float k_FadeDuration = 0.5f;
        [SerializeField] private Text m_TextTitle;
        [SerializeField] private Text m_TextContent;
        [SerializeField] private CanvasGroup m_CanvasGroup;


        private float m_TimerShowText, m_TimerFade;
        private bool m_Fading;
        private EState m_State;


        protected override bool PauseGame => false;

        protected override void Start()
        {
            base.Start();
            //m_TextContent.verticalOverflow = VerticalWrapMode.Overflow;
            m_TextTitle.text = "提示";
        }

        public void ShowText(string message, float textTime)
        {
            if (m_State == EState.ShowText)
                m_TextContent.text = message + "\n" + m_TextContent.text;
            else
                m_TextContent.text = message;

            m_State = EState.ShowText;
            if (m_TimerShowText < textTime)
                m_TimerShowText = textTime;
        }

        public void ShowText(string message)
        {
            ShowText(message, 1);
        }

        void FadeEffect()
        {
            if (m_State == EState.ShowText)
            {
                m_CanvasGroup.alpha = 1;
                m_TimerShowText -= Time.deltaTime;
                if (m_TimerShowText <= 0)
                {
                    m_State = EState.Fade;
                    m_TimerFade = k_FadeDuration;
                }
            }
            else if (m_State == EState.Fade)
            {
                m_TimerFade -= Time.deltaTime;
                if (m_TimerFade <= 0)
                {
                    m_TimerFade = 0;
                    m_State = EState.Hide;
                }

                m_CanvasGroup.alpha = m_TimerFade / k_FadeDuration;
            }
            else if (m_State == EState.Hide)
            {
                m_CanvasGroup.alpha = 0;
            }
            else
            {
                throw new InvalidOperationException("Unknown state:" + m_State);
            }
        }

        protected override void Update()
        {
            base.Update();
            FadeEffect();
        }
    }
}