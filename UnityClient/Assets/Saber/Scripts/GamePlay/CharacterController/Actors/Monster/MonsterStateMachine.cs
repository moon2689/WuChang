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
            base.RegisterState(new GetHit());
            base.RegisterState(new Die());
            base.RegisterState(new PlayActionState());

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
    }
}