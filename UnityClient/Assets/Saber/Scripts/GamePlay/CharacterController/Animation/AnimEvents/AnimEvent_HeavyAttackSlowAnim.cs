namespace Saber.CharacterController
{
    public class AnimEvent_HeavyAttackSlowAnim : AnimRangeTimeEvent
    {
        public float m_SlowAnimSpeed = 0.1f;
        public override EAnimRangeEvent EventType => EAnimRangeEvent.HeavyAttackSlowAnim;
    }
}