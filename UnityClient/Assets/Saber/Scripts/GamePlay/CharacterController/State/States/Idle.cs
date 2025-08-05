using System;
using System.Collections;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Idle : ActorStateBase
    {
        public override bool ApplyRootMotionSetWhenEnter => false;

        //private bool m_IsFalling;
        private SCharacter Character;

        protected override ActorBaseStats.EStaminaRecoverSpeed StaminaRecoverSpeed =>
            ActorBaseStats.EStaminaRecoverSpeed.Fast;


        public Idle() : base(EStateType.Idle)
        {
        }

        public override void Init(ActorStateMachine parent)
        {
            base.Init(parent);
            Character = base.Actor as SCharacter;
        }

        /*
        public override void Enter()
        {
            base.Enter();

            m_IsFalling = Character.CAnim.IsName("FallArmed", 0) || Character.CAnim.IsName("FallUnarmed", 0);
        }

        public override void OnStay()
        {
            base.OnStay();
            
            if (StateMachine.Fall())
                return;
            
            if (m_IsFalling && Character.CPhysic.Grounded)
            {
                Character.CAnim.Play("LandSoft" + AnimEndStringByArmed);
                m_IsFalling = false;
            }
        }

        public void PlayTalkingGesture()
        {
            int ranIndex = UnityEngine.Random.Range(1, 5);
            Character.CAnim.Play("TalkingGesture" + ranIndex);
        }
        */

        public void BranchRepair(Action onPlayed)
        {
            Character.CAnim.Play("BranchRepair", exitTime: 0.6f, onFinished: onPlayed);
        }

        public void BranchRest()
        {
            Character.CAnim.Play("BranchRest");
        }

        public void BranchRestEnd()
        {
            Character.CAnim.Play("BranchRestEnd");
        }

        public void BranchTeleport()
        {
            Character.CAnim.Play("BranchTeleport");
        }

        public void GoHome()
        {
            Character.CAnim.Play("GoHome");
        }
    }
}