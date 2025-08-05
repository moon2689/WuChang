using System.Collections.Generic;
using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterJump : ActorStateBase
    {
        enum EState
        {
            None,
            Jump,
            Fall,
            Land,
        }

        private const float k_JumpHorizontalSpeed = 3, k_JumpVerticalSpeed = 3;
        private EState m_State;
        private Vector3 m_JumpDir;

        public override bool CanEnter => Actor.CPhysic.Grounded;
        public bool InAir { get; private set; }
        public Vector3 JumpAxis { get; set; }


        public MonsterJump() : base(EStateType.Jump)
        {
        }


        public override void Enter()
        {
            base.Enter();

            InAir = true;
            Actor.CPhysic.EnableSlopeMovement = false;
            Actor.CAnim.Play($"JumpStart");
            m_State = EState.Jump;
            m_JumpDir = Quaternion.LookRotation(Actor.DesiredLookDir) * JumpAxis;
        }

        public override void OnStay()
        {
            base.OnStay();

            if (InAir)
            {
                Actor.CPhysic.AdditivePosition += m_JumpDir * k_JumpHorizontalSpeed * DeltaTime;
                Actor.CPhysic.AdditivePosition += k_JumpVerticalSpeed * Vector3.up * DeltaTime;
            }

            if (m_State == EState.Jump)
            {
                if (Actor.CAnim.IsPlayingOrWillPlay("JumpLoop"))
                {
                    m_State = EState.Fall;
                }

                Actor.CPhysic.AlignForwardTo(Actor.DesiredLookDir, 360);
            }
            else if (m_State == EState.Fall)
            {
                if (Actor.CPhysic.Grounded)
                {
                    m_State = EState.Land;
                    Actor.CAnim.Play("JumpEnd");
                    InAir = false;
                }
            }
            else if (m_State == EState.Land)
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay("JumpEnd"))
                {
                    Exit();
                }
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            Actor.CPhysic.EnableSlopeMovement = true;
            m_State = EState.None;
        }
    }
}