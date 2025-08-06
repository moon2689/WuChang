using UnityEngine;

//Replace the "ChargeTime" with the event you want to create
//If you want to create a object with handle in preview, please inherit the "AbilityEventObj" with "AbilityEventObj_CreateObjWithHandle"

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / ChargeTime")]
    public class AbilityEventObj_ChargeTime : AbilityEventObj
    {
        public float m_QuickAnimSpeed = 2;


        //Write the data you need here.
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventRange;
        }

        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_ChargeTime(this);
        }
#if UNITY_EDITOR
        public override AbilityEventPreview InitializePreview()
        {
            return null;
        }
#endif
    }

//Write you logic here
    public partial class AbilityEventEffect_ChargeTime : AbilityEventEffect
    {
        private float m_CurrentSpeed;
        private float m_TargetSpeed;
        private CharacterAnimSpeedModifier m_Modifier;

        public override void StartEffect()
        {
            base.StartEffect();
            m_CurrentSpeed = 1;
            m_TargetSpeed = EventObj.m_QuickAnimSpeed;
            m_Modifier = base.Actor.m_AnimSpeedExecutor.AddAnimSpeedModifier(1);
        }

        public override void EffectRunning(float currentTimePercentage)
        {
            base.EffectRunning(currentTimePercentage);
            if (CurrentSkill.IsPowerEnough)
            {
                if (m_TargetSpeed > m_CurrentSpeed)
                {
                    m_CurrentSpeed += Actor.DeltaTime * 10;
                    if (m_CurrentSpeed > m_TargetSpeed)
                    {
                        m_CurrentSpeed = m_TargetSpeed;
                    }
                }
                else if (m_TargetSpeed < m_CurrentSpeed)
                {
                    m_CurrentSpeed -= Actor.DeltaTime * 10;
                    if (m_CurrentSpeed < m_TargetSpeed)
                    {
                        m_CurrentSpeed = m_TargetSpeed;
                    }
                }

                m_Modifier.SpeedScale = m_CurrentSpeed;
            }
        }

        protected override void EndEffect()
        {
            base.EndEffect();
            base.Actor.m_AnimSpeedExecutor.RemoveAnimSpeedModifier(m_Modifier);
        }
    }

    public partial class AbilityEventEffect_ChargeTime : AbilityEventEffect
    {
        private AbilityEventObj_ChargeTime EventObj { get; set; }

        public AbilityEventEffect_ChargeTime(AbilityEventObj initObj) : base(initObj)
        {
            m_EventObj = initObj;
            EventObj = (AbilityEventObj_ChargeTime)initObj;
        }
    }
}