using UnityEngine;

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / AttackStates")]
    public class AbilityEventObj_AttackStates : AbilityEventObj_States
    {
        private string[] m_States = new string[3]
        {
            EAttackStates.BeforeAttack.ToString(),
            EAttackStates.Attacking.ToString(),
            EAttackStates.AfterAttack.ToString(),
        };

        public override int DivideCount => 3;
        public override string[] States => m_States;

        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_AttackStates(this);
        }
    }

    public enum EAttackStates
    {
        /// <summary>攻击前摇</summary>
        BeforeAttack,

        /// <summary>攻击中</summary>
        Attacking,

        /// <summary>攻击后摇</summary>
        AfterAttack,
    }

//Write you logic here
    public partial class AbilityEventEffect_AttackStates : AbilityEventEffect
    {
        public override void EffectRunning(float currentTimePercentage)
        {
            base.EffectRunning(currentTimePercentage);
            if (CurrentSkill.CurrentAttackState == EAttackStates.BeforeAttack)
            {
                if (currentTimePercentage > AbilityEvent.EventMultiRange[0])
                {
                    CurrentSkill.CurrentAttackState = EAttackStates.Attacking;
                }
            }
            else if (CurrentSkill.CurrentAttackState == EAttackStates.Attacking)
            {
                if (currentTimePercentage > AbilityEvent.EventMultiRange[1])
                {
                    CurrentSkill.CurrentAttackState = EAttackStates.AfterAttack;
                }
            }
        }
    }

    public partial class AbilityEventEffect_AttackStates : AbilityEventEffect
    {
        private AbilityEventObj_AttackStates EventObj { get; set; }

        public AbilityEventEffect_AttackStates(AbilityEventObj initObj) : base(initObj)
        {
            m_EventObj = initObj;
            EventObj = (AbilityEventObj_AttackStates)initObj;
        }
    }
}