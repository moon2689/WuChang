using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.CharacterController
{
    public abstract class ObstructStateBase : ActorStateBase
    {
        private bool m_CanExit;
        protected ObstructBase m_CurrentObstruct;


        public DamageInfo Damage { get; set; }

        public override bool CanExit => m_CanExit;


        protected abstract ObstructBase GetCurrentHitObj();


        public ObstructStateBase() : base(EStateType.GetHit)
        {
        }

        public override void Enter()
        {
            base.Enter();
            m_CanExit = false;
            m_CurrentObstruct = GetCurrentHitObj();
            if (m_CurrentObstruct != null)
                m_CurrentObstruct.Enter(Damage);
            else
                base.Exit();
        }

        public override void ReEnter()
        {
            base.ReEnter();
            m_CanExit = false;
            m_CurrentObstruct = GetCurrentHitObj();
            m_CurrentObstruct.ReEnter(Damage);
        }

        public override void OnStay()
        {
            base.OnStay();
            m_CurrentObstruct.OnStay(Damage, DeltaTime);
        }

        public override void OnTriggerAnimClipEvent(string str)
        {
            base.OnTriggerAnimClipEvent(str);
            if (str == "Invincible")
            {
                Actor.Invincible = true;
            }
            else if (str == "CanExit")
            {
                m_CanExit = true;
            }
            else
            {
                Debug.LogError("Unknown event:" + str);
            }
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.AnimCanExit)
            {
                Exit();
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            Actor.CPhysic.UseGravity = true;
            Actor.Invincible = false;
            Actor.CAnim.Play("Idle");
        }
    }
}