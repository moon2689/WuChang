using System;
using System.Collections;
using System.Collections.Generic;
using Saber.CharacterController;
using UnityEngine;

namespace Saber.AI
{
    public abstract class BaseAI
    {
        public Action OnSetLockingEnemy;


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
                    OnSetLockingEnemy?.Invoke();
                }
            }
        }


        public virtual void Init(SActor actor)
        {
            Actor = actor;
        }

        public virtual void Update()
        {
        }

        public virtual void Release()
        {
        }

        public virtual void ClearLockEnemy()
        {
            LockingEnemy = null;
        }

        public virtual void OnLockedByEnemy(SActor enemy)
        {
        }
    }
}