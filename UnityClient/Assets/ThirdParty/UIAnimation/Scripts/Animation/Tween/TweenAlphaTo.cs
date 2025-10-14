using SakashoUISystem;
using System.Collections.Generic;
using UIAnimation.Actions;
using UnityEngine;
using UnityEngine.UI;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Alpha To")]
    public class TweenAlphaTo : TweenActionBase
    {
        [SerializeField] float toAlpha;

        List<Graphic> m_widgets = new();
        List<Color> m_colors = new();
        List<float> m_alphas = new();

        protected override void Awake()
        {
            base.Awake();

            /*
            var roowWidget = transform.GetComponent<UIWidget>();

            if (roowWidget != null)
            {
                m_widgets.Add(roowWidget);
                m_colors.Add(roowWidget.color);
                m_alphas.Add(roowWidget.color.a);
            }
            */

            foreach (var childWidget in transform.GetComponentsInChildren<Graphic>())
            {
                m_widgets.Add(childWidget);
                m_colors.Add(childWidget.color);
                m_alphas.Add(childWidget.color.a);
            }
        }

        public override void ResetStatus()
        {
            base.ResetStatus();
            for (int i = 0; i < m_widgets.Count; i++)
            {
                if (m_widgets[i] != null)
                    m_widgets[i].color = m_colors[i];
            }
        }

        public override void Prepare()
        {
            base.Prepare();

            for (int i = 0; i < m_widgets.Count; i++)
            {
                if (m_widgets[i] != null)
                    m_alphas[i] = m_widgets[i].color.a;
            }
        }

        protected override void Lerp(float normalizedTime)
        {
            for (int i = 0; i < m_widgets.Count; i++)
            {
                var col = m_colors[i];
                col.a = Mathematics.LerpFloat(m_alphas[i], ToAlpha, normalizedTime);
                if (m_widgets[i] != null)
                    m_widgets[i].color = col;
            }
        }

        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
            for (int i = 0; i < m_widgets.Count; i++)
            {
                var imageColor = m_colors[i];
                imageColor.a = ToAlpha;
                if (m_widgets[i] != null)
                    m_widgets[i].color = imageColor;
            }
        }

        public float ToAlpha
        {
            get { return toAlpha; }
            set { toAlpha = value; }
        }
    }
}