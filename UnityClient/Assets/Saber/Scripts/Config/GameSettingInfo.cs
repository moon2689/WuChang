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

        [Header("Game")] public float PlayerRebirthDelaySeconds = 8;

        [Header("Player")] public float DodgeCostStamina = 20;
        public float IKBoneForceOnHit = 1;
        public float RecoverUnbalanceValueDelaySeconds = 4;
        public float RecoverUnbalanceSpeed = 10;
        public int MaxHPPotionCount = 5;
        public float HPPotionRecoverRate = 0.5f;
        public AudioClip[] m_VoiceTired;

        [Header("Melee")] public AudioClip[] m_SoundSwordHitSword;
        public AudioClip[] m_SoundWeaponHitGround;

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