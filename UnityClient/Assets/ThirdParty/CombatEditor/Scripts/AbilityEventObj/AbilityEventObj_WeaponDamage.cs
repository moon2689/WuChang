using UnityEngine;
using Saber.CharacterController;

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / WeaponDamage")]
    public class AbilityEventObj_WeaponDamage : AbilityEventObj
    {
        public WeaponDamageSetting m_WeaponDamageSetting;

        //Write the data you need here.
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventRange;
        }

        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_WeaponDamage(this);
        }
    }

    public partial class AbilityEventEffect_WeaponDamage : AbilityEventEffect
    {
        public override void StartEffect()
        {
            base.StartEffect();
            base.Actor.CMelee.ToggleDamage(EventObj.m_WeaponDamageSetting, true);
        }

        protected override void EndEffect()
        {
            base.EndEffect();
            base.Actor.CMelee.ToggleDamage(EventObj.m_WeaponDamageSetting, false);
        }
    }

    public partial class AbilityEventEffect_WeaponDamage : AbilityEventEffect
    {
        private AbilityEventObj_WeaponDamage EventObj { get; set; }

        public AbilityEventEffect_WeaponDamage(AbilityEventObj initObj) : base(initObj)
        {
            m_EventObj = initObj;
            EventObj = (AbilityEventObj_WeaponDamage)initObj;
        }
    }
}