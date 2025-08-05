using UnityEngine;

namespace Saber.CharacterController
{
    public class Slide : ActorStateBase
    {
        private EState m_State;

        enum EState
        {
            None,
            StartSlide,
            Sliding,
            EndSlide,
        }

        public override bool ApplyRootMotionSetWhenEnter => true;

        public override bool CanEnter
        {
            get
            {
                return Actor.CPhysic.Grounded &&
                       Actor.CPhysic.SlopeDirectionSmooth != Vector3.zero &&
                       Actor.CPhysic.SlopeDirectionSmooth.magnitude > 0.1f;
            }
        }

        public override bool CanExit => m_State == EState.EndSlide;


        public Slide() : base(EStateType.Slide)
        {
        }

        public override void Enter()
        {
            base.Enter();
            StartSlide();
        }

        public override void ReEnter()
        {
            base.ReEnter();
            StartSlide();
        }

        void StartSlide()
        {
            if (m_State != EState.StartSlide && m_State != EState.Sliding)
            {
                m_State = EState.StartSlide;
            }

            //Actor.StrafeMode = true;
        }

        public override void OnStay()
        {
            base.OnStay();

            if (StateMachine.Fall())
                return;

            if (m_State == EState.StartSlide)
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay("Slide"))
                {
                    Actor.transform.rotation = Quaternion.Euler(0, Actor.CPhysic.SlopeDirection.y, 0);
                    Actor.CAnim.Play("Slide");
                }

                m_State = EState.Sliding;
            }
            else if (m_State == EState.Sliding)
            {
                if (!Actor.CAnim.IsReallyPlaying("Slide"))
                    return;

                if (Actor.CPhysic.SlopeDirectionSmooth.magnitude > 0.1f)
                {
                    Actor.CPhysic.AlignForwardTo(Actor.CPhysic.SlopeDirection, 120);

                    Vector3 moveDir = Vector3.Project(Actor.DesiredMoveDir, Actor.transform.right);
                    Actor.CPhysic.AdditivePosition += moveDir * DeltaTime * 3f;
                }
                else
                {
                    Actor.CAnim.Play("SlideUp");
                    m_State = EState.EndSlide;
                }
            }
            else if (m_State == EState.EndSlide)
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay("SlideUp"))
                {
                    Exit();
                }
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            //Actor.StrafeMode = false;
            Actor.CPhysic.DefaultSlopeLimit();
            m_State = EState.None;
        }
    }
}