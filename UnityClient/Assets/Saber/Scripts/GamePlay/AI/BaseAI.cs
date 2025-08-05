using System.Collections;
using System.Collections.Generic;
using Saber.CharacterController;
using UnityEngine;

namespace Saber.AI
{
    public abstract class BaseAI
    {
        public SActor Actor { get; private set; }
        public SActor LockingEnemy { get; protected set; }

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