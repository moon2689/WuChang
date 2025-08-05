using UnityEngine;

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / CancelGravity")]
    public class AbilityEventObj_CancelGravity : AbilityEventObj
    {
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventRange;
        }

        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_CancelGravity(this);
        }
    }

    public partial class AbilityEventEffect_CancelGravity : AbilityEventEffect
    {
        public override void StartEffect()
        {
            base.StartEffect();
            Actor.CPhysic.UseGravity = false;
            Actor.CPhysic.EnablePlatformMovement = false;
            Actor.CPhysic.SetPlatform(null);
        }

        protected override void EndEffect()
        {
            base.EndEffect();
            Actor.CPhysic.UseGravity = true;
            Actor.CPhysic.EnablePlatformMovement = true;
        }
    }

    public partial class AbilityEventEffect_CancelGravity : AbilityEventEffect
    {
        private AbilityEventObj_CancelGravity EventObj { get; set; }

        public AbilityEventEffect_CancelGravity(AbilityEventObj InitObj) : base(InitObj)
        {
            m_EventObj = InitObj;
            EventObj = (AbilityEventObj_CancelGravity)InitObj;
        }
    }
}