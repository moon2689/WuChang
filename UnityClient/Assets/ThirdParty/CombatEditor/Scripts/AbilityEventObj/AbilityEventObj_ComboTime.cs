using UnityEngine;

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / ComboTime")]
    public class AbilityEventObj_ComboTime: AbilityEventObj
    {
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventRange;
        }
        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_ComboTime(this);
        }
    }

    public partial class AbilityEventEffect_ComboTime : AbilityEventEffect
    {
        public override void StartEffect()
        {
            base.StartEffect();
            CurrentSkill.InComboTime = true;
        }

        protected override void EndEffect()
        {
            base.EndEffect();
            CurrentSkill.InComboTime = false;
            CurrentSkill.ComboTimePassed = true;
        }
    }

    public partial class AbilityEventEffect_ComboTime : AbilityEventEffect
    {
        private AbilityEventObj_ComboTime EventObj { get; set; }

        public AbilityEventEffect_ComboTime(AbilityEventObj InitObj) : base(InitObj)
        {
            m_EventObj = InitObj;
            EventObj  = (AbilityEventObj_ComboTime)InitObj;
        }
    }
}