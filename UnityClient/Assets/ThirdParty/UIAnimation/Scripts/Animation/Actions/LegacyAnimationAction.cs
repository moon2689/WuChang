using UnityEngine;
using UnityEngine.Events;

namespace UIAnimation.Actions
{
    [AddComponentMenu("UIAnimation/Actions/LegacyAnimationAction Action")]
    [RequireComponent(typeof(Animation))]
    public class LegacyAnimationAction : IAction
    {
        [SerializeField]
        private AnimationClip clip;
        public AnimationClip Clip {
            get {
                return clip;
            }
            set {
                clip = value;
            }
        }

        public WrapMode ClipWrapMode {
            get {
                return Clip.wrapMode;
            }
            set {
                Clip.wrapMode = value;
            }
        }

        [SerializeField]
        private float speed = 1f;
        public float Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }
                        
        private float timeElapsedDuringDelay = 0;
        public float TimeElapsedDuringDelay {
            get {
                return timeElapsedDuringDelay;
            }
        }

        private Animation animationComponent;
        public Animation AnimationComponent {
            get {
                if (animationComponent == null) {
                    animationComponent = GetComponent<Animation>();
                }
                return animationComponent;
            }
        }

        private void Awake()
        {
            myType = ActionType.LegacyAnimation;
            if (Clip == null) {
                throw new System.Exception("No LegacyAnimation is assigned for LegacyAnimationAction");
            }
            AnimationComponent.AddClip(Clip, Clip.name);
        }

        private bool isDone = false;
                
        private AnimationState animationState;
        private AnimationState AnimationState {
            get {
                if (animationState == null) {
                    foreach (AnimationState state in AnimationComponent) {
                        if (state.clip.name == Clip.name) {
                            animationState = state;
                            break;
                        }
                    }
                }
                return animationState;
            }
        }

        #region implemented abstract members of IAction

        public override void FinalizeAction (bool isFastforward = false)
        {
            AnimationState.normalizedTime = 1f;
            base.CallOnActionDoneEvent();
        }

        public override bool IsDone ()
        {
            return isDone;
        }

        public override void OnStep (float deltaTime, bool shouldPause)
        {
            if (timeElapsedDuringDelay < DelaySeconds) {
                timeElapsedDuringDelay += deltaTime;
                return;
            }
            if (shouldPause) {
                AnimationState.speed = 0f;
            } else {
                AnimationState.speed = Speed;
            }

            if (!AnimationComponent.IsPlaying(clip.name)) {
                isDone = true;
                FinalizeAction();
            }
        }

        public override void ResetStatus()
        {
            AnimationComponent.Stop(Clip.name);
            AnimationState.speed = Speed;
            if (Speed > 0)
            {
                AnimationState.time =0 ;
            }
            else
            {
                AnimationState.time = clip.length;
            }
            AnimationComponent.Play(clip.name);
            isDone = false;
        }

        public override void Prepare ()
        {
            ResetStatus();
        }

        #endregion
                
    }
}