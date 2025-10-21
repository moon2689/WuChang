using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIAnimation.Actions;
using SakashoUISystem;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Rect Transform Move To")]
    public class TweenRectTransformMoveTo : TweenActionBase
    {
        [SerializeField] private Vector3 toPosition;
        private Vector3 originalPosition;
        private Vector3 fromPosition;
        private Vector3 value;
        private RectTransform m_RectTransform;


        public Vector3 ToPosition
        {
            get { return toPosition; }
            set { toPosition = value; }
        }


        protected override void Awake()
        {
            base.Awake();
            m_RectTransform = GetComponent<RectTransform>();
            if (m_RectTransform == null)
            {
                throw new System.Exception("Transformコンポーネントが見つかりません");
            }

            originalPosition = m_RectTransform.anchoredPosition3D;
        }

        public override void ResetStatus()
        {
            base.ResetStatus();
            m_RectTransform.anchoredPosition3D = originalPosition;
        }

        public override void Prepare()
        {
            base.Prepare();
            fromPosition = m_RectTransform.anchoredPosition3D;
        }


        protected override void Lerp(float normalizedTime)
        {
            value = Mathematics.LerpVec3(fromPosition, ToPosition, normalizedTime);
            m_RectTransform.anchoredPosition3D = value;
        }


        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
            m_RectTransform.anchoredPosition3D = ToPosition;
        }
    }
}