using System;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Jump : ActorStateBase
    {
        enum EState
        {
            None,
            Jump,
            FallRoll,
            Fall,
            FallLand,
            FallDie,
        }

        private const float k_JumpHorizontalSpeed = 5, k_JumpVerticalSpeed = 3;
        private EState m_State;
        private float m_StartLandSpeed;

        public override bool CanEnter => Actor.CPhysic.Grounded;
        public bool InAir { get; private set; }


        public Jump() : base(EStateType.Jump)
        {
        }

        public override void Enter()
        {
            base.Enter();

            InAir = true;
            Actor.CPhysic.EnableSlopeMovement = false;
            Actor.CAnim.Play($"Jump");
            m_State = EState.Jump;
        }

        public override void OnStay()
        {
            base.OnStay();

            if (InAir)
            {
                Actor.CPhysic.AdditivePosition += Actor.transform.forward * k_JumpHorizontalSpeed * DeltaTime;
                Actor.CPhysic.AdditivePosition += k_JumpVerticalSpeed * Vector3.up * DeltaTime;
            }

            if (m_State == EState.Jump)
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay("Jump", 0.9f))
                {
                    m_State = EState.Fall;
                    Actor.CAnim.Play("Fall");
                }
            }
            else if (m_State == EState.Fall)
            {
                if (Actor.CPhysic.Grounded)
                {
                    m_State = EState.FallRoll;
                    Actor.CAnim.Play("FallRoll");
                    m_StartLandSpeed = k_JumpHorizontalSpeed;
                    InAir = false;
                }
            }
            else if (m_State == EState.FallRoll)
            {
                if (m_StartLandSpeed > 0)
                {
                    Actor.CPhysic.AdditivePosition += Actor.transform.forward * m_StartLandSpeed * DeltaTime;
                    m_StartLandSpeed -= DeltaTime * k_JumpHorizontalSpeed * 1.5f;
                }

                if (!Actor.CAnim.IsPlayingOrWillPlay("FallRoll", 0.9f))
                {
                    Exit();
                    m_State = EState.None;
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