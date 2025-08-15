using System;
using System.Collections;
using System.Collections.Generic;
using Saber;
using UnityEngine;
using UnityEngine.AI;

namespace Saber.CharacterController
{
    public class HitBox : MonoBehaviour
    {
        protected AbilityEventEffect_CreateHitBox m_EventObj;

        public SActor Actor => m_EventObj.Actor;
        

        public void Init(AbilityEventEffect_CreateHitBox eventObj)
        {
            m_EventObj = eventObj;
        }
    }
}