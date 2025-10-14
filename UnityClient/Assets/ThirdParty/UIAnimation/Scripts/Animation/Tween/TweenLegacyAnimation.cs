using UnityEngine;
using UIAnimation.Actions;
using System;

namespace UIAnimation.Tween
{
    [AddComponentMenu("UIAnimation/Tween/Tween LegacyAnimation")]
    [RequireComponent(typeof(Animation))]
    public class TweenLegacyAnimation : TweenActionBase
    {
        [SerializeField] private AnimationClip clip;

        public AnimationClip Clip
        {
            get { return clip; }
            set { clip = value; }
        }

        private Animation animationComponent;

        public Animation AnimationComponent
        {
            get
            {
                if (animationComponent == null)
                {
                    animationComponent = GetComponent<Animation>();
                }

                return animationComponent;
            }
        }

        private AnimationState animationState;

        private AnimationState AnimationState
        {
            get
            {
                if (animationState == null)
                {
                    foreach (AnimationState state in AnimationComponent)
                    {
                        if (state.clip.name == Clip.name)
                        {
                            animationState = state;
                            break;
                        }
                    }
                }

                return animationState;
            }
        }

        private float fromNormalizedTime;
        private float toNormalizedTime;
        private bool isPrepared = false;

        protected override void Awake()
        {
            base.Awake();
            fromNormalizedTime = Speed >= 0 ? 0f : 1f;
            toNormalizedTime = Speed >= 0 ? 1f : 0f;
        }

        public override void ResetStatus()
        {
            base.ResetStatus();
            Prepare();

            PauseAt(fromNormalizedTime);
        }

        public override void Prepare()
        {
            base.Prepare();

            if (!AnimationComponent.GetClip(Clip.name))
            {
                AnimationComponent.AddClip(Clip, Clip.name);
            }
        }

        protected override void OnActionIsDone()
        {
            if (!isPrepared)
            {
                GetReady();
            }

            AnimationState.normalizedTime = toNormalizedTime;
            RequestToStopAnimation();
            isPrepared = false;

            base.OnActionIsDone();
        }

        protected override void Lerp(float normalizedTime)
        {
            if (!isPrepared)
            {
                GetReady();
            }

            if (!gameObject.activeInHierarchy)
            {
                throw new System.Exception(gameObject.name + " must be activeInHierarchy during playback");
            }

            AnimationState.normalizedTime = Mathf.Lerp(fromNormalizedTime, toNormalizedTime, normalizedTime);
        }

        private void GetReady()
        {
            AnimationComponent.Play(Clip.name);
            AnimationState.speed = 0;
            isPrepared = true;
        }

        private void PauseAt(float normalizedTime)
        {
            AnimationComponent.Play(Clip.name);
            AnimationState.speed = 0;
            AnimationState.normalizedTime = normalizedTime;
            RequestToStopAnimation();
        }

        void RequestToStopAnimation()
        {
            if (!isPrepared)
                AnimationComponent.Stop(Clip.name);
        }
    }
}