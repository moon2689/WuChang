using SakashoUISystem;
using System.Collections.Generic;
using UIAnimation.Actions;
using UnityEngine;
using UnityEngine.UI;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Size To")]
    public class TweenUISizeTo : TweenActionBase
    {
        [SerializeField] public Vector2 fromWidthAndHeight;
        [SerializeField] public Vector2 toWidthAndHeight;

        Vector2 m_OrightSize;
        RectTransform m_RectTransform;

        protected override void Awake()
        {
            base.Awake();

            m_RectTransform = transform.GetComponent<RectTransform>();
            if (m_RectTransform != null)
            {
                m_OrightSize = m_RectTransform.sizeDelta;
            }
        }

        public override void ResetStatus()
        {
            base.ResetStatus();
            if (m_RectTransform == null)
            {
                return;
            }

            m_RectTransform.sizeDelta = m_OrightSize;
        }

        protected override void Lerp(float normalizedTime)
        {
            int width = (int)Mathematics.LerpFloat(fromWidthAndHeight.x, toWidthAndHeight.x, normalizedTime);
            int height = (int)Mathematics.LerpFloat(fromWidthAndHeight.y, toWidthAndHeight.y, normalizedTime);
            m_RectTransform.sizeDelta = new Vector2(width, height);
        }

        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
            m_RectTransform.sizeDelta = toWidthAndHeight;
        }
    }
}