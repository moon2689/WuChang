using Saber.Frame;
using UnityEngine;

//Replace the "WitchTime" with the event you want to create
//If you want to create a object with handle in preview, please inherit the "AbilityEventObj" with "AbilityEventObj_CreateObjWithHandle"

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / WitchTime")]
    public class AbilityEventObj_WitchTime : AbilityEventObj
    {
        //Write the data you need here.
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventRange;
        }

        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_WitchTime(this);
        }
    }

//Write you logic here
    public partial class AbilityEventEffect_WitchTime : AbilityEventEffect
    {
        public override void StartEffect()
        {
            base.StartEffect();
            GameApp.Entry.Game.World.BeginSkillWitchTime();
        }

        protected override void EndEffect()
        {
            base.EndEffect();
            GameApp.Entry.Game.World.EndWitchTime(true);
        }

        public override void Release()
        {
            base.Release();
            GameApp.Entry.Game.World.EndWitchTime(true);
        }
    }

    public partial class AbilityEventEffect_WitchTime : AbilityEventEffect
    {
        private AbilityEventObj_WitchTime EventObj { get; set; }

        public AbilityEventEffect_WitchTime(AbilityEventObj initObj) : base(initObj)
        {
            m_EventObj = initObj;
            EventObj = (AbilityEventObj_WitchTime)initObj;
        }
    }
}