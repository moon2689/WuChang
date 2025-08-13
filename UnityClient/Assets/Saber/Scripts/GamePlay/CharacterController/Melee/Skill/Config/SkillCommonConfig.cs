using System;
using System.Collections.Generic;
using CombatEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.CharacterController
{
    [CreateAssetMenu(menuName = "Saber/Skill common config", fileName = "SkillCommon", order = 1)]
    public class SkillCommonConfig : ScriptableObject
    {
        [Header("处决")] public float ExecuteMaxDistance = 3;
        public float ExecuteMaxAngle = 80;
        public AudioClip ExecuteStartSound;
        public ExecuteDamage[] ExecuteDamages;
        public float ExecuteSkillCanExitTime = 0.8f;
    }

    [Serializable]
    public class ExecuteDamage
    {
        public float m_DamageTime;
        public float m_Damage;
        public AudioClip m_Sound;
        public GameObject m_Blood;

        public bool IsDmgDone { get; set; }
    }
    
}