using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.CharacterController
{
    public class MonsterRootMotionMove : ActorStateBase
    {
        private ActorFootstep[] m_ActorFootstep;

        public override bool ApplyRootMotionSetWhenEnter => true;

        public override bool CanEnter
        {
            get
            {
                return Actor.CPhysic.Grounded &&
                       Actor.MoveSpeedV != EMoveSpeedV.None &&
                       Actor.MovementAxisMagnitude > 0.1f;
            }
        }


        public MonsterRootMotionMove() : base(EStateType.Move)
        {
        }

        public override void Init(ActorStateMachine parent)
        {
            base.Init(parent);
            m_ActorFootstep = Actor.GetComponentsInChildren<ActorFootstep>();
        }

        public override void Enter()
        {
            base.Enter();
            Actor.CAnim.Play("Move");

            for (int i = 0; i < m_ActorFootstep.Length; i++)
            {
                m_ActorFootstep[i].ActiveSelf = true;
            }
        }

        public override void OnStay()
        {
            base.OnStay();

            if (Actor.MovementAxisMagnitude < 0.1f)
            {
                Exit();
                return;
            }

            int moveSpeed = (int)Actor.MoveSpeedV;
            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, Actor.MovementAxis.x * moveSpeed);
            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, Actor.MovementAxis.z * moveSpeed);

            Actor.CPhysic.AlignForwardTo(Actor.DesiredLookDir, angle => Mathf.Abs(angle) > 45 ? 1080 : 360);
        }

        protected override void OnExit()
        {
            base.OnExit();
            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, 0);

            for (int i = 0; i < m_ActorFootstep.Length; i++)
            {
                m_ActorFootstep[i].ActiveSelf = false;
            }
        }
    }
}