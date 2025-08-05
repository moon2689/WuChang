using UnityEngine;

namespace Saber.CharacterController
{
    public class Move : ActorStateBase, ISkillCanTrigger
    {
        private const float k_NoInputEndTime = 0.04f;

        private ActorFootstep[] m_ActorFootstep;
        private string m_PlayingMoveStartAnim;
        private string m_SprintTurnAnim;
        private string m_EndAnim;
        private float m_TimerCheckEnd;
        private Vector3 m_FrontDir;
        private SCharacter m_Character;
        private bool m_ApplyRootMotion;


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

            m_TimerCheckEnd = k_NoInputEndTime;

            Actor.UpdateMovementAxisAnimatorParams = false;

            // 播放冲刺时的起始动画
            if (Actor.MoveSpeedV == EMoveSpeedV.Sprint)
            {
                float curSpeedV = Actor.CAnim.GetCurSmoothFloat(EAnimatorParams.Vertical);
                if (curSpeedV < 0.5)
                {
                    GameHelper.EDir4 dir = Actor.DesiredMoveDir.Calc4Dir(Actor.transform.forward, out _);
                    m_PlayingMoveStartAnim = $"Sprint{dir}Start";
                    Actor.CAnim.Play(m_PlayingMoveStartAnim, force: true);
                }
            }

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
            //if (!Actor.IsDraggingMovementAxis)
            if (Actor.MovementAxisMagnitude < 0.1f)
            {
                m_TimerCheckEnd -= base.DeltaTime;
                if (m_TimerCheckEnd < 0)
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
            }
            else
            {
                m_TimerCheckEnd = k_NoInputEndTime;
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

            if (m_PlayingMoveStartAnim.IsNotEmpty() && !Actor.CAnim.IsPlayingOrWillPlay(m_PlayingMoveStartAnim, 0.6f))
            {
                m_PlayingMoveStartAnim = null;
            }

            if (m_SprintTurnAnim.IsNotEmpty() && !Actor.CAnim.IsPlayingOrWillPlay(m_SprintTurnAnim))
            {
                m_SprintTurnAnim = null;
            }

            // 当播放移动动画时，关闭root motion，使用程序控制位移
            // 当播放其它动画，如转向等时，使用root motion控制
            m_ApplyRootMotion = m_SprintTurnAnim.IsNotEmpty() || m_PlayingMoveStartAnim.IsNotEmpty();
            Actor.CPhysic.ApplyRootMotion = m_ApplyRootMotion;

            if (StateMachine.Fall())
            {
                return;
            }

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

            // 转向
            if (m_PlayingMoveStartAnim.IsEmpty() && m_SprintTurnAnim.IsEmpty())
            {
                Actor.CPhysic.AlignForwardTo(m_FrontDir, 720);
            }

            Actor.CStats.StaminaRecoverSpeed = StaminaRecoverSpeed;
        }


        /// <summary>当冲刺或者未锁定敌人时，玩家朝向移动方向而移动</summary>
        void UpdateMoveFree()
        {
            int moveSpeed = (int)Actor.MoveSpeedV;
            bool isSprint = Actor.MoveSpeedV == EMoveSpeedV.Sprint;

            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, Actor.MovementAxisMagnitude * moveSpeed);
            m_FrontDir = Actor.DesiredMoveDir;

            if (isSprint)
            {
                // 冲刺中的急转向
                TryPlaySprintTurnAnim();

                // 若体力为0，则等待体力恢复一定值再冲刺，否则抖动
                Actor.CStats.CostStamina(10 * base.DeltaTime);
                if (Actor.CStats.CurrentStamina <= 0)
                {
                    Actor.WaitStaminaRecoverBeforeSprint = true;
                }
            }

            // 位移
            if (!m_ApplyRootMotion)
            {
                float speed = GetSpeedVertical();
                Actor.CPhysic.AdditivePosition += Actor.DesiredMoveDir * speed * base.DeltaTime;
            }

            // 播放移动动画
            if (m_PlayingMoveStartAnim.IsEmpty() && m_SprintTurnAnim.IsEmpty())
            {
                Actor.CAnim.Play("MoveFree");
            }
        }

        /// <summary>当锁定敌人，并且未冲刺时，玩家始终面朝向敌人</summary>
        void UpdateMoveLock()
        {
            int moveSpeed = (int)Actor.MoveSpeedV;
            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, Actor.MovementAxis.x * moveSpeed);
            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, Actor.MovementAxis.z * moveSpeed);
            m_FrontDir = Actor.DesiredLookDir;

            // 位移
            if (!m_ApplyRootMotion)
            {
                float speed = GetSpeed2D();
                Actor.CPhysic.AdditivePosition += Actor.DesiredMoveDir * speed * base.DeltaTime;
            }

            // 播放移动动画
            if (m_PlayingMoveStartAnim.IsEmpty() && m_SprintTurnAnim.IsEmpty())
            {
                Actor.CAnim.Play("Move");
            }
        }

        // 冲刺中的急转向
        private void TryPlaySprintTurnAnim()
        {
            if (m_SprintTurnAnim.IsNotEmpty() || m_PlayingMoveStartAnim.IsNotEmpty())
            {
                return;
            }

            float curSpeedV = Actor.CAnim.GetCurSmoothFloat(EAnimatorParams.Vertical);
            if (curSpeedV < 2.5f)
                return;

            float dot = Vector3.Dot(Actor.transform.forward, Actor.DesiredMoveDir);
            if (dot >= 0)
                return;

            m_SprintTurnAnim = "SprintTurn";
            Actor.CAnim.Play(m_SprintTurnAnim, blendTime: 0.2f);
        }

        protected override void OnExit()
        {
            base.OnExit();

            m_PlayingMoveStartAnim = null;
            m_SprintTurnAnim = null;
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