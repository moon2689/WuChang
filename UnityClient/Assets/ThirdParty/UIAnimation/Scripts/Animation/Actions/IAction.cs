using System;
using UnityEngine;

namespace UIAnimation.Actions
{
    /// <summary>
    /// Use Abstract Class instead of Interface to define common behaviours, 
    /// because only the subclasses of MonoBehaviour are editable in Unity Inspector
    /// </summary>
    public abstract class IAction : MonoBehaviour
    {
        [SerializeField]
        protected string description;
        [SerializeField]
        protected float delaySeconds;
        [SerializeField]
        protected bool isRunning = false;

        protected ActionType myType;
        public event Action OnActionDone;

        public enum ActionType
        {
            Tween,
            LegacyAnimation,
            Callback
        };

        public abstract void FinalizeAction(bool isFastforward = false);
        public abstract bool IsDone();
        public abstract void OnStep(float deltaTime, bool shouldPause);
        /// <summary>
        /// Use this method to reset the Action to the same status as that after Awake()
        /// </summary>
        public abstract void ResetStatus();
        /// <summary>
        /// Call this method before running the action
        /// </summary>
        public abstract void Prepare();

        protected void CallOnActionDoneEvent()
        {
            if (OnActionDone != null)
            {
                OnActionDone();
            }
        }

        public ActionType MyType
        {
            get { return myType; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public float DelaySeconds
        {
            get { return delaySeconds; }
            set { delaySeconds = value; }
        }

        public bool IsRunning
        {
            get { return isRunning; }
        }

    }
}
