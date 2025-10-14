using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIAnimation.Actions;
using SakashoUISystem;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Quadratic Bezier")]
    public class TweenQuadraticBezier : TweenActionBase
    {
        [SerializeField] private Vector3 endPosition;

        public Vector3 EndPosition
        {
            get { return endPosition; }
            set { endPosition = value; }
        }

        [SerializeField] private Vector3 controlPosition;

        public Vector3 ControlPosition
        {
            get { return controlPosition; }
            set { controlPosition = value; }
        }

        private Vector3 originalPosition;
        private Vector3 startPosition;

        protected override void Awake()
        {
            base.Awake();
            originalPosition = transform.localPosition;
        }

        public override void ResetStatus()
        {
            base.ResetStatus();
            transform.localPosition = originalPosition;
        }

        public override void Prepare()
        {
            base.Prepare();
            startPosition = transform.localPosition;
        }

        #region implemented abstract members of TweenerBase

        protected override void Lerp(float normalizedTime)
        {
            transform.localPosition = Mathematics.QuadraticBezier3(startPosition, EndPosition, ControlPosition, normalizedTime);
        }

        #endregion

        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
            transform.localPosition = EndPosition;
        }
    }
}