using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIAnimation.Actions;
using SakashoUISystem;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Transform Move To")]
    public class TweenTransformMoveTo : TweenActionBase
    {
        [SerializeField] private Vector3 toPosition;

        public Vector3 ToPosition
        {
            get { return toPosition; }
            set { toPosition = value; }
        }

        private Vector3 originalPosition;
        private Vector3 fromPosition;
        private Vector3 value;

        protected override void Awake()
        {
            base.Awake();
            if (transform == null)
            {
                throw new System.Exception("Transformコンポーネントが見つかりません");
            }

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
            fromPosition = transform.localPosition;
        }

        #region implemented abstract members of TweenerBase

        protected override void Lerp(float normalizedTime)
        {
            value = Mathematics.LerpVec3(fromPosition, ToPosition, normalizedTime);
            transform.localPosition = value;
        }

        #endregion

        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
            transform.localPosition = ToPosition;
        }
    }
}