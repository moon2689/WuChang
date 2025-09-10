using System;
using System.Collections;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Idle : ActorStateBase
    {
        private SCharacter Character;

        private bool m_IsSettingPosAndForward;
        private Vector3 m_ToSetForward;
        private float m_ToSetPosTime;
        private Vector3 m_ToSetPosSpeed;
        private Action m_SetPosAndForwardFinishedEvent;

        public override bool ApplyRootMotionSetWhenEnter => true;
        protected override ActorBaseStats.EStaminaRecSpeed StaminaRecSpeed => ActorBaseStats.EStaminaRecSpeed.Fast;


        public Idle() : base(EStateType.Idle)
        {
        }

        public override void Init(ActorStateMachine parent)
        {
            base.Init(parent);
            Character = base.Actor as SCharacter;
        }

        public override void OnStay()
        {
            base.OnStay();

            if (m_IsSettingPosAndForward)
                SetPosAndForward();
        }

        void SetPosAndForward()
        {
            if (m_ToSetPosTime > 0)
            {
                Actor.CPhysic.AdditivePosition += m_ToSetPosSpeed * base.DeltaTime;
                m_ToSetPosTime -= DeltaTime;
            }

            if (Actor.CPhysic.AlignForwardTo(m_ToSetForward, 720))
            {
                if (m_ToSetPosTime <= 0)
                {
                    m_IsSettingPosAndForward = false;
                    m_SetPosAndForwardFinishedEvent?.Invoke();
                }
            }
        }

        public void SetPosAndForward(Vector3 tarPos, Vector3 forward, Action onFinished)
        {
            Vector3 dis = tarPos - Actor.transform.position;
            dis.y = 0;
            float speed = 5f;
            m_ToSetPosSpeed = dis.normalized * speed;

            m_IsSettingPosAndForward = true;
            m_ToSetForward = forward;
            m_ToSetPosTime = dis.magnitude / speed;
            m_SetPosAndForwardFinishedEvent = onFinished;
        }

        public void IdolActive(Action onPlayFinish)
        {
            Character.CAnim.Play("IdolActive", onFinished: onPlayFinish);
        }

        public void IdolRest(Action onPlayFinish)
        {
            Character.CAnim.Play("IdolRest", onFinished: onPlayFinish);
        }

        public void IdolRestEnd(Action onPlayFinish)
        {
            Character.CAnim.Play("IdolRestEnd", onFinished: onPlayFinish);
        }

        public void BranchTeleport(Action onPlayFinish)
        {
            Character.CAnim.Play("IdolRest", onFinished: onPlayFinish);
        }

        public void GoHome(Action onPlayFinish)
        {
            Character.CAnim.Play("IdolRest", onFinished: onPlayFinish);
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.ShowWeapon)
            {
                Character.CMelee.CWeapon.ShowOrHideWeapon(true);
            }
            else if (eventObj.EventType == EAnimTriggerEvent.HideWeapon)
            {
                Character.CMelee.CWeapon.ShowOrHideWeapon(false);
            }
        }
    }
}