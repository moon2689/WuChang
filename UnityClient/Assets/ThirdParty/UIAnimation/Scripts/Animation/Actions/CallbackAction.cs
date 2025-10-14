using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UIAnimation.Actions
{
    [AddComponentMenu("UIAnimation/Actions/Callback Action")]
    public class CallbackAction : IAction
    {
        [SerializeField]
        protected UnityEvent callback;
        public UnityEvent Callback {
            get {
                return callback;
            }
        }

        protected bool isCalled = false;
        protected float timeElapsedDuringDelay = 0f;

        #region implemented abstract members of IAction
        public override bool IsDone ()
        {
            return isCalled;
        }

        public override void OnStep (float deltaTime, bool shouldPause)
        {
            if (shouldPause) {
                return;
            }

            if (timeElapsedDuringDelay < DelaySeconds) {
                timeElapsedDuringDelay += deltaTime;
                return;
            }

            FinalizeAction();           
        }

        public override void Prepare ()
        {
            timeElapsedDuringDelay = 0f;
            isCalled = false;
        }

        public override void FinalizeAction (bool isFastforward = false)
        {                   
            isCalled = true;
            callback.Invoke();
            base.CallOnActionDoneEvent();
        }

        public override void ResetStatus ()
        {

        }

        #endregion

        protected virtual void Awake()
        {
            myType = ActionType.Callback;
        }


    }
}