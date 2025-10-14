using UnityEngine;

namespace UIAnimation.Actions
{
    public abstract class TweenActionBase : IAction
    {
        [SerializeField]
        protected AnimationWrapMode wrapMode = AnimationWrapMode.Clamp;
        [SerializeField]
        protected AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
        [SerializeField]
        protected float durationSeconds = 0f;
        [SerializeField]
        protected float speed = 1f;

        protected float m_timeElapsedDuringDelay;
        protected float m_timeElapsedInSec;

        #region enum

        public enum AnimationWrapMode
        {
            Clamp,
            Loop,
            PingPong
        };

        public enum PlaybackDirection
        {
            Forward,
            Backward
        };

        #endregion

        /// <summary>
        /// Lerp should be implemented in each Tweener class
        /// </summary>        
        protected abstract void Lerp(float normalizedTime);

        /// <summary>
        /// Only Valid for WrapMode.Clamp. 
        /// For WrapMode.Loop or WrapMode.PingPong, there is no concept of "Finish/Done"
        /// </summary>
        public override bool IsDone()
        {
            return WrapMode == AnimationWrapMode.Clamp && (m_timeElapsedInSec > DurationSeconds || m_timeElapsedInSec < 0f);
        }

        public override void OnStep(float deltaTime, bool shouldPause)
        {
            isRunning = false;

            if (shouldPause)
                return;

            if (m_timeElapsedDuringDelay < DelaySeconds)
            {
                m_timeElapsedDuringDelay += deltaTime;
                return;
            }

            isRunning = true;
            Lerp(Curve.Evaluate(ProcessedNormalizedTime));

            if (Speed > 0)
                m_timeElapsedInSec += (deltaTime * Speed);
            else if (Speed < 0)
                m_timeElapsedInSec -= (deltaTime * Speed);
            else
                return;

            if (IsDone())
                OnActionIsDone();
        }

        public override void Prepare()
        {
            m_timeElapsedDuringDelay = 0f;
            m_timeElapsedInSec = 0f;
        }

        public override void FinalizeAction(bool isFastforward = false)
        {
            if (WrapMode != AnimationWrapMode.Clamp)
                return;

            OnActionIsDone();
        }

        protected virtual void OnActionIsDone()
        {
            isRunning = false;
            base.CallOnActionDoneEvent();
        }

        protected virtual void Awake()
        {
            myType = ActionType.Tween;
        }

        public override void ResetStatus()
        {
            // will be implemented in derived classes
        }

        #region Properties

        public AnimationWrapMode WrapMode
        {
            get { return wrapMode; }
            set { wrapMode = value; }
        }

        public AnimationCurve Curve
        {
            get { return curve; }
            set { curve = value; }
        }

        public float DurationSeconds
        {
            get { return durationSeconds; }
            set { durationSeconds = value; }
        }

        public PlaybackDirection Direction
        {
            get
            {
                if (WrapMode == AnimationWrapMode.PingPong)
                    return (Mathf.FloorToInt(NormalizedTime) % 2 == 0) ? PlaybackDirection.Forward : PlaybackDirection.Backward;
                else
                    return (Speed >= 0) ? PlaybackDirection.Forward : PlaybackDirection.Backward;
            }
        }

        public float NormalizedTime
        {
            get { return DurationSeconds == 0 ? 1f : m_timeElapsedInSec / DurationSeconds; }
        }


        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        public float ProcessedNormalizedTime
        {
            get
            {
                var normalizedTime = NormalizedTime;
                if (normalizedTime >= 0 && normalizedTime <= 1)
                    return normalizedTime;

                if (WrapMode == AnimationWrapMode.Clamp)
                    normalizedTime = Mathf.Clamp01(normalizedTime);

                if (WrapMode == AnimationWrapMode.Loop)
                {
                    normalizedTime = normalizedTime % 1;
                    if (normalizedTime < 0)
                        normalizedTime += 1f;
                }

                if (WrapMode == AnimationWrapMode.PingPong)
                {
                    if (Direction == PlaybackDirection.Forward)
                        normalizedTime %= 1;
                    else
                        normalizedTime = 1f - (normalizedTime % 1);
                }
                return normalizedTime;
            }
        }

        #endregion

    }
}
