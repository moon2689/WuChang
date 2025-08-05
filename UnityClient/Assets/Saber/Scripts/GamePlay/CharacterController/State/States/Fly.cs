using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Fly : ActorStateBase
    {
        enum EState
        {
            TakeOff,

            Fly,
            Rise,

            Dodge,

            Fall,
            Land,
        }

        private EState m_State;
        private float m_Speed, m_RiseSpeedY, m_FallSpeedY;
        private string m_LandAnim;
        private float m_StopFlyGroundDistance;
        private bool m_IsDashing;
        private SCharacter m_Character;

        public override bool ApplyRootMotionSetWhenEnter => true;

        public override bool CanEnter
        {
            get
            {
                if (m_Character.CurrentStateType == EStateType.Swim)
                {
                    Swim swim = (Swim)m_Character.CStateMachine.CurrentState;
                    if (swim.IsUnderWater)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        protected override ActorBaseStats.EStaminaRecoverSpeed StaminaRecoverSpeed => ActorBaseStats.EStaminaRecoverSpeed.Fast;

        public bool IsFlying => m_State != EState.Fall && m_State != EState.Land;
        private bool IsInputingToMove => m_Character.MovementAxisMagnitude > 0.1f;


        public static bool IsTakingOff(SActor actor)
        {
            return actor.CAnim.IsPlayingOrWillPlay("FlyTakeOffInPlace");
        }

        public Fly() : base(EStateType.Fly)
        {
        }

        public override void Init(ActorStateMachine parent)
        {
            base.Init(parent);
            m_Character = base.Actor as SCharacter;
        }

        public override void Enter()
        {
            base.Enter();

            m_Character.UpdateMovementAxisAnimatorParams = false;
            m_Character.EventOnMoveSpeedVChange += OnMoveSpeedVChange;
            m_IsDashing = false;
            m_Character.CStats.CostStamina(5);

            m_Character.CRender.ToggleFlyTrailEffect(false);
            m_Character.CPhysic.UseGravity = false;

            if (m_Character.CPhysic.Grounded || m_Character.CRender.IsInWater)
            {
                m_Character.CAnim.Play("FlyTakeOffInPlace");
                m_State = EState.TakeOff;
            }
            else
            {
                m_Character.CAnim.Play("Fly");
                m_State = EState.Fly;
                m_FallSpeedY = m_Character.CPhysic.RB.velocity.y;
            }
        }

        public void StopFly()
        {
            m_Character.CPhysic.ClampGravitySpeed = 0;
            m_Character.CPhysic.GravityTime = 0;
            m_Character.CPhysic.UseGravity = true;
            m_Character.CRender.ToggleFlyTrailEffect(false);
            m_StopFlyGroundDistance = m_Character.CPhysic.GroundDistance;

            m_Character.CStateMachine.ForceFall();
        }

        public void StartRise()
        {
            if (m_State == EState.Fly)
            {
                m_State = EState.Rise;
                m_RiseSpeedY = 0;
            }
        }

        public void StopRise()
        {
            if (m_State == EState.Rise)
            {
                m_State = EState.Fly;
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            m_Character.UpdateMovementAxisAnimatorParams = true;
            m_Character.CPhysic.UseGravity = true;
            m_Character.EventOnMoveSpeedVChange -= OnMoveSpeedVChange;
            m_Character.CRender.ToggleFlyTrailEffect(false);
        }

        private void OnMoveSpeedVChange(EMoveSpeedV from, EMoveSpeedV to)
        {
            //Debug.Log($"{from} -> {to}");
            if (from != EMoveSpeedV.Sprint && to == EMoveSpeedV.Sprint)
            {
                // dash
                m_Character.CAnim.Play("FlyDash");
            }
            else if ((from == EMoveSpeedV.Sprint || from == EMoveSpeedV.Run) && to == EMoveSpeedV.None)
            {
                // dash end
                m_Character.CAnim.Play("FlyDashStop");
            }

            if (from == EMoveSpeedV.Sprint && to != EMoveSpeedV.Sprint)
            {
                m_IsDashing = false;
            }
        }

        public override void OnStay()
        {
            base.OnStay();

            if (m_State == EState.TakeOff)
            {
                if (!m_Character.CAnim.IsPlayingOrWillPlay("FlyTakeOffInPlace"))
                {
                    m_State = EState.Fly;
                }
            }
            else if (m_State == EState.Land)
            {
                if (!m_Character.CAnim.IsPlayingOrWillPlay(m_LandAnim))
                {
                    Exit();
                }

                m_Character.CPhysic.GravityExtraPower = 1;
            }
            else if (m_Character.CPhysic.Grounded)
            {
                m_State = EState.Land;

                bool flyHorFast = m_Speed > 10;
                if (flyHorFast)
                    m_LandAnim = "FlyLandSlide";
                else if (m_StopFlyGroundDistance > 5)
                    m_LandAnim = "FlyLandHard";
                else
                    m_LandAnim = "FlyLandSoft";
                m_Character.CAnim.Play(m_LandAnim);
                m_Character.CRender.ToggleFlyTrailEffect(false);

                if (!flyHorFast)
                {
                    GameApp.Entry.Game.Audio.Play3DSound("Sound/Actor/FallLand", m_Character.transform.position);
                }
            }
            else if (m_State == EState.Fly)
            {
                UpdateFlying();
            }
            else if (m_State == EState.Rise)
            {
                m_Character.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
                m_Character.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, 1);

                if (m_RiseSpeedY < 10)
                    m_RiseSpeedY += DeltaTime * 5;
                if (m_RiseSpeedY > 10)
                    m_RiseSpeedY = 10;
                m_Character.CPhysic.AdditivePosition += Vector3.up * m_RiseSpeedY * base.DeltaTime;
            }
            else if (m_State == EState.Fall)
            {
                m_Character.CPhysic.AdditivePosition += m_Character.transform.forward * DeltaTime * m_Speed;
            }
        }

        void UpdateFlying()
        {
            if (!IsInputingToMove)
            {
                if (m_RiseSpeedY > 0)
                    m_RiseSpeedY -= DeltaTime * 5;
                if (m_RiseSpeedY < 0)
                    m_RiseSpeedY = 0;
                if (m_RiseSpeedY > 0)
                    m_Character.CPhysic.AdditivePosition += Vector3.up * m_RiseSpeedY * base.DeltaTime;

                if (m_FallSpeedY < 0)
                {
                    m_FallSpeedY += DeltaTime * 20;
                }

                if (m_FallSpeedY > 0)
                {
                    m_FallSpeedY = 0;
                }

                if (m_FallSpeedY < 0)
                {
                    m_Character.CPhysic.AdditivePosition += Vector3.up * m_FallSpeedY * base.DeltaTime;
                }

                m_Speed = 0;
                m_Character.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
                m_Character.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, 0);
                m_Character.CRender.ToggleFlyTrailEffect(false);
                return;
            }

            EMoveSpeedV speed = m_Character.MoveSpeedV;

            bool isBack = m_Character.MovementAxis.z < 0;
            if (isBack)
            {
                speed = EMoveSpeedV.Walk;
            }
            else if (speed == EMoveSpeedV.Sprint)
            {
                if (m_IsDashing && m_Character.CStats.CurrentStamina > 0)
                    m_Character.CStats.CostStamina(5 * DeltaTime);
                else
                    speed = EMoveSpeedV.Run;
            }

            // 朝向
            float turnSpeed = speed == EMoveSpeedV.Sprint ? 60 : 240;
            Vector3 forwardDir = isBack ? -m_Character.DesiredMoveDir : m_Character.DesiredMoveDir;
            float angle = m_Character.CPhysic.AlignForwardTo(forwardDir, angle => angle > 60 ? turnSpeed * 2 : turnSpeed);
            Vector3 flyDir = new Vector3(m_Character.transform.forward.x, m_Character.DesiredLookDirIn3D.y, m_Character.transform.forward.z);

            float paramH = 0;
            int paramV = (int)speed;
            if (isBack) // back
            {
                paramV = -1;
                m_Speed = -3;
            }
            else if (speed == EMoveSpeedV.Walk)
            {
                if (angle > 0.1f)
                    paramH = Mathf.Clamp01(angle / 20f);
                else if (angle < -0.1f)
                    paramH = -Mathf.Clamp01(Mathf.Abs(angle) / 20f);

                m_Speed = 5;
            }
            else if (speed == EMoveSpeedV.Run)
            {
                paramH = Vector3.Dot(flyDir, Vector3.up) > 0 ? 1 : -1;
                m_Speed = 10;
            }
            else if (speed == EMoveSpeedV.Sprint)
            {
                m_Speed = 30;
            }

            m_Character.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, paramH);
            m_Character.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, paramV);

            // 位移
            Vector3 dirInHor = Vector3.ProjectOnPlane(flyDir, Vector3.up);
            float angleToHor = Vector3.Angle(flyDir, dirInHor);
            Vector3 fixedFlyDir = flyDir;
            if (angleToHor < 20)
                fixedFlyDir.y = 0;
            fixedFlyDir.Normalize();
            m_Character.CPhysic.AdditivePosition += fixedFlyDir * m_Speed * base.DeltaTime;
            SDebug.DrawArrow(m_Character.transform.position, fixedFlyDir * m_Speed, Color.green);

            m_Character.CRender.ToggleFlyTrailEffect(speed == EMoveSpeedV.Run || speed == EMoveSpeedV.Sprint);
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.FlyDash)
            {
                m_IsDashing = true;
            }
        }
    }
}