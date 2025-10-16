using System;
using System.Collections.Generic;
using CombatEditor;
using Saber.Frame;
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
        public AudioClip[] m_SwordHitBodySound;
        public AudioClip m_SoundAddYuMao;
        public AudioClip m_SoundUseYuMao;
        public AudioClip[] m_SoundSwordHitSword;
        public AudioClip[] m_SoundWeaponHitGround;
        public AudioClip[] m_BoxingHitBodySound;
        public AudioClip[] m_MagicHitBodyCommonSound;

        [Header("特效")] public GameObject m_EffectAddYuMao;
        public GameObject[] m_EffectBlood;
        public GameObject[] m_EffectWeaponHitGround;
        public GameObject[] m_EffectSharpWeaponHitBody;
        public GameObject[] m_EffectBoxingHitBody;
        public GameObject[] m_CommonEffectMagicHitBody;

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
            else if (weaponType == EWeaponType.Sword)
            {
                return GetRandomSound(m_SwordHitBodySound);
            }
            else if (weaponType == EWeaponType.WoodStick)
            {
                return GetRandomSound(m_BoxingHitBodySound);
            }
            else
            {
                Debug.LogError($"Unknown weapon:{weaponType}");
                return null;
            }
        }

        public AudioClip GetRandomBoxingHitBodySound()
        {
            return GetRandomSound(m_BoxingHitBodySound);
        }

        public AudioClip GetRandomMagicHitBodyCommonSound()
        {
            return GetRandomSound(m_MagicHitBodyCommonSound);
        }

        public AudioClip GetRandomSound_SwordHitSword()
        {
            return GetRandomSound(m_SoundSwordHitSword);
        }

        public AudioClip GetRandomSound_WeaponHitGround()
        {
            return GetRandomSound(m_SoundWeaponHitGround);
        }

        private GameObject GetRandomEffectPrefab(GameObject[] effects)
        {
            if (effects != null && effects.Length > 0)
            {
                int ranIndex = UnityEngine.Random.Range(0, effects.Length);
                return effects[ranIndex];
            }

            return null;
        }

        public GameObject GetRandomEffectPrefab_Blood()
        {
            return GetRandomEffectPrefab(m_EffectBlood);
        }

        public GameObject GetRandomEffectPrefab_WeaponHitGround()
        {
            return GetRandomEffectPrefab(m_EffectWeaponHitGround);
        }

        public GameObject GetRandomEffectPrefab_SharpWeaponHitBody()
        {
            return GetRandomEffectPrefab(m_EffectSharpWeaponHitBody);
        }

        public GameObject GetRandomEffectPrefab_BoxingHitBody()
        {
            return GetRandomEffectPrefab(m_EffectBoxingHitBody);
        }

        public GameObject GetRandomCommonEffectPrefab_MagicHitBody()
        {
            return GetRandomEffectPrefab(m_CommonEffectMagicHitBody);
        }

        public void PreloadEffects()
        {
            GameApp.Entry.Game.Effect.PreloadEffect(m_EffectAddYuMao);

            foreach (var e in m_EffectBlood)
            {
                GameApp.Entry.Game.Effect.PreloadEffect(e);
            }

            foreach (var e in m_EffectWeaponHitGround)
            {
                GameApp.Entry.Game.Effect.PreloadEffect(e);
            }

            foreach (var e in m_EffectSharpWeaponHitBody)
            {
                GameApp.Entry.Game.Effect.PreloadEffect(e);
            }

            foreach (var e in m_EffectBoxingHitBody)
            {
                GameApp.Entry.Game.Effect.PreloadEffect(e);
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
        public float m_BloodTime = 1;

        public bool IsDmgDone { get; set; }
    }
}