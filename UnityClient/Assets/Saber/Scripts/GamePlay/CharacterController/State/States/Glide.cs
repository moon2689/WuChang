using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Glide : ActorStateBase
    {
        enum EState
        {
            Glide,
            StopGlide,
            Fall,
            Land,
        }

        private EState m_State;
        private float m_HorSpeed;
        private float m_GravityExtraPower;
        private string m_LandAnim;
        private float m_StopGlideGroundDistance;
        private SCharacter Character;

        public override bool ApplyRootMotionSetWhenEnter => true;

        public override bool CanEnter => Character.CStats.CurrentStamina > 0 && !Character.CPhysic.Grounded && Character.CPhysic.GroundDistance > 2;

        protected override ActorBaseStats.EStaminaRecoverSpeed StaminaRecoverSpeed => ActorBaseStats.EStaminaRecoverSpeed.Slow;

        private Vector3 FlyDir => Character.DesiredLookDir != Vector3.zero ? Character.DesiredLookDir : Character.transform.forward;
        public bool IsGliding => m_State == EState.Glide;

        public Glide() : base(EStateType.Glide)
        {
        }

        public override void Init(ActorStateMachine parent)
        {
            base.Init(parent);
            Character = base.Actor as SCharacter;
        }

        public override void Enter()
        {
            base.Enter();
            Actor.CStats.CostStamina(5);

            StartGlide();
        }

        public override void ReEnter()
        {
            base.ReEnter();
            StartGlide();
        }

        void StartGlide()
        {
            Character.CAnim.Play("GlideForward");
            m_State = EState.Glide;
            //Character.CRender.ToggleFlyTrailEffect(true);
            Character.CPhysic.ResetGravityValues();
            Character.CPhysic.ClampGravitySpeed = 20;
            //Debug.Log("enter Glide");
        }

        public void StopGlide()
        {
            //Debug.Log("StopGlide");
            if (m_State == EState.Glide)
            {
                m_State = EState.StopGlide;
                Character.CAnim.Play("GlideStop");
                Character.CPhysic.GravityTime = 0;
            }

            Character.CPhysic.ClampGravitySpeed = 0;

            //Character.CRender.ToggleFlyTrailEffect(false);
        }

        protected override void OnExit()
        {
            base.OnExit();
            Character.CPhysic.ResetGravityValues();
            Character.CPhysic.ClampGravitySpeed = 0;
            //Character.CRender.ToggleFlyTrailEffect(false);
        }

        public override void OnStay()
        {
            base.OnStay();

            if (m_State == EState.Land)
            {
                if (!Character.CAnim.IsPlayingOrWillPlay(m_LandAnim))
                {
                    Exit();
                }

                Character.CPhysic.GravityExtraPower = 1;
            }
            else if (Character.CPhysic.Grounded)
            {
                bool glideHorFast = m_HorSpeed > 10;
                if (glideHorFast)
                    m_LandAnim = "FlyLandSlide";
                else if (m_StopGlideGroundDistance > 5)
                    m_LandAnim = "FlyLandHard";
                else
                    m_LandAnim = "FlyLandSoft";
                Character.CAnim.Play(m_LandAnim);
                m_State = EState.Land;
                //Character.CRender.ToggleFlyTrailEffect(false);

                if (!glideHorFast)
                {
                    GameApp.Entry.Game.Audio.Play3DSound("Sound/Actor/FallLand", Character.transform.position);
                }
            }
            else if (m_State == EState.Glide)
            {
                m_HorSpeed = 15;
                m_GravityExtraPower = 0.3f;
                Character.CPhysic.GravityExtraPower = m_GravityExtraPower;
                Character.CPhysic.AdditivePosition += Character.transform.forward * DeltaTime * m_HorSpeed;
                Character.CPhysic.AlignForwardTo(FlyDir, 120);
            }
            else if (m_State == EState.StopGlide)
            {
                m_HorSpeed -= DeltaTime * 5;
                if (m_GravityExtraPower < 1)
                    m_GravityExtraPower += DeltaTime * 2f;
                else
                    m_GravityExtraPower = 1;

                Character.CPhysic.GravityExtraPower = m_GravityExtraPower;
                Character.CPhysic.AdditivePosition += Character.transform.forward * DeltaTime * m_HorSpeed;
                m_StopGlideGroundDistance = Character.CPhysic.GroundDistance;

                if (Character.CAnim.IsPlayingOrWillPlay("GlideFall"))
                {
                    m_State = EState.Fall;
                    //Character.CRender.ToggleFlyTrailEffect(false);
                }
            }
            else if (m_State == EState.Fall)
            {
                Character.CPhysic.GravityExtraPower = 1;
                Character.CPhysic.AdditivePosition += Character.transform.forward * DeltaTime * m_HorSpeed;
            }
        }
    }
}