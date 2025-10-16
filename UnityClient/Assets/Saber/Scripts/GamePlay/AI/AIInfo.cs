using System;

namespace Saber.AI
{
    [Serializable]
    public class AIInfo
    {
        public float m_WarningRange = 10;
        public float m_LostFocusRange = 10;
        public EDodgeType m_DodgeType;

        public bool CanDodge => m_DodgeType != EDodgeType.CannotDodge;
    }
    
    public enum EDodgeType
    {
        CannotDodge,
        CanDodge,
        OnlyCanJumpBack,
    }
}