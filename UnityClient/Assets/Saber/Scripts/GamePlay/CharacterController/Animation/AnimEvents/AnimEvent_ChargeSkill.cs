using UnityEngine.Serialization;

namespace Saber.CharacterController
{
    public class AnimEvent_ChargeSkill : AnimRangeTimeEvent
    {
        public float m_QuickAnimTime = 2f;
        public override EAnimRangeEvent EventType => EAnimRangeEvent.InChargeTime;
    }
}