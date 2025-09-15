using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering.UI;

namespace Saber.CharacterController
{
    public abstract class AnimatorLayer
    {
        private Animator m_Animator;
        protected int m_Layer;
        private IHandler m_Handler;
        private int? m_TransitionAnimID;
        private AnimatorStateInfo m_CurStateInfo;
        private int m_OldStateNameHash;
        private Dictionary<int, Action> m_OnAnimFinishedEvent = new();


        public interface IHandler
        {
            // Dictionary<int, List<Item_AnimEvent>> DicAnimEvents { get; }
            // void OnTriggerAnimEvent(AnimEventType animEvent, Item_AnimEvent config);

            void OnAnimEnter(int nameHash, int layer);
            void OnAnimExit(int nameHash, int layer);
        }


        public AnimatorStateInfo CurStateInfo => m_CurStateInfo;
        bool IsSwitchingAnim => m_TransitionAnimID != null;
        public float AnimNormalizedTime => IsSwitchingAnim ? 0 : m_CurStateInfo.normalizedTime % 1;


        public AnimatorLayer(Animator animator, int layer, IHandler handler)
        {
            m_Animator = animator;
            m_Layer = layer;
            m_Handler = handler;
            SetWeight(1);
        }

        protected void SetWeight(float weight)
        {
            m_Animator.SetLayerWeight(m_Layer, weight);
        }

        public virtual void Update()
        {
            /*
            if (m_Layer == 0)
            {
                if (m_Animator.IsInTransition(m_Layer))
                {
                    var stateInfo = m_Animator.GetNextAnimatorStateInfo(m_Layer);
                    Debug.Log($"{stateInfo.shortNameHash}\t{stateInfo.normalizedTime}\ttransition");
                }
                else
                {
                    var stateInfo = m_Animator.GetCurrentAnimatorStateInfo(m_Layer);
                    Debug.Log($"{stateInfo.shortNameHash}\t{stateInfo.normalizedTime}");
                }
            }
            */

            if (m_Animator.IsInTransition(m_Layer))
            {
                m_CurStateInfo = m_Animator.GetNextAnimatorStateInfo(m_Layer);
            }
            else
            {
                m_CurStateInfo = m_Animator.GetCurrentAnimatorStateInfo(m_Layer);
            }

            if (m_CurStateInfo.shortNameHash != m_OldStateNameHash)
            {
                int enterAnimID = m_CurStateInfo.shortNameHash;
                int exitAnimID = m_OldStateNameHash;
                m_OldStateNameHash = m_CurStateInfo.shortNameHash;
                m_Handler.OnAnimExit(exitAnimID, m_Layer);
                m_Handler.OnAnimEnter(enterAnimID, m_Layer);

                if (m_OnAnimFinishedEvent.TryGetValue(exitAnimID, out Action action))
                {
                    m_OnAnimFinishedEvent.Remove(exitAnimID);
                    action?.Invoke();
                }
            }

            if (m_TransitionAnimID != null && m_CurStateInfo.shortNameHash == m_TransitionAnimID.Value)
            {
                m_TransitionAnimID = null;
            }
        }

        // Animator trigger
        public virtual void Play(string anim, bool force, float blendTime, float timeOffset, Action onFinished = null)
        {
            if (string.IsNullOrEmpty(anim))
                return;

#if UNITY_EDITOR
            if (!m_Animator.HasState(this.m_Layer, anim.GetAnimatorHash()))
            {
                Debug.LogError($"No anim {anim} in animator {m_Animator.name}, layer {this.m_Layer}", m_Animator);
            }
#endif

            //Debug.Log($"Play anim:{anim}, hash:{anim.GetAnimatorHash()}, layer:{m_Layer}, blendTime:{blendTime}, exitTime:{exitTime}");
            if (force || !IsReallyPlaying(anim))
            {
                if (timeOffset > 0)
                {
                    m_Animator.CrossFadeInFixedTime(anim, blendTime, m_Layer, timeOffset);
                }
                else
                {
                    m_Animator.CrossFadeInFixedTime(anim, blendTime, m_Layer);
                }

                m_TransitionAnimID = anim.GetAnimatorHash();
            }

            int id = anim.GetAnimatorHash();
            if (m_OnAnimFinishedEvent.TryGetValue(id, out Action action) && action != null)
                action += onFinished;
            else
                m_OnAnimFinishedEvent[id] = onFinished;
        }

        public bool IsPlayingOrWillPlay(string anim, float exitTime = 0.99f)
        {
            if (string.IsNullOrEmpty(anim))
                return false;

            if (m_TransitionAnimID != null && m_TransitionAnimID == anim.GetAnimatorHash())
                return true;

            if (m_CurStateInfo.loop)
                return m_CurStateInfo.IsName(anim);
            else
                return m_CurStateInfo.IsName(anim) && m_CurStateInfo.normalizedTime < exitTime;
        }

        public bool IsReallyPlaying(string anim)
        {
            if (string.IsNullOrEmpty(anim))
                return false;
            return m_CurStateInfo.IsName(anim);
        }

        public virtual void StopAnim()
        {
        }
    }
}