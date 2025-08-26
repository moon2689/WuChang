using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Saber.CharacterController
{
    public class AnimEvent_ShakeCamera : AnimPointTimeEvent
    {
        public float m_Duration = 0.1f;
        public float m_Amount = 0.3f;
        public float m_Speed = 3f;

        public override EAnimTriggerEvent EventType => EAnimTriggerEvent.PlaySound;

        protected override void OnTrigger(Animator anim, AnimatorStateInfo state)
        {
            GameApp.Entry.Game.PlayerCamera.ShakeCamera(m_Duration, m_Amount, m_Speed);
        }
    }
}