using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIAnimation.Actions;
using SakashoUISystem;
using System.Collections.Generic;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween Transform Scale By")]
    public class TweenTransformScaleBy : TweenActionBase
    {
        [SerializeField] private float deltaScaleX;

        public float DeltaScaleX
        {
            get { return deltaScaleX; }
            set { deltaScaleX = value; }
        }

        [SerializeField] private float deltaScaleY;

        public float DeltaScaleY
        {
            get { return deltaScaleY; }
            set { deltaScaleY = value; }
        }

        [SerializeField] private float deltaScaleZ;

        public float DeltaScaleZ
        {
            get { return deltaScaleZ; }
            set { deltaScaleZ = value; }
        }

        private Vector3 fromScale;
        private Vector3 toScale;
        private Vector3 value;
        private Vector3 originalScale;

        protected override void Awake()
        {
            base.Awake();
            if (transform == null)
            {
                throw new System.Exception("Transformコンポーネントが見つかりません");
            }

            originalScale = transform.localScale;
        }

        public override void ResetStatus()
        {
            base.ResetStatus();

            fromScale = originalScale;
        }

        public override void Prepare()
        {
            base.Prepare();

            fromScale = transform.localScale;
            toScale = fromScale + new Vector3(DeltaScaleX, DeltaScaleY, DeltaScaleZ);
        }

        #region implemented abstract members of Tweener

        protected override void Lerp(float normalizedTime)
        {
            transform.localScale = Mathematics.LerpVec3(fromScale, toScale, normalizedTime);
        }

        #endregion

        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
            transform.localScale = toScale;
        }
    }
}