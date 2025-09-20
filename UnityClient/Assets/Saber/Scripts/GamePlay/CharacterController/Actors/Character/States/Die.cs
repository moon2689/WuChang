using System;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Die : ActorStateBase
    {
        private string m_CurAnim;
        private bool m_DieEndEventTriggered;

        public override bool ApplyRootMotionSetWhenEnter => true;

        public override bool CanEnter => Actor.IsDead;
        public string SpecialAnim { get; set; }


        public Die() : base(EStateType.Die)
        {
        }

        public override void Enter()
        {
            base.Enter();
            m_CurAnim = !string.IsNullOrEmpty(SpecialAnim) ? SpecialAnim : "Die";
            Actor.CAnim.Play(m_CurAnim, force: true);
            Actor.CPhysic.UseGravity = true;
            m_DieEndEventTriggered = false;
        }

        public override void OnStay()
        {
            base.OnStay();

            if (!m_DieEndEventTriggered && !Actor.CAnim.IsPlayingOrWillPlay(m_CurAnim, 0.8f))
            {
                m_DieEndEventTriggered = true;
                Actor.OnDieAnimPlayFinished();
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            SpecialAnim = null;
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.WeaponFallToGround)
            {
                Actor.CMelee.CWeapon.WeaponFallToGround();
            }
        }
    }
}