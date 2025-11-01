using UnityEngine;

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / Invincible")]
    public class AbilityEventObj_Invincible : AbilityEventObj
    {
        public bool m_AddYuMaoWhenHittedAtStart;


        //Write the data you need here.
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventRange;
        }

        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_Invincible(this);
        }
    }

//Write you logic here
    public partial class AbilityEventEffect_Invincible : AbilityEventEffect
    {
        private float m_TimerCanAddYuMao;

        public override void StartEffect()
        {
            base.StartEffect();

            /*
            ParryEvent parryEvent = EventObj.m_ParryEvent;

            if (parryEvent.m_Invincible)
            {
                CurSkill.InInvincibleTime = true;
                CurSkill.m_ParryEvent = parryEvent;
            }

            if (parryEvent.m_CanPerfectDodge)
            {
                CurSkill.InPerfectDodgeTime = true;
                CurSkill.m_ParryEvent = parryEvent;
            }*/
            Actor.Invincible = true;

            if (EventObj.m_AddYuMaoWhenHittedAtStart)
            {
                Actor.AddYuMaoWhenHitted = true;
                m_TimerCanAddYuMao = 0.5f;
            }
        }

        public override void EffectRunning(float currentTimePercentage)
        {
            base.EffectRunning(currentTimePercentage);

            if (EventObj.m_AddYuMaoWhenHittedAtStart && m_TimerCanAddYuMao > 0)
            {
                m_TimerCanAddYuMao -= Time.deltaTime;
                if (m_TimerCanAddYuMao <= 0)
                {
                    Actor.AddYuMaoWhenHitted = false;
                }
            }
        }

        protected override void EndEffect()
        {
            base.EndEffect();

            /*
            CurSkill.InInvincibleTime = false;
            CurSkill.InPerfectDodgeTime = false;
            CurSkill.m_ParryEvent = null;*/
            Actor.Invincible = false;
            if (EventObj.m_AddYuMaoWhenHittedAtStart)
                Actor.AddYuMaoWhenHitted = false;
        }
    }

    public partial class AbilityEventEffect_Invincible : AbilityEventEffect
    {
        private AbilityEventObj_Invincible EventObj { get; set; }

        public AbilityEventEffect_Invincible(AbilityEventObj initObj) : base(initObj)
        {
            m_EventObj = initObj;
            EventObj = (AbilityEventObj_Invincible)initObj;
        }
    }
}