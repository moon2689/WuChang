using SakashoUISystem;
using System.Collections.Generic;
using UIAnimation.Actions;
using UnityEngine;
using UnityEngine.UI;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Alpha By")]
    public class TweenAlphaBy : TweenActionBase
    {
        [SerializeField] float deltaAlpha;

        List<Graphic> m_widgets = new();
        List<Color> m_colors = new();
        List<float> m_fromAlphas = new();
        List<float> m_toAlphas = new();

        protected override void Awake()
        {
            base.Awake();

            /*
            UIWidget rootWidget = transform.GetComponent<UIWidget>();
            if (rootWidget != null)
            {
                m_widgets.Add(rootWidget);
                m_colors.Add(rootWidget.color);
                m_fromAlphas.Add(rootWidget.color.a);
                m_toAlphas.Add(rootWidget.color.a);
            }
            */


            foreach (Graphic childWidget in transform.GetComponentsInChildren<Graphic>())
            {
                m_widgets.Add(childWidget);
                m_colors.Add(childWidget.color);
                m_fromAlphas.Add(childWidget.color.a);
                m_toAlphas.Add(childWidget.color.a);
            }
        }

        public override void ResetStatus()
        {
            base.ResetStatus();
            for (int i = 0; i < m_widgets.Count; i++)
                m_widgets[i].color = m_colors[i];
        }

        public override void Prepare()
        {
            base.Prepare();

            for (int i = 0; i < m_widgets.Count; i++)
            {
                m_fromAlphas[i] = m_widgets[i].color.a;
                m_toAlphas[i] = Mathf.Clamp01(m_fromAlphas[i] + DeltaAlpha);
            }
        }

        protected override void Lerp(float normalizedTime)
        {
            for (int i = 0; i < m_widgets.Count; i++)
            {
                var col = m_colors[i];
                col.a = Mathematics.LerpFloat(m_fromAlphas[i], m_toAlphas[i], normalizedTime);
                m_widgets[i].color = col;
            }
        }

        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
            for (int i = 0; i < m_widgets.Count; i++)
            {
                var imageColor = m_colors[i];
                imageColor.a = m_toAlphas[i];
                m_widgets[i].color = imageColor;
            }
        }

        public float DeltaAlpha
        {
            get { return deltaAlpha; }
            set { deltaAlpha = value; }
        }
    }
}