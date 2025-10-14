using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UIAnimation.Actions;
using SakashoUISystem;
using System.Collections.Generic;

namespace Straw.FrameWork.Tween
{
    [ExecuteInEditMode()]
    /// <summary>
    /// TweenScaleToはheightを使う　こちらはRectTransformのscaleをつかう
    /// </summary>
    public class TweenRealLocalScaleTo : TweenActionBase
    {
        private float fromScaleX;
        private float fromScaleY;

        [SerializeField] private float toScaleX;

        public float ToScaleX
        {
            get { return toScaleX; }
            set { toScaleX = value; }
        }

        [SerializeField] private float toScaleY;

        public float ToScaleY
        {
            get { return toScaleY; }
            set { toScaleY = value; }
        }

        [SerializeField] private float originX;
        [SerializeField] private float originY;
        private float valueX;
        private float valueY;

        private bool isAwaked = false;

        protected override void Awake()
        {
            base.Awake();
            isAwaked = true;
        }

        public override void ResetStatus()
        {
            base.ResetStatus();
            transform.localScale = new Vector3(originX, originY, transform.localScale.z);
        }

        public override void Prepare()
        {
            base.Prepare();
            if (!isAwaked)
                Awake();
            fromScaleX = transform.localScale.x;
            fromScaleY = transform.localScale.y;
        }


        #region implemented abstract members of TweenActionBase

        protected override void Lerp(float normalizedTime)
        {
            valueX = Mathematics.LerpFloat(fromScaleX, toScaleX, normalizedTime);
            valueY = Mathematics.LerpFloat(fromScaleY, toScaleY, normalizedTime);

            transform.localScale = new Vector3(valueX, valueY, transform.localScale.z);
        }

        #endregion


        protected override void OnActionIsDone()
        {
            base.OnActionIsDone();
            transform.localScale = new Vector3(originX * toScaleX, originY * toScaleY, transform.localScale.z);
        }
    }
}