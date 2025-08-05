using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Saber.CharacterController
{
    public class AnimEvent_PlaySound : AnimPointTimeEvent
    {
        public List<AudioClip> m_Clips;
        public float m_Volume = 1;

        public override EAnimTriggerEvent EventType => EAnimTriggerEvent.PlaySound;

        protected override void OnTrigger(Animator anim, AnimatorStateInfo state)
        {
            var clip = m_Clips[UnityEngine.Random.Range(0, m_Clips.Count)];
            GameApp.Entry.Game.Audio.Play3DSound(clip, anim.transform.position);
        }
    }
}