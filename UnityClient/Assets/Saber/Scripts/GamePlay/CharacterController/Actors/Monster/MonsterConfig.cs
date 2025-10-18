using System;
using System.Collections;
using System.Collections.Generic;
using Saber.AI;
using UnityEngine;
using UnityEngine.Serialization;

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
        public EAIType m_DefaultAI;
        public float m_SpeedWalk;
        public float m_SpeedRun;
        public float m_SpeedSprint;

        public bool m_CanDefense;

        public string[] m_SpecialIdls;
        public float m_AttackDesirePercent = 50;
        public float m_DodgeDamagePercent = 50;
        public EAIAttackStyleWhenTooFar m_AIAttackStyleWhenTooFar;

        public float AttackDesireRatio => Mathf.Clamp01(m_AttackDesirePercent / 100f);
        public float DodgeDamageRatio => Mathf.Clamp01(m_DodgeDamagePercent / 100f);
    }

    public enum EAIAttackStyleWhenTooFar
    {
        ToStalemate,
        UseLongestRangeSkill,
        UseRandomSkill,
    }
}