using UnityEngine;

namespace Saber.CharacterController
{
    public abstract class AnimRangeTimeEvent : AnimEventBase
    {
        public enum EState
        {
            Unenter,
            Stay,
            Exit,
        }

        [MinMaxRange(0, 1)] public RangedFloat m_RangeTime;

        private EState m_State;
        private bool m_InTransition;
        private bool m_ExitEventTriggered;

        public abstract EAnimRangeEvent EventType { get; }

        public override void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(anim, stateInfo, layerIndex);
            m_State = EState.Unenter;
            m_InTransition = true;
        }

        public override void OnStateUpdate(Animator anim, AnimatorStateInfo state, int layer)
        {
            if (m_InTransition && !anim.IsInTransition(layer))
            {
                m_InTransition = false;
            }

            if (m_InTransition)
            {
                return;
            }

            var time = state.normalizedTime % 1;
            //Debug.Log($"name hash:{state.shortNameHash}  time:{time}  layer:{layer}");

            // Range events
            if (m_State == EState.Unenter)
            {
                if (time >= m_RangeTime.minValue)
                {
                    OnRangeEventEnter();
                    m_State = EState.Stay;
                }
            }
            else if (m_State == EState.Stay)
            {
                if (time >= m_RangeTime.maxValue)
                {
                    OnRangeEventExit();
                    m_State = EState.Exit;
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            OnRangeEventExit();
        }

        protected virtual void OnRangeEventEnter()
        {
            m_ExitEventTriggered = false;
            base.m_Actor.OnTriggerAnimRangeTimeEvent(this, true);
        }

        protected virtual void OnRangeEventExit()
        {
            if (!m_ExitEventTriggered)
            {
                m_ExitEventTriggered = true;
                base.m_Actor.OnTriggerAnimRangeTimeEvent(this, false);
            }
        }
    }
}