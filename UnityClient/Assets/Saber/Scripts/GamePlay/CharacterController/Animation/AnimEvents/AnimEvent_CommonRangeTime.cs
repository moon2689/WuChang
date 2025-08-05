namespace Saber.CharacterController
{
    public class AnimEvent_CommonRangeTime : AnimRangeTimeEvent
    {
        public EAnimRangeEvent m_EventType;
        public override EAnimRangeEvent EventType => m_EventType;
    }
}