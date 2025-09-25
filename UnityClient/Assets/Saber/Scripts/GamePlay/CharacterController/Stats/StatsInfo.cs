using System;
using UnityEngine.Serialization;

namespace Saber.CharacterController
{
    [Serializable]
    public class StatsInfo
    {
        public int m_MaxHp = 100;
        public float m_DefaultHpRecSpeed;
        public int m_MaxStamina = 100;
        public int m_MaxPower = 3;
        public EResilience m_Resilience = EResilience.Level1;
        public int m_UnbalanceValue = 100;
    }
}