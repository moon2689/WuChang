using System;
using System.Collections;
using System.Collections.Generic;


using UnityEngine;

namespace Saber.CharacterController
{
    public abstract class AbilityBase
    {
        public virtual bool CanEnter => CurStateType == EStateType.Idle || CurStateType == EStateType.Move;
        public bool IsTriggering { get; private set; }
        public EAbilityType AbilityType { get; private set; }
        public CharacterAbility Parent { get; private set; }

        public SCharacter Actor => Parent.Actor;

        public EStateType CurStateType => Actor.CurrentStateType;
        public Action OnFinisehd { get; set; }


        public AbilityBase(EAbilityType abilityType)
        {
            AbilityType = abilityType;
        }

        public virtual void Init(CharacterAbility parent)
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