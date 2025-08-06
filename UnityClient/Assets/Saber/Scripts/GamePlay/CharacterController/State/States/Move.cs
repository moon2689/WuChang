using UnityEngine;

namespace Saber.CharacterController
{
    public class Move : ActorStateBase, ISkillCanTrigger
    {
        private ActorFootstep[] m_ActorFootstep;
        private string m_EndAnim;
        private SCharacter m_Character;


        public override bool CanEnter
        {
            get
            {
                return Actor.CPhysic.Grounded &&
                       Actor.MoveSpeedV != EMoveSpeedV.None &&
                       Actor.MovementAxisMagnitude > 0;
            }
        }

        protected override ActorBaseStats.EStaminaRecoverSpeed StaminaRecoverSpeed
        {
            get
            {
                return Actor.MoveSpeedV switch
                {
                    EMoveSpeedV.Sprint => ActorBaseStats.EStaminaRecoverSpeed.Stop,
                    EMoveSpeedV.Run => ActorBaseStats.EStaminaRecoverSpeed.Fast,
                    EMoveSpeedV.Walk => ActorBaseStats.EStaminaRecoverSpeed.Fast,
                    _ => ActorBaseStats.EStaminaRecoverSpeed.Stop,
                };
            }
        }

        public SCharacter Character => m_Character ??= (SCharacter)Actor;
        private bool IsMoveFree => Actor.AI.LockingEnemy == null || Actor.MoveSpeedV == EMoveSpeedV.Sprint;


        public override void Init(ActorStateMachine parent)
        {
            base.Init(parent);
            m_ActorFootstep = Actor.GetComponentsInChildren<ActorFootstep>();
        }

        public Move() : base(EStateType.Move)
        {
        }

        public override void Enter()
        {
            base.Enter();

            Actor.CAnim.ResetSmoothFloat(EAnimatorParams.Horizontal, 0);
            Actor.CAnim.ResetSmoothFloat(EAnimatorParams.Vertical, 0);

            Actor.UpdateMovementAxisAnimatorParams = false;

            for (int i = 0; i < m_ActorFootstep.Length; i++)
            {
                m_ActorFootstep[i].ActiveSelf = true;
            }
        }

        private bool TryEndMove()
        {
            if (m_EndAnim.IsNotEmpty())
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay(m_EndAnim, 0.3f))
                {
                    Exit();
                }

                return true;
            }

            // 无输入则认为移动自动结束
            if (Actor.MovementAxisMagnitude < 0.1f)
            {
                // 播放急停动画
                m_EndAnim = GetEndAnim();
                if (m_EndAnim.IsNotEmpty())
                {
                    Actor.CAnim.Play(m_EndAnim, blendTime: 0.1f);
                    Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
                    Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, 0);
                }
                else
                {
                    Exit();
                }

                return true;
            }

            return false;
        }

        private string GetEndAnim()
        {
            // 跑或冲刺结束时的急停
            float curSpeedV = Actor.CAnim.GetCurSmoothFloat(EAnimatorParams.Vertical);
            float curSpeedH = Actor.CAnim.GetCurSmoothFloat(EAnimatorParams.Horizontal);
            //Debug.Log($"ArmedWeapon:{Actor.ArmedWeapon} curSpeedH:{curSpeedH},curSpeedV:{curSpeedV}");
            if (IsMoveFree)
            {
                if (curSpeedV > 2f)
                {
                    return "SprintEnd";
                }
                else if (curSpeedH * curSpeedH + curSpeedV * curSpeedV > 1)
                {
                    return "RunEnd";
                }
            }

            return null;
        }

        float GetSpeedVertical()
        {
            float curSmoothFloat = Actor.CAnim.GetCurSmoothFloat(EAnimatorParams.Vertical);
            return GetSpeed(curSmoothFloat);
        }

        float GetSpeed(float curSmoothFloat)
        {
            float speed = 0;
            if (curSmoothFloat <= 1)
            {
                speed = Mathf.Lerp(0, Character.m_CharacterInfo.m_SpeedWalk, curSmoothFloat);
            }
            else if (curSmoothFloat <= 2)
            {
                speed = Mathf.Lerp(Character.m_CharacterInfo.m_SpeedWalk, Character.m_CharacterInfo.m_SpeedRun,
                    curSmoothFloat - 1);
            }
            else if (curSmoothFloat <= 3)
            {
                speed = Mathf.Lerp(Character.m_CharacterInfo.m_SpeedRun, Character.m_CharacterInfo.m_SpeedSprint,
                    curSmoothFloat - 2);
            }

            return speed;
        }

        float GetSpeed2D()
        {
            float curSmoothFloatH = Actor.CAnim.GetCurSmoothFloat(EAnimatorParams.Horizontal);
            float curSmoothFloatV = Actor.CAnim.GetCurSmoothFloat(EAnimatorParams.Vertical);
            float curSmoothFloat = Mathf.Sqrt(curSmoothFloatH * curSmoothFloatH + curSmoothFloatV * curSmoothFloatV);
            // float curSmoothFloat = 2 * GameHelper.GetStickLength(curSmoothFloatH * 0.5f, curSmoothFloatV * 0.5f);
            return GetSpeed(curSmoothFloat);
        }

        public override void OnStay()
        {
            base.OnStay();

            if (StateMachine.Fall())
            {
                return;
            }

            Actor.CPhysic.ApplyRootMotion = m_EndAnim.IsNotEmpty();

            // 当未输入时，结束移动并退出
            if (TryEndMove())
            {
                return;
            }

            // 移动
            if (IsMoveFree)
            {
                UpdateMoveFree(); //当冲刺或者未锁定敌人时，玩家朝向移动方向而移动
            }
            else
            {
                UpdateMoveLock(); //当锁定敌人，并且未冲刺时，玩家始终面朝向敌人
            }

            Actor.CStats.StaminaRecoverSpeed = StaminaRecoverSpeed;
        }


        /// <summary>当冲刺或者未锁定敌人时，玩家朝向移动方向而移动</summary>
        void UpdateMoveFree()
        {
            int moveSpeed = (int)Actor.MoveSpeedV;
            bool isSprint = Actor.MoveSpeedV == EMoveSpeedV.Sprint;
            float verticalValue = Actor.MovementAxisMagnitude * moveSpeed;

            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, verticalValue);

            if (isSprint)
            {
                // 若体力为0，则等待体力恢复一定值再冲刺，否则抖动
                Actor.CStats.CostStamina(10 * base.DeltaTime);
                if (Actor.CStats.CurrentStamina <= 0)
                {
                    Actor.WaitStaminaRecoverBeforeSprint = true;
                }
            }

            // 位移
            float speed = GetSpeedVertical();
            //Actor.CPhysic.AdditivePosition += Actor.DesiredMoveDir * speed * base.DeltaTime;
            Actor.CPhysic.AdditivePosition += Actor.transform.forward * speed * base.DeltaTime;

            Actor.CPhysic.AlignForwardTo(Actor.DesiredMoveDir, verticalValue > 2 ? 360 : 720);

            // 播放移动动画
            Actor.CAnim.Play("MoveFree");
        }

        /// <summary>当锁定敌人，并且未冲刺时，玩家始终面朝向敌人</summary>
        void UpdateMoveLock()
        {
            int moveSpeed = (int)Actor.MoveSpeedV;
            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, Actor.MovementAxis.x * moveSpeed);
            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, Actor.MovementAxis.z * moveSpeed);

            // 位移
            float speed = GetSpeed2D();
            Actor.CPhysic.AdditivePosition += Actor.DesiredMoveDir * speed * base.DeltaTime;

            Actor.CPhysic.AlignForwardTo(Actor.DesiredLookDir, 720);

            // 播放移动动画
            Actor.CAnim.Play("Move");
        }

        protected override void OnExit()
        {
            base.OnExit();

            m_EndAnim = null;

            Actor.UpdateMovementAxisAnimatorParams = true;

            for (int i = 0; i < m_ActorFootstep.Length; i++)
            {
                m_ActorFootstep[i].ActiveSelf = false;
            }
        }

        bool ISkillCanTrigger.CanTriggerSkill(SkillItem skill)
        {
            if (skill.m_TriggerCondition == ETriggerCondition.InSprint)
            {
                return Actor.MoveSpeedV == EMoveSpeedV.Sprint;
            }
            else if (skill.m_TriggerCondition == ETriggerCondition.InGround)
            {
                return Actor.MoveSpeedV != EMoveSpeedV.Sprint;
            }

            return false;
        }
    }
}