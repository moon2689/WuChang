using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIAnimation.Actions;
using SakashoUISystem;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Transform Rotate To")]
    public class TweenTransformRotationTo : TweenActionBase
    {
        [SerializeField] private Vector3 toRotation;

        public Vector3 ToRotation
        {
            get { return toRotation; }
            set { toRotation = value; }
        }

        private Vector3 originalRotation;
        private Vector3 fromRotation;
        private Quaternion value = Quaternion.identity;

        protected override void Awake()
        {
            base.Awake();
            if (transform == null)
            {
                throw new System.Exception("Transformコンポーネントが見つかりません");
            }

            originalRotation = transform.localRotation.eulerAngles;
        }

        public override void ResetStatus()
        {
            base.ResetStatus();
            transform.localRotation = Quaternion.Euler(originalRotation);
        }

        public override void Prepare()
        {
            base.Prepare();
            fromRotation = transform.localRotation.eulerAngles;
        }

        #region implemented abstract members of TweenerBase

        protected override void Lerp(float normalizedTime)
        {
            value = Quaternion.Euler(
                new Vector3(
                    Mathematics.LerpFloat(fromRotation.x, ToRotation.x, normalizedTime),
                    Mathematics.LerpFloat(fromRotation.y, ToRotation.y, normalizedTime),
                    Mathematics.LerpFloat(fromRotation.z, ToRotation.z, normalizedTime)
                )
            );
            transform.localRotation = value;
        }

        #endregion

        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
            transform.localRotation = Quaternion.Euler(ToRotation);
        }
    }
}