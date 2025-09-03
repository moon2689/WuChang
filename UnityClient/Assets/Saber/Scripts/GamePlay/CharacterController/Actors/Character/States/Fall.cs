using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Fall : ActorStateBase
    {
        private Vector3 m_HorizontalSpeed;

        private bool m_InAir;
        private string m_LandAnim;
        private float m_StartFallPosY;
        private Vector3 m_LandSpeedDir;
        private float m_StartLandSpeed;

        public override bool ApplyRootMotionSetWhenEnter => true;

        public override bool CanEnter
        {
            get { return !Actor.CPhysic.Grounded && Actor.CPhysic.GroundDistance > 1; }
        }

        public bool PlayFallAnim { get; set; }

        public Fall() : base(EStateType.Fall)
        {
        }

        public override void Enter()
        {
            base.Enter();
            m_InAir = true;
            m_StartFallPosY = Actor.transform.position.y;

            if (PlayFallAnim)
                Actor.CAnim.Play("Fall");

            //Debug.Log($"fall, {Parent.PreStateType}");
            //m_HorizontalSpeed = Vector3.ProjectOnPlane(Actor.CPhysic.RB.velocity, Vector3.up);
            m_HorizontalSpeed = Actor.CPhysic.DeltaPos / DeltaTime;
            m_HorizontalSpeed.y = 0;

            Actor.CPhysic.EnableSlopeMovement = false;
        }

        public override void OnStay()
        {
            base.OnStay();

            // 空中
            if (m_InAir)
            {
                if (Actor.CPhysic.Grounded)
                {
                    //float fallHeight = m_StartFallPosY - Actor.transform.position.y;
                    m_LandAnim = "Land";
                    Actor.CAnim.Play(m_LandAnim);
                    m_InAir = false;

                    m_StartLandSpeed = m_HorizontalSpeed.magnitude * 0.5f;
                    m_LandSpeedDir = m_HorizontalSpeed.normalized;
                }

                Actor.CPhysic.AdditivePosition += m_HorizontalSpeed * DeltaTime;

                // 控制
                if (Actor.MovementAxisMagnitude > 0.1f)
                {
                    Actor.CPhysic.ControlInAir();
                }
            }
            // 着落中
            else
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay(m_LandAnim))
                    Exit();

                if (m_StartLandSpeed > 0)
                {
                    Actor.CPhysic.AdditivePosition += m_LandSpeedDir * m_StartLandSpeed * DeltaTime;
                    m_StartLandSpeed -= DeltaTime * 3f;
                }
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            Actor.CPhysic.EnableSlopeMovement = true;
        }
    }
}