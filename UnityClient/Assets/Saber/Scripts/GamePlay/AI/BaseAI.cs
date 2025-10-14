using System;
using System.Collections;
using System.Collections.Generic;
using Saber.CharacterController;
using Saber.Config;
using UnityEngine;

namespace Saber.AI
{
    public abstract class BaseAI
    {
        public Action<SActor, SActor> OnSetLockingEnemy;

        private SActor m_LockingEnemy;

        public SActor Actor { get; private set; }

        public SActor LockingEnemy
        {
            get => m_LockingEnemy;
            protected set
            {
                if (m_LockingEnemy != value)
                {
                    m_LockingEnemy = value;
                    OnSetLockingEnemy?.Invoke(Actor, value);
                }
            }
        }


        protected abstract void OnDead(SActor owner);


        public virtual void Init(SActor actor)
        {
            Actor = actor;
            Actor.Event_OnDead -= OnDead;
            Actor.Event_OnDead += OnDead;
        }

        public virtual void Update()
        {
        }

        public virtual void Release()
        {
            Actor.Event_OnDead -= OnDead;
        }
    }
}