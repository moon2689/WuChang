using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class EnchantWeapon : AbilityBase
    {
        private bool m_Breaked;
        private bool m_CanExit;

        public override bool CanEnter
        {
            get
            {
                return base.Actor.CStats.HPPotionCount > 0 &&
                       (CurStateType == EStateType.Idle ||
                        CurStateType == EStateType.Move);
            }
        }

        public EEnchantedMagic Magic { get; set; }
        public float HoldSeconds { get; set; }

        public EnchantWeapon() : base(EAbilityType.EnchantWeapon)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Actor.CAnim.Play("EnchantedMiaoDao", 2, onFinished: Exit);
            m_Breaked = false;
            m_CanExit = false;

            Actor.MaxMoveSpeedV = EMoveSpeedV.Walk;
        }

        protected override void OnExit()
        {
            base.OnExit();

            Actor.MaxMoveSpeedV = EMoveSpeedV.Sprint;
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.Enchanted)
            {
                if (!m_Breaked)
                {
                    Actor.CMelee.EnchantedByItem(Magic, HoldSeconds);
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