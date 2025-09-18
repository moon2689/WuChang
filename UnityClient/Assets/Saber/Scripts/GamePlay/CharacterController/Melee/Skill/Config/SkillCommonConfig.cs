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
        public AudioClip ExecuteStartSound;
        public ExecuteDamage[] ExecuteDamages;
        public float ExecuteSkillCanExitTime = 0.8f;
        public EImpactForce BackStabPower = EImpactForce.Level3;

        [Header("弹反")] public float CanTanFanSecondsFromDefenseStart = 2f;
        public AudioClip BlockBrokenSound;

        [Header("音效")] public AudioClip[] m_MiaoDaoHitBodySound;
        public AudioClip[] m_YueYaChanHitBodySound;
        public AudioClip m_SoundAddYuMao;
        public AudioClip m_SoundUseYuMao;
        
        [Header("特效")]
        public GameObject m_EffectAddYuMao;

        private AudioClip GetRandomSound(AudioClip[] clips)
        {
            if (clips != null && clips.Length > 0)
            {
                int ranIndex = UnityEngine.Random.Range(0, clips.Length);
                return clips[ranIndex];
            }

            return null;
        }

        public AudioClip GetRandomHitBodySound(EWeaponType weaponType)
        {
            if (weaponType == EWeaponType.MiaoDao)
            {
                return GetRandomSound(m_MiaoDaoHitBodySound);
            }
            else if (weaponType == EWeaponType.YueYaChan)
            {
                return GetRandomSound(m_YueYaChanHitBodySound);
            }
            else
            {
                Debug.LogError($"Unknown weapon:{weaponType}");
                return null;
            }
        }
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