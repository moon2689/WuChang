using UnityEngine;

namespace Saber.CharacterController
{
    public abstract class AnimPointTimeEvent : AnimEventBase
    {
        [Range(0, 1)] public float m_TriggerTime;
        private bool m_Triggered;
        private bool m_InTransition;

        public abstract EAnimTriggerEvent EventType { get; }

        public override void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(anim, stateInfo, layerIndex);
            m_Triggered = false;
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

            if (!m_Triggered)
            {
                var time = state.normalizedTime % 1;
                //Debug.Log($"name hash:{state.shortNameHash}  time:{time}  layer:{layer}");
                if (time > m_TriggerTime)
                {
                    m_Triggered = true;
                    OnTrigger(anim, state);
                }
            }
        }

        protected virtual void OnTrigger(Animator anim, AnimatorStateInfo state)
        {
            m_Actor.OnTriggerAnimEvent(this);
        }
    }
}