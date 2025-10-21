using System;
using System.Collections.Generic;
using System.Linq;
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

        class TextItem
        {
            public string Messenge;
            public float ShowingTime;
        }

        private const float k_FadeDuration = 0.5f;
        [SerializeField] private Text m_TextTitle;
        [SerializeField] private Text m_TextContent;
        [SerializeField] private CanvasGroup m_CanvasGroup;


        private float m_TimerShowText, m_TimerFade;
        private bool m_Fading;
        private EState m_State;
        private Queue<TextItem> m_QueueTexts = new();
        private List<TextItem> m_CacheTexts = new();
        private float m_TimerShowTextInterval;


        protected override void Start()
        {
            base.Start();
            //m_TextContent.verticalOverflow = VerticalWrapMode.Overflow;
            m_TextTitle.text = "提示";
        }

        public void ShowText(string message, float textTime)
        {
            TextItem item = m_CacheTexts.FirstOrDefault();
            if (item != null)
            {
                m_CacheTexts.RemoveAt(0);
            }
            else
            {
                item = new TextItem();
            }

            item.Messenge = message;
            item.ShowingTime = textTime;
            m_QueueTexts.Enqueue(item);
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

            if (m_TimerShowTextInterval > 0)
            {
                m_TimerShowTextInterval -= Time.deltaTime;
            }

            if (m_QueueTexts.Count > 0)
            {
                if (m_TimerShowTextInterval <= 0)
                {
                    TextItem item = m_QueueTexts.Dequeue();
                    if (item != null)
                    {
                        m_CacheTexts.Add(item);
                        ShowText(item);
                    }
                }
            }

            FadeEffect();
        }

        private void ShowText(TextItem item)
        {
            m_TimerShowTextInterval = 0.5f;
            string message = item.Messenge;
            float textTime = item.ShowingTime;
            if (m_State == EState.ShowText)
                m_TextContent.text = message + "\n" + m_TextContent.text;
            else
                m_TextContent.text = message;

            m_State = EState.ShowText;
            if (m_TimerShowText < textTime)
                m_TimerShowText = textTime;
        }
    }
}