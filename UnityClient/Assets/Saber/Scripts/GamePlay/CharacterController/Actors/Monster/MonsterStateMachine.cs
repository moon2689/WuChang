using System;
using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterStateMachine : ActorStateMachine
    {
        private SMonster m_Monster;

        SMonster Monster => m_Monster ??= (SMonster)Actor;


        public MonsterStateMachine(SActor actor) : base(actor)
        {
        }

        protected override void RegisterStates()
        {
            base.RegisterState(new MonsterIdle());
            base.RegisterState(new SkillState());
            base.RegisterState(new MonsterGetHit());
            base.RegisterState(new MonsterDie());
            base.RegisterState(new Weak());

            if (Monster.m_MonsterInfo.m_CanJump)
            {
                base.RegisterState(new MonsterJump());
            }

            if (Monster.m_MonsterInfo.m_MoveByRootMotion)
            {
                base.RegisterState(new MonsterRootMotionMove());
            }
            else
            {
                base.RegisterState(new MonsterMove());
            }

            if (Monster.m_MonsterInfo.m_CanDodge)
            {
                base.RegisterState(new MonsterDodge());
            }

            if (Monster.m_MonsterInfo.m_CanDefense)
            {
                base.RegisterState(new MonsterDefense());
            }
        }

        public override bool Startled(Action onFinished)
        {
            if (Actor.CurrentStateType == EStateType.Idle)
            {
                MonsterIdle idle = Actor.CStateMachine.GetState<MonsterIdle>(EStateType.Idle);
                idle.DoActionStartled(onFinished);
                return true;
            }

            return false;
        }

        public override bool PlayAction_LookAround(Action onFinished)
        {
            if (Actor.CurrentStateType == EStateType.Idle)
            {
                MonsterIdle idle = Actor.CStateMachine.GetState<MonsterIdle>(EStateType.Idle);
                idle.LookAround(onFinished);
                return true;
            }

            onFinished?.Invoke();
            return false;
        }

        public override bool PlayActionWhenIdle(string name, Action onFinished)
        {
            if (Actor.CurrentStateType == EStateType.Idle)
            {
                MonsterIdle idle = Actor.CStateMachine.GetState<MonsterIdle>(EStateType.Idle);
                idle.PlayAction(name, onFinished);
                return true;
            }

            onFinished?.Invoke();
            return false;
        }

        public override bool IsPlayingActionWhenIdle(string name)
        {
            if (Actor.CurrentStateType != EStateType.Idle)
            {
                return false;
            }
            
            MonsterIdle idle = Actor.CStateMachine.GetState<MonsterIdle>(EStateType.Idle);
            return idle.IsPlayingAction(name);
        }

        public override bool PlayAction_TurnDirection(Vector3 targetPos, Action onFinished)
        {
            if (Actor.CurrentStateType == EStateType.Idle)
            {
                MonsterIdle idle = Actor.CStateMachine.GetState<MonsterIdle>(EStateType.Idle);
                idle.TurnDirection(targetPos, onFinished);
                return true;
            }

            onFinished?.Invoke();
            return false;
        }

        public override bool OnHit(DamageInfo dmgInfo)
        {
            bool playHitRect = CurrentStateType == EStateType.GetHit ||
                               Actor.StatsInfo.m_MaxSuperArmorValue <= 0 ||
                               Actor.CStats.CurrentSuperArmor <= 0;

            if (playHitRect)
            {
                return TryEnterState<MonsterGetHit>(EStateType.GetHit, state => state.Damage = dmgInfo);
            }
            else
            {
                return false;
            }
        }

        public override void OnParried()
        {
            TryEnterState<MonsterObstruct>(EStateType.GetHit, state =>
            {
                state.Damage = new DamageInfo()
                {
                    ObstructType = EObstructType.Parried,
                };
            });
        }

        public override bool Dodge(Vector3 axis)
        {
            if (!Monster.m_MonsterInfo.m_CanDodge)
                return false;
            return TryEnterState<MonsterDodge>(EStateType.Dodge, state => state.DodgeAxis = axis);
        }

        public override bool DefenseStart()
        {
            if (Monster.m_MonsterInfo.m_CanDefense)
                return TryEnterState(EStateType.Defense);
            else
                return false;
        }

        public override bool DefenseEnd()
        {
            if (CurrentStateType == EStateType.Defense)
            {
                if (CurrentState.CanExit)
                {
                    ((MonsterDefense)CurrentState).EndDefense();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public override void DefenseHit(DamageInfo dmgInfo)
        {
            if (CurrentStateType == EStateType.Defense)
            {
                ((MonsterDefense)CurrentState).OnHit(dmgInfo);
            }
        }

        public override bool Jump(Vector3 axis)
        {
            return TryEnterState<MonsterJump>(EStateType.Jump, state => state.JumpAxis = axis);
        }
    }
}