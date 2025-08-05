
using UnityEngine;

//Replace the "CanParry" with the event you want to create
//If you want to create a object with handle in preview, please inherit the "AbilityEventObj" with "AbilityEventObj_CreateObjWithHandle"

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / CanParry")]
    public class AbilityEventObj_CanParry : AbilityEventObj
    {
        //Write the data you need here.
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventRange;
        }

        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_CanParry(this);
        }
    }

//Write you logic here
    public partial class AbilityEventEffect_CanParry : AbilityEventEffect
    {
        public override void StartEffect()
        {
            base.StartEffect();

            /*
            ParryEvent parryEvent = EventObj.m_ParryEvent;

            if (parryEvent.m_CanParry)
            {
                CurSkill.InCanParryTime = true;
                CurSkill.m_ParryEvent = parryEvent;
            }

            if (parryEvent.m_CanPerfectDodge)
            {
                CurSkill.InPerfectDodgeTime = true;
                CurSkill.m_ParryEvent = parryEvent;
            }*/
        }

        protected override void EndEffect()
        {
            base.EndEffect();

            /*
            CurSkill.InCanParryTime = false;
            CurSkill.InPerfectDodgeTime = false;
            CurSkill.m_ParryEvent = null;*/
        }
    }

    public partial class AbilityEventEffect_CanParry : AbilityEventEffect
    {
        private AbilityEventObj_CanParry EventObj { get; set; }

        public AbilityEventEffect_CanParry(AbilityEventObj initObj) : base(initObj)
        {
            m_EventObj = initObj;
            EventObj = (AbilityEventObj_CanParry)initObj;
        }
    }
}