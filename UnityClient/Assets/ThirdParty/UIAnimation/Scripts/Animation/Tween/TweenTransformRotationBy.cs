using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIAnimation.Actions;
using SakashoUISystem;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Transform Rotate By")]
    public class TweenTransformRotationBy : TweenActionBase
    {
        [SerializeField] private Vector3 deltaVec3;

        public Vector3 DeltaVec3
        {
            get { return deltaVec3; }
            set { deltaVec3 = value; }
        }

        private Vector3 fromRotation;
        private Quaternion value = Quaternion.identity;
        private Vector3 toRotation;

        protected override void Awake()
        {
            base.Awake();
            if (transform == null)
            {
                throw new System.Exception("RectTransformコンポーネントが見つかりません");
            }
        }

        public override void ResetStatus()
        {
            base.ResetStatus();
        }

        public override void Prepare()
        {
            base.Prepare();
            fromRotation = transform.localRotation.eulerAngles;
            toRotation = fromRotation + deltaVec3;
        }

        #region implemented abstract members of TweenerBase

        protected override void Lerp(float normalizedTime)
        {
            value = Quaternion.Euler(
                new Vector3(
                    Mathematics.LerpFloat(fromRotation.x, toRotation.x, normalizedTime),
                    Mathematics.LerpFloat(fromRotation.y, toRotation.y, normalizedTime),
                    Mathematics.LerpFloat(fromRotation.z, toRotation.z, normalizedTime)
                )
            );
            transform.localRotation = value;
        }

        #endregion

        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
            transform.localRotation = Quaternion.Euler(toRotation);
        }
    }
}