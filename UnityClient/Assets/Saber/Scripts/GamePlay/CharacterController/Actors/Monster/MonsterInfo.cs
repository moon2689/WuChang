using System;
using System.Collections;
using System.Collections.Generic;
using Saber.AI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.CharacterController
{
    [Serializable]
    public class MonsterInfo
    {
        public enum EFootstepType
        {
            ColliderChest,
            PlayWhenMove,
        }

        public enum EHitRecType
        {
            _2Side,
            _4Side,
        }

        public EAIType m_DefaultAI;
        public bool m_MoveByRootMotion;
        public float m_SpeedWalk;
        public float m_SpeedRun;
        public EFootstepType m_FootstepType;
        public List<AudioClip> m_FootstepClips;

        public bool m_PlaySoundWhenIdle;
        public List<AudioClip> m_IdleSoundClips;

        public bool m_CanDodge;
        public bool m_CanDefense;

        public EHitRecType m_HitRecType;

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