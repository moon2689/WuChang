using System.Collections.Generic;
using UIAnimation.Actions;
using SakashoUISystem;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Alpha To")]
    public class TweenUIAlphaTo : TweenActionBase
    {
        [SerializeField] float m_ToAlpha;

        Graphic m_Graphic;
        private Color m_OriginColor;

        public float ToAlpha
        {
            get => m_ToAlpha;
            set => m_ToAlpha = value;
        }


        protected override void Awake()
        {
            base.Awake();
            m_Graphic = transform.GetComponent<Graphic>();
            m_OriginColor = m_Graphic.color;
        }

        public override void ResetStatus()
        {
            base.ResetStatus();
            m_Graphic.color = m_OriginColor;
        }

        public override void Prepare()
        {
            base.Prepare();
            m_Graphic.color = m_OriginColor;
        }

        protected override void Lerp(float normalizedTime)
        {
            float tarAlpha = Mathematics.LerpFloat(m_OriginColor.a, ToAlpha, normalizedTime);
            m_Graphic.color = new Color(m_OriginColor.r, m_OriginColor.g, m_OriginColor.b, tarAlpha);
        }
    }
}