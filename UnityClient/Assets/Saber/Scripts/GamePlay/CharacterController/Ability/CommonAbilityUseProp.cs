using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public abstract class CommonAbilityUseProp : AbilityBase
    {
        public Action OnUse;

        private bool m_Breaked;
        private bool m_CanExit;

        public override bool CanEnter => CurStateType == EStateType.Idle || CurStateType == EStateType.Move;

        protected abstract string AnimName { get; }


        public CommonAbilityUseProp(EAbilityType abilityType) : base(abilityType)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Actor.CAnim.Play(AnimName, 1, onFinished: Exit);
            m_Breaked = false;
            m_CanExit = false;

            Actor.MaxMoveSpeedV = EMoveSpeedV.Walk;
        }

        protected override void OnExit()
        {
            base.OnExit();

            Actor.MaxMoveSpeedV = EMoveSpeedV.Sprint;
            OnUse = null;
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.UseItem)
            {
                if (!m_Breaked)
                {
                    OnUse?.Invoke();
                }
            }
            else if (eventObj.EventType == EAnimTriggerEvent.AnimCanExit)
            {
                m_CanExit = true;
            }
        }

        public override void OnStateChange(EStateType from, EStateType to)
        {
            base.OnStateChange(from, to);
            if (to != EStateType.Move && to != EStateType.Idle)
            {
                Actor.CAnim.StopMaskLayerAnims();
                m_Breaked = true;
            }
        }

        public override bool CanSwitchTo(EStateType to)
        {
            if (m_CanExit)
            {
                return true;
            }

            return base.CanSwitchTo(to);
        }
    }
}