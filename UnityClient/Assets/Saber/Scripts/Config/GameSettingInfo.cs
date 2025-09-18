using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.Config
{
    [CreateAssetMenu(menuName = "Saber/Game Setting Info", fileName = "GameSetting", order = 1)]
    public class GameSettingInfo : ScriptableObject
    {
        public bool OpenCameraLight;
        public float ShadowDistance = 50;

        public int PlayerID = 1;
        public int[] PlayerStartClothes;
        public int StartSceneID = 1;

        [Header("Player")] public float DodgeCostStamina = 20;
        public float IKBoneForceOnHit = 1;
        public float RecoverUnbalanceValueDelaySeconds = 4;
        public float RecoverUnbalanceSpeed = 10;
        public int PerfectDodgeShaEffCount = 6;
        public float PerfectDodgeShaInterval = 0.1f;
        public float PerfectDodgeShaHoldTime = 0.3f;
        public int MaxHPPotionCount = 5;
        public float HPPotionRecoverRate = 0.5f;
        public AudioClip[] m_VoiceTired;

        [Header("Melee")] public AudioClip[] m_SoundSwordHitSword;
        public AudioClip[] m_SoundWeaponHitGround;
        public AudioClip[] m_SoundSharpWeaponHitBody;
        public AudioClip[] m_SoundFistHitBody;
        public AudioClip[] m_SoundClawHitBody;

        public GameObject[] m_EffectBlood;
        public GameObject[] m_EffectWeaponHitGround;
        public GameObject[] m_EffectSharpWeaponHitBody;
        public GameObject[] m_EffectFistHitBody;
        public GameObject[] m_EffectClawHitBody;

        public AudioClip GetRandomVoiceTired()
        {
            int ranIndex = UnityEngine.Random.Range(0, m_VoiceTired.Length);
            return m_VoiceTired[ranIndex];
        }
        
        public AudioClip GetRandomSound_SwordHitSword()
        {
            int ranIndex = UnityEngine.Random.Range(0, m_SoundSwordHitSword.Length);
            return m_SoundSwordHitSword[ranIndex];
        }

        public AudioClip GetRandomSound_WeaponHitGround()
        {
            int ranIndex = UnityEngine.Random.Range(0, m_SoundWeaponHitGround.Length);
            return m_SoundWeaponHitGround[ranIndex];
        }

        public AudioClip GetRandomSound_SharpWeaponHitBody()
        {
            int ranIndex = UnityEngine.Random.Range(0, m_SoundSharpWeaponHitBody.Length);
            return m_SoundSharpWeaponHitBody[ranIndex];
        }

        public AudioClip GetRandomSound_FistHitBody()
        {
            int ranIndex = UnityEngine.Random.Range(0, m_SoundFistHitBody.Length);
            return m_SoundFistHitBody[ranIndex];
        }

        public AudioClip GetRandomSound_ClawHitBody()
        {
            int ranIndex = UnityEngine.Random.Range(0, m_SoundClawHitBody.Length);
            return m_SoundClawHitBody[ranIndex];
        }

        public GameObject GetRandomEffectPrefab_Blood()
        {
            int ranIndex = UnityEngine.Random.Range(0, m_EffectBlood.Length);
            return m_EffectBlood[ranIndex];
        }

        public GameObject GetRandomEffectPrefab_WeaponHitGround()
        {
            int ranIndex = UnityEngine.Random.Range(0, m_EffectWeaponHitGround.Length);
            return m_EffectWeaponHitGround[ranIndex];
        }

        public GameObject GetRandomEffectPrefab_SharpWeaponHitBody()
        {
            int ranIndex = UnityEngine.Random.Range(0, m_EffectSharpWeaponHitBody.Length);
            return m_EffectSharpWeaponHitBody[ranIndex];
        }

        public GameObject GetRandomEffectPrefab_FistHitBody()
        {
            int ranIndex = UnityEngine.Random.Range(0, m_EffectFistHitBody.Length);
            return m_EffectFistHitBody[ranIndex];
        }

        public GameObject GetRandomEffectPrefab_ClawHitBody()
        {
            int ranIndex = UnityEngine.Random.Range(0, m_EffectClawHitBody.Length);
            return m_EffectClawHitBody[ranIndex];
        }

        public void PreloadEffects()
        {
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

            foreach (var e in m_EffectFistHitBody)
            {
                GameApp.Entry.Game.Effect.PreloadEffect(e);
            }

            foreach (var e in m_EffectClawHitBody)
            {
                GameApp.Entry.Game.Effect.PreloadEffect(e);
            }
        }
    }
}