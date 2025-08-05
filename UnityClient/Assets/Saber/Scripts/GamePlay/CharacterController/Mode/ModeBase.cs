using System;
using System.Collections;
using System.Collections.Generic;


using UnityEngine;

namespace Saber.CharacterController
{
    public abstract class ModeBase
    {
        public virtual bool CanEnter => CurStateType == EStateType.Idle || CurStateType == EStateType.Move;
        public bool IsTriggering { get; private set; }
        public EModeType ModeType { get; private set; }
        public CharacterModes Parent { get; private set; }

        public SCharacter Actor => Parent.Actor;

        public EStateType CurStateType => Actor.CurrentStateType;
        public Action OnFinisehd { get; set; }


        public ModeBase(EModeType modeType)
        {
            ModeType = modeType;
        }

        public virtual void Init(CharacterModes parent)
        {
            Parent = parent;
        }

        public virtual void Enter()
        {
            IsTriggering = true;
        }

        public virtual void OnStay()
        {
        }

        public void Exit()
        {
            if (IsTriggering)
            {
                IsTriggering = false;
                OnExit();
            }
        }

        protected virtual void OnExit()
        {
            OnFinisehd?.Invoke();
            OnFinisehd = null;
        }

        public virtual void Release()
        {
        }

        public virtual void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
        }

        public virtual void OnAnimEnter(int nameHash, int layer)
        {
        }

        public virtual void OnAnimExit(int nameHash, int layer)
        {
        }

        public virtual void OnStateChange(EStateType from, EStateType to)
        {
        }
    }
}