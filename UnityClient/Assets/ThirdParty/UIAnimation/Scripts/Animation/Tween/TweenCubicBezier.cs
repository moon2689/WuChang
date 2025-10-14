using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIAnimation.Actions;
using SakashoUISystem;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Cubic Bezier")]
    public class TweenCubicBezier : TweenActionBase
    {
        [SerializeField] private Vector3 endPosition;

        public Vector3 EndPosition
        {
            get { return endPosition; }
            set { endPosition = value; }
        }

        [SerializeField] private Vector3 controlPosition1;

        public Vector3 ControlPosition1
        {
            get { return controlPosition1; }
            set { controlPosition1 = value; }
        }

        [SerializeField] private Vector3 controlPosition2;

        public Vector3 ControlPosition2
        {
            get { return controlPosition2; }
            set { controlPosition2 = value; }
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

        protected override void Lerp(float normalizedTime)
        {
            transform.localPosition = Mathematics.CubicBezier3(startPosition, EndPosition, ControlPosition1, ControlPosition2, normalizedTime);
        }

        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
            transform.localPosition = EndPosition;
        }
    }
}