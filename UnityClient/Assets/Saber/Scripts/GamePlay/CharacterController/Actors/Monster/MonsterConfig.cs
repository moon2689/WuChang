using System;
using Saber.AI;
using UnityEngine;

namespace Saber.CharacterController
{
    [CreateAssetMenu(menuName = "Saber/Monster config", fileName = "MonsterConfig", order = 1)]
    public class MonsterConfig : ScriptableObject
    {
        public BaseActorInfo m_BaseActorInfo;
        public MonsterInfo m_MonsterInfo;
    }

    [Serializable]
    public class MonsterInfo
    {
        public bool m_CanDefense;
        public EDodgeType m_DodgeType;

        public string[] m_SpecialIdls;
        public string[] m_SpecialDodgeBackAnims;
        public string[] m_SpecialDodgeLeftAnims;
        public string[] m_SpecialDodgeRightAnims;
        public string[] m_SpecialDodgeFrontAnims;

        [Header("AI")] public AIInfo m_AIInfo;


        public bool CanDodge => m_DodgeType != 0;
    }
}