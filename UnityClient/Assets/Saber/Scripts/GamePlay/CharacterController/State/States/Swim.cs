using UnityEngine;

namespace Saber.CharacterController
{
    public class Swim : ActorStateBase
    {
        private const float k_SwimSpeedNormal = 2, k_SwimSpeedFast = 4;
        private float m_WaterLevelY; //WaterLine postion
        private bool m_UnderWaterLastFrame;
        private float m_EnterWaterSpeedY;
        private SCharacter Character;


        private bool IsInputingToMove => Character.MovementAxisMagnitude > 0.1f;

        protected override ActorBaseStats.EStaminaRecoverSpeed StaminaRecoverSpeed
        {
            get
            {
                return IsInputingToMove
                    ? ActorBaseStats.EStaminaRecoverSpeed.Stop
                    : ActorBaseStats.EStaminaRecoverSpeed.Slow;
            }
        }

        public override bool CanEnter
        {
            get
            {
                if (!IsUnderWater && Fly.IsTakingOff(base.Actor))
                {
                    return false;
                }

                return WhetherSwim();
            }
        }

        private Vector3 WaterPivotPoint => Character.GetNodeTransform(ENodeType.Neck).position;
        public bool IsUnderWater => m_UnderWaterLastFrame;


        public Swim() : base(EStateType.Swim)
        {
        }

        public override void Init(ActorStateMachine parent)
        {
            base.Init(parent);
            Character = base.Actor as SCharacter;
        }

        /// <summary>Check if the character in a water surface</summary>
        bool WhetherSwim()
        {
            Vector3 upPoint = WaterPivotPoint + Vector3.up * 20;
            var rayLength = upPoint.y - Character.transform.position.y;
            int waterLayer = EStaticLayers.Water.GetLayerMask();

            if (Physics.Raycast(upPoint, Vector3.down, out RaycastHit waterHit, rayLength, waterLayer,
                    QueryTriggerInteraction.Collide))
            {
                m_WaterLevelY = waterHit.point.y; //Find the water Level
                float disToSurface = WaterPivotPoint.y - m_WaterLevelY;
                return disToSurface < -0.1f;
            }
            else
            {
                return false;
            }
        }

        public override void Enter()
        {
            base.Enter();

            Character.CPhysic.UseGravity = false;
            Character.CPhysic.Force_Reset();
            Character.CPhysic.SetPlatform(null);
            Character.CPhysic.EnableSlopeMovement = false;
            //Character.CRender.ToggleEyeLock(false);
            Character.UpdateMovementAxisAnimatorParams = false;
            m_UnderWaterLastFrame = false;

            m_EnterWaterSpeedY = Character.CPhysic.DeltaPos.y / Character.DeltaTime;
            //Debug.Log($"swim,DeltaPos:{Actor.CPhysic.DeltaPos}  DeltaVelocity:{Actor.CPhysic.DeltaVelocity}  m_EnterWaterSpeedY:{m_EnterWaterSpeedY}");
        }

        protected override void OnExit()
        {
            base.OnExit();
            Character.CPhysic.UseGravity = true;
            Character.CPhysic.EnableSlopeMovement = true;
            //Character.CRender.ToggleEyeLock(true);
            Character.UpdateMovementAxisAnimatorParams = true;
            m_UnderWaterLastFrame = false;
        }

        public override void OnStay()
        {
            base.OnStay();
            if (m_EnterWaterSpeedY < 0)
                EnterSwim();
            else if (!UpdateSwim())
                Exit();

            Actor.CStats.StaminaRecoverSpeed = StaminaRecoverSpeed;
        }

        void EnterSwim()
        {
            if (m_EnterWaterSpeedY >= 0)
            {
                return;
            }

            if (Character.CPhysic.Grounded)
            {
                m_EnterWaterSpeedY = 0;
                return;
            }

            Character.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
            Character.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, 0);

            m_EnterWaterSpeedY += Character.DeltaTime * 100;
            Character.CPhysic.AdditivePosition += new Vector3(0, m_EnterWaterSpeedY) * DeltaTime;

            if (!Character.CAnim.IsPlayingOrWillPlay("SwimUnder"))
            {
                Character.CAnim.Play("SwimUnder");
            }
        }

        // 游泳
        bool UpdateSwim()
        {
            float disToSurface = WaterPivotPoint.y - m_WaterLevelY;
            bool underWater = disToSurface < -0.3f;

            // 在水下游泳时没有水波纹
            if (m_UnderWaterLastFrame != underWater)
            {
                m_UnderWaterLastFrame = underWater;
                Character.CRender.ActiveWaterWave(!underWater);
            }

            // 设置动画参数
            if (IsInputingToMove)
            {
                // 转向
                bool isSprint = Character.MoveSpeedV == EMoveSpeedV.Sprint && Character.CStats.CurrentStamina > 0;
                if (isSprint && !underWater)
                {
                    Character.CStats.CostStamina(10 * DeltaTime);
                }

                float turnSpeed = isSprint || underWater ? 60 : 120;
                float angle = Character.CPhysic.AlignForwardTo(Character.DesiredMoveDir,
                    angle => angle > 60 ? turnSpeed * 2 : turnSpeed);

                float paramH;
                if (angle > 0.1f)
                    paramH = Mathf.Clamp01(angle / 20f);
                else if (angle < -0.1f)
                    paramH = -Mathf.Clamp01(Mathf.Abs(angle) / 20f);
                else
                    paramH = 0;
                float paramV = isSprint && !underWater ? 2 : 1;

                Character.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, paramH);
                Character.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, paramV);
            }
            else
            {
                Character.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
                Character.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, 0);
            }

            // 位移
            Vector3 swimSpeed = GetSwimSpeed(underWater, out string tarAnim);
            if (swimSpeed != Vector3.zero)
                Character.CPhysic.AdditivePosition += swimSpeed * base.DeltaTime;

            // 不能超出水面
            if (disToSurface > 0)
            {
                Vector3 posFixed = disToSurface * Vector3.up;
                Character.CPhysic.AdditivePosition -= Vector3.Lerp(Vector3.zero, posFixed, DeltaTime * 10);
            }

            // 播放动画
            if (!Character.CAnim.IsPlayingOrWillPlay(tarAnim))
            {
                Character.CAnim.Play(tarAnim);
            }

            return disToSurface < 0.2f;
        }

        Vector3 GetSwimSpeed(bool underWater, out string tarAnim)
        {
            if (!IsInputingToMove)
            {
                if (underWater)
                {
                    tarAnim = "SwimUnder";
                    return Vector3.zero;
                }
                else
                {
                    // 在水表面附近，则浮出水面
                    tarAnim = "SwimSurface";
                    return Character.transform.up * 0.1f;
                }
            }

            Vector3 dirInHor = Vector3.ProjectOnPlane(Character.DesiredLookDirIn3D, Vector3.up);
            float angleToHor = Vector3.Angle(Character.DesiredLookDirIn3D, dirInHor);
            if (Character.DesiredLookDirIn3D.y < 0)
                angleToHor *= -1;
            //Debug.Log($"angleToHor:{angleToHor}");

            Vector3 forward;
            if (angleToHor > 20 && underWater)
            {
                // 向上游
                forward = Character.transform.forward + Character.transform.up;
                forward.Normalize();
                tarAnim = "SwimUnderUp";
            }
            else if (angleToHor < -45)
            {
                // 向下游
                forward = Character.transform.forward - Character.transform.up;
                forward.Normalize();
                tarAnim = "SwimUnderDown";
            }
            else if (underWater)
            {
                // 在水下
                forward = Character.transform.forward;
                tarAnim = "SwimUnder";
            }
            else
            {
                // 在水表面附近，则浮出水面
                forward = Character.transform.forward + Character.transform.up * 0.1f;
                forward.Normalize();
                tarAnim = "SwimSurface";
            }

            bool isSprint = Character.MoveSpeedV == EMoveSpeedV.Sprint;
            float speed = isSprint && !underWater ? k_SwimSpeedFast : k_SwimSpeedNormal;
            return forward * speed;
        }
    }
}