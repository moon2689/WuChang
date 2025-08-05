using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Saber.CharacterController
{
    [AddComponentMenu("Saber/AnimationEventListener")]
    public class AnimEventListener : StateMachineBehaviour
    {
        [SerializeField] private AnimTriggerEvent[] m_TriggerEvents;
        [SerializeField] private AnimRangeEvent[] m_RangeEvents;
        private int m_CurTriggerEventIndex;


        private void Awake()
        {
            Array.Sort(m_TriggerEvents, CompareAnimEvent);
        }

        int CompareAnimEvent(AnimTriggerEvent x, AnimTriggerEvent y)
        {
            return x.m_TriggerTime.CompareTo(y.m_TriggerTime);
        }

        public override void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_CurTriggerEventIndex = 0;

            foreach (var range in m_RangeEvents)
            {
                range.State = 1;
            }
        }

        public override void OnStateUpdate(Animator anim, AnimatorStateInfo state, int layer)
        {
            var time = state.normalizedTime % 1;
            //Debug.Log($"name hash:{state.shortNameHash}  time:{time}  layer:{layer}");
            if (m_CurTriggerEventIndex >= 0 && m_CurTriggerEventIndex < m_TriggerEvents.Length)
            {
                var item = m_TriggerEvents[m_CurTriggerEventIndex];
                if (time >= item.m_TriggerTime)
                {
                    //Debug.Log($"anim time:{time}  event:{item.m_AnimEvent}  trigger time:{item.m_TriggerTime}");
                    OnTriggerAnimEvent(anim, item);
                    ++m_CurTriggerEventIndex;
                }
            }

            // Range events
            foreach (var range in m_RangeEvents)
            {
                if (range.State == 1)
                {
                    if (time >= range.m_RangeTime.minValue)
                    {
                        OnRangeEventEnter(anim, range);
                        range.State = 2;
                    }
                }
                else if (range.State == 2)
                {
                    if (time >= range.m_RangeTime.maxValue)
                    {
                        OnRangeEventExit(anim, range);
                        range.State = 3;
                    }
                }
            }
        }

        void OnTriggerAnimEvent(Animator anim, AnimTriggerEvent animEvent)
        {
            //anim.gameObject.SendMessage("OnTriggerAnimEvent", animEvent);
        }

        void OnRangeEventEnter(Animator anim, AnimRangeEvent animRangeEvent)
        {
            //anim.gameObject.SendMessage("OnRangeEventEnter", animRangeEvent);
        }

        void OnRangeEventExit(Animator anim, AnimRangeEvent animRangeEvent)
        {
            //anim.gameObject.SendMessage("OnRangeEventExit", animRangeEvent);
        }

        public override void OnStateExit(Animator anim, AnimatorStateInfo state, int layer)
        {
        }
    }

    [Serializable]
    public class AnimTriggerEvent
    {
        public EAnimTriggerEvent m_AnimEvent;
        [Range(0, 1)] public float m_TriggerTime = 0.5f;

        public string m_Param;
        public Object m_Res;
    }

    [Serializable]
    public class AnimRangeEvent
    {
        public EAnimRangeEvent m_AnimEvent;
        [MinMaxRange(0, 1)] public RangedFloat m_RangeTime;

        public string m_Param;

        /// <summary>1 unenter,2 stay,3 exit</summary>
        public int State { get; set; }
    }
}