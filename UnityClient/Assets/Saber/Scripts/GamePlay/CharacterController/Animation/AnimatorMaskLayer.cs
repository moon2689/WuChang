using UnityEngine;
using System;

namespace Saber.CharacterController
{
    public class AnimatorMaskLayer : AnimatorLayer
    {
        enum EState
        {
            None,
            WaitPlay,
            Playing,
        }

        private const float k_BlendTime = 0.2f;

        private string m_CurAnim;
        private EState m_State;

        bool IsCurAnimReallyPlaying => !string.IsNullOrEmpty(m_CurAnim) && IsReallyPlaying(m_CurAnim);


        public AnimatorMaskLayer(Animator animator, int layer, IHandler handler)
            : base(animator, layer, handler)
        {
            if (layer == 0)
            {
                throw new InvalidOperationException("layer == 0");
            }

            SetWeight(0);
        }

        public override void Update()
        {
            base.Update();
            SetMaskLayerWeight();
        }

        void SetMaskLayerWeight()
        {
            float weight = 0;
            if (m_State == EState.None)
            {
            }
            else if (m_State == EState.WaitPlay)
            {
                if (IsCurAnimReallyPlaying)
                {
                    m_State = EState.Playing;
                }
            }
            else if (m_State == EState.Playing)
            {
                if (IsCurAnimReallyPlaying)
                {
                    if (AnimNormalizedTime < k_BlendTime)
                    {
                        weight = AnimNormalizedTime / k_BlendTime;
                        weight = Mathf.Clamp01(weight);
                    }
                    else if (AnimNormalizedTime > 0.95f - k_BlendTime)
                    {
                        weight = (0.95f - AnimNormalizedTime) / k_BlendTime;
                        weight = Mathf.Clamp01(weight);
                    }
                    else
                    {
                        weight = 1;
                    }

                    // Debug.Log($"set weight: {weight}, time: {AnimNormalizedTime}, layer: {base.m_Layer}");
                }
                else
                {
                    m_State = EState.None;
                    m_CurAnim = null;
                }
            }

            SetWeight(weight);
        }

        public override void Play(string anim, bool force, float blendTime, float timeOffset, Action onFinished = null)
        {
            if (!string.IsNullOrEmpty(anim))
            {
                m_State = EState.WaitPlay;
                m_CurAnim = anim;
                base.Play(anim, force, blendTime, timeOffset, onFinished);
            }
        }

        public override void StopAnim()
        {
            base.StopAnim();
            m_CurAnim = null;
        }
    }
}