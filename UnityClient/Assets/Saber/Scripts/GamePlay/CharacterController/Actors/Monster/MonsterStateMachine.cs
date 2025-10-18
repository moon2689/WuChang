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
            base.RegisterState(new Idle());
            base.RegisterState(new SkillState());
            base.RegisterState(new GetHit());
            base.RegisterState(new Die());
            base.RegisterState(new PlayActionState());
            base.RegisterState(new MonsterMove());

            if (Monster.m_BaseActorInfo.m_AIInfo.CanDodge)
            {
                base.RegisterState(new Dodge());
            }

            if (Monster.m_MonsterInfo.m_CanDefense)
            {
                base.RegisterState(new MonsterDefense());
            }
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