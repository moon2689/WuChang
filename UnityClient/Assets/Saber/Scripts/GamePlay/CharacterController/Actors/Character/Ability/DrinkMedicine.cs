using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.CharacterController
{
    public class DrinkMedicine : AbilityBase
    {
        private bool m_Breaked;

        public override bool CanEnter
        {
            get
            {
                return base.Actor.CStats.HPPotionCount > 0 &&
                       (CurStateType == EStateType.Idle ||
                        CurStateType == EStateType.Move);
            }
        }

        public DrinkMedicine() : base(EAbilityType.DrinkMedicine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Actor.CAnim.Play("DrinkPotion", 1, exitTime: 0.98f, onFinished: Exit);
            m_Breaked = false;

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
            if (eventObj.EventType == EAnimTriggerEvent.RecoverHP && !this.Actor.CStats.IsHPFull && !m_Breaked)
            {
                float hpValue = Actor.CStats.MaxHp;
                Actor.CStats.PlayHealingEffect(hpValue);
                --Actor.CStats.HPPotionCount;
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
    }
}