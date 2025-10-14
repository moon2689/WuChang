using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIAnimation.Actions;
using SakashoUISystem;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Transform Move By")]
    public class TweenTransformMoveBy : TweenActionBase
    {
        [SerializeField] private Vector3 deltaVec3;

        public Vector3 DeltaVec3
        {
            get { return deltaVec3; }
            set { deltaVec3 = value; }
        }

        private Vector3 toPosition;

        private Vector3 fromPosition;
        private Vector3 value;

        protected override void Awake()
        {
            base.Awake();
            if (transform == null)
            {
                throw new System.Exception("Transformコンポーネントが見つかりません");
            }
        }

        public override void ResetStatus()
        {
            base.ResetStatus();
        }

        public override void Prepare()
        {
            base.Prepare();
            fromPosition = transform.localPosition;
            toPosition = deltaVec3 + fromPosition;
        }

        #region implemented abstract members of TweenerBase

        protected override void Lerp(float normalizedTime)
        {
            value = Mathematics.LerpVec3(fromPosition, toPosition, normalizedTime);
            transform.localPosition = value;
        }

        #endregion

        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
            transform.localPosition = toPosition;
        }
    }
}