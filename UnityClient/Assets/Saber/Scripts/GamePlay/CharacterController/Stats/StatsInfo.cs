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

        public int m_MaxSuperArmorValue;

        //public int m_MaxBlockValue = 100;
        public int m_MaxPower = 30;
        public int m_ParriedMaxTimes = 3;
    }
}