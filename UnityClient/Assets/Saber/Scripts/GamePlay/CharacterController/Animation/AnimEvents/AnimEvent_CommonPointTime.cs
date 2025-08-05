namespace Saber.CharacterController
{
    public class AnimEvent_CommonPointTime : AnimPointTimeEvent
    {
        public EAnimTriggerEvent m_EventType;
        public override EAnimTriggerEvent EventType => m_EventType;
    }
}