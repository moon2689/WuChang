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
        public float PlayerRebirthDelaySeconds = 8;
        public float DodgeCostStamina = 20;
        public float IKBoneForceOnHit = 1;
        public float RecoverUnbalanceValueDelaySeconds = 4;
        public float RecoverUnbalanceSpeed = 10;
        public int MaxHPPotionCount = 5;
        public float HPPotionRecoverRate = 0.5f;
        public AudioClip[] m_VoiceTired;
        public AudioClip[] m_SoundFootStepWater;
        public AudioClip[] m_SoundFootStepGround;
        public AudioClip[] m_SoundFootStepSnow;
        public PlayerPropItemInfo[] m_IdolRestRecItems;
        

        public AudioClip GetRandomVoiceTired()
        {
            int ranIndex = UnityEngine.Random.Range(0, m_VoiceTired.Length);
            return m_VoiceTired[ranIndex];
        }
    }
}