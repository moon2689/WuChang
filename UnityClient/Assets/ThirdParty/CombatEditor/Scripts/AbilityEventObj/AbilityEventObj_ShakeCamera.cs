using Saber.Frame;

using UnityEngine;

//Replace the "ShakeCamera" with the event you want to create
//If you want to create a object with handle in preview, please inherit the "AbilityEventObj" with "AbilityEventObj_CreateObjWithHandle"

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / ShakeCamera")]
    public class AbilityEventObj_ShakeCamera : AbilityEventObj
    {
        public float m_Duration;
        public float m_Amount;
        public float m_Speed;

        //Write the data you need here.
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventTime;
        }

        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_ShakeCamera(this);
        }
#if UNITY_EDITOR
        public override AbilityEventPreview InitializePreview()
        {
            return new AbilityEventPreview_ShakeCamera(this);
        }
#endif
    }

//Write you logic here
    public partial class AbilityEventEffect_ShakeCamera : AbilityEventEffect
    {
        public override void StartEffect()
        {
            base.StartEffect();
            if (Actor.IsPlayer || Vector3.Distance(Actor.transform.position, GameApp.Entry.Game.Player.transform.position) < 8)
            {
                GameApp.Entry.Game.PlayerCamera.ShakeCamera(EventObj.m_Duration, EventObj.m_Amount, EventObj.m_Speed);
            }
        }
    }

    public partial class AbilityEventEffect_ShakeCamera : AbilityEventEffect
    {
        private AbilityEventObj_ShakeCamera EventObj { get; set; }

        public AbilityEventEffect_ShakeCamera(AbilityEventObj initObj) : base(initObj)
        {
            m_EventObj = initObj;
            EventObj = (AbilityEventObj_ShakeCamera)initObj;
        }
    }
}