using UnityEngine;
//Replace the "OwnerForce" with the event you want to create
//If you want to create a object with handle in preview, please inherit the "AbilityEventObj" with "AbilityEventObj_CreateObjWithHandle"

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / OwnerForce")]
    public class AbilityEventObj_OwnerForce: AbilityEventObj
    {
        public Vector2 m_Force;
        
        //Write the data you need here.
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventTime;
        }
        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_OwnerForce(this);
        }
#if UNITY_EDITOR
        public override AbilityEventPreview InitializePreview()
        {
            //return new AbilityEventPreview_OwnerForce(this);
            return null;
        }
#endif
    }

//Write you logic here
    public partial class AbilityEventEffect_OwnerForce : AbilityEventEffect
    {
        public override void StartEffect()
        {
            base.StartEffect();
            CurrentSkill.AddOwnerForce(EventObj.m_Force);
        }
    }

    public partial class AbilityEventEffect_OwnerForce : AbilityEventEffect
    {
        private AbilityEventObj_OwnerForce EventObj {get;set;}
        public AbilityEventEffect_OwnerForce(AbilityEventObj initObj) : base(initObj)
        {
            m_EventObj = initObj;
            EventObj  = (AbilityEventObj_OwnerForce)initObj;
        }
    }
}