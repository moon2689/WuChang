using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MagicaCloth2;
using Saber.Config;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public abstract class BuffBase
    {
        protected SActor m_Actor;
        protected float m_Value;
        private float m_HoldSeconds;
        private float m_Timer;

        public EBuffType BuffType { get; private set; }
        public bool IsRunning { get; private set; }


        protected abstract void OnStart();
        protected abstract void OnUpdate(float deltaTime);
        protected abstract void OnEnd();


        public BuffBase(SActor actor, EBuffType buffType)
        {
            m_Actor = actor;
            BuffType = buffType;
        }

        public void Update(float deltaTime)
        {
            if (IsRunning)
            {
                OnUpdate(deltaTime);

                m_Timer += deltaTime;
                if (m_Timer >= m_HoldSeconds)
                {
                    IsRunning = false;
                    OnEnd();
                }
            }
        }

        public void Start(float value, float holdSeconds)
        {
            m_Value = value;
            m_HoldSeconds = holdSeconds;
            IsRunning = true;
            m_Timer = 0;
            OnStart();
        }
    }
}