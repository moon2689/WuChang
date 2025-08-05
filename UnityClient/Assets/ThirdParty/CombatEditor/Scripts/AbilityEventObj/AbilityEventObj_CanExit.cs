using UnityEngine;

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / CanExit")]
    public class AbilityEventObj_CanExit: AbilityEventObj
    {
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventTime;
        }
        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_CanExit(this);
        }
    }

    public partial class AbilityEventEffect_CanExit : AbilityEventEffect
    {
        public override void StartEffect()
        {
            base.StartEffect();
            CurrentSkill.CanExit = true;
        }
    }

    public partial class AbilityEventEffect_CanExit : AbilityEventEffect
    {
        private AbilityEventObj_CanExit EventObj { get; set; }

        public AbilityEventEffect_CanExit(AbilityEventObj InitObj) : base(InitObj)
        {
            m_EventObj = InitObj;
            EventObj  = (AbilityEventObj_CanExit)InitObj;
        }
    }
}