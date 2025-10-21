using System;

namespace Saber.AI
{
    [Serializable]
    public class AIInfo
    {
        public float m_WarningRange = 10;
        public float m_LostFocusRange = 10;
        public EDodgeType m_DodgeType;

        public bool CanDodge => m_DodgeType != 0;
    }

    [Flags]
    public enum EDodgeType
    {
        Back = 1,
        Left = 2,
        Right = 4,
        Front = 8,
    }
}