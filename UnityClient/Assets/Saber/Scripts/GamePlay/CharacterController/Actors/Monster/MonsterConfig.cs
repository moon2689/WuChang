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
        public bool m_MoveByRootMotion;
        public float m_SpeedWalk;
        public float m_SpeedRun;
        public float m_SpeedSprint;
        public List<AudioClip> m_FootstepClips;

        public bool m_PlaySoundWhenIdle;
        public List<AudioClip> m_IdleSoundClips;

        public bool m_CanDodge;
        public bool m_CanDefense;

        public AudioClip GetRandomFootstepAudio()
        {
            return m_FootstepClips[UnityEngine.Random.Range(0, m_FootstepClips.Count)];
        }

        public AudioClip GetRandomIdleAudio()
        {
            return m_IdleSoundClips[UnityEngine.Random.Range(0, m_IdleSoundClips.Count)];
        }
    }
}
