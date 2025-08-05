using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents/ AnimSpeed")]
    public class AbilityEventObj_AnimSpeed : AbilityEventObj
    {
        public float Speed = 1;

        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventRange;
        }

        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_AnimSpeed(this);
        }

        public override AbilityEventPreview InitializePreview()
        {
            return new AbilityEventPreview_AnimSpeed(this);
        }
    }

    public partial class AbilityEventEffect_AnimSpeed : AbilityEventEffect
    {
        CharacterAnimSpeedModifier m_Modifier;

        public override void StartEffect()
        {
            base.StartEffect();
            m_Modifier = Actor.m_AnimSpeedExecutor.AddAnimSpeedModifier(EventObj.Speed);
        }

        protected override void EndEffect()
        {
            base.EndEffect();
            Actor.m_AnimSpeedExecutor.RemoveAnimSpeedModifier(m_Modifier);
        }
    }

    public partial class AbilityEventEffect_AnimSpeed : AbilityEventEffect
    {
        private AbilityEventObj_AnimSpeed EventObj { get; set; }

        public AbilityEventEffect_AnimSpeed(AbilityEventObj Obj) : base(Obj)
        {
            m_EventObj = Obj;
            EventObj = (AbilityEventObj_AnimSpeed)Obj;
        }
    }
}