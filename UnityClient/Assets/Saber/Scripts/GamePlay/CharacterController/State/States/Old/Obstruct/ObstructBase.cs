using System;
using System.Collections.Generic;


using UnityEngine;

namespace Saber.CharacterController
{
    public abstract class ObstructBase
    {
        protected SActor Actor;
        private Action m_ActionExit;

        public ObstructBase(SActor actor, Action actionExit)
        {
            Actor = actor;
            m_ActionExit = actionExit;
        }

        public abstract void Enter(DamageInfo damageInfo);

        public abstract void ReEnter(DamageInfo damageInfo);

        public virtual void OnStay(DamageInfo damageInfo, float deltaTime)
        {
        }

        protected virtual void Exit()
        {
            m_ActionExit();
        }
    }
}