using System;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>处理角色动画相关事务</summary>
    public class CharacterAnimation
    {
        private List<AnimatorSmoothFloatSetter> m_ListSmoothFloat = new();
        private Dictionary<EAnimatorParams, AnimatorSmoothFloatSetter> m_DicSmoothFloat = new();
        private AnimatorLayer[] m_Layers;
        private AnimatorLayer.IHandler m_Handler;
        private AnimatorOverrideController m_AnimatorOverrideController;
        private string m_CurPlayingClipState;
        private Action m_OnClipFinishedPlay;
        private AnimationClip m_CurPlayingClip;
        private float m_TimerCartonFrames, m_CartonFrameSpeed;

        public SActor Actor { get; private set; }
        public Animator AnimatorObj { get; private set; }

        public AnimatorLayer this[int index]
        {
            get
            {
                if (m_Layers != null && index >= 0 && index < m_Layers.Length)
                {
                    return m_Layers[index];
                }

                return null;
            }
        }

        public float NormalizedTimeOfPlayingClip
        {
            get
            {
                if (m_CurPlayingClipState.IsEmpty())
                    return 0;
                return GetAnimNormalizedTime(m_CurPlayingClipState, 0);
            }
        }

        public AnimationClip PlayingClip => m_CurPlayingClip;
        public bool IsClipPlaying => m_CurPlayingClipState.IsNotEmpty();


        public CharacterAnimation(SActor actor, AnimatorLayer.IHandler handler)
        {
            Actor = actor;
            m_Handler = handler;
            AnimatorObj = actor.GetComponent<Animator>();
            AnimatorObj.updateMode = AnimatorUpdateMode.AnimatePhysics;
            AnimatorObj.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            m_AnimatorOverrideController = new AnimatorOverrideController();
            m_AnimatorOverrideController.runtimeAnimatorController = AnimatorObj.runtimeAnimatorController;
            AnimatorObj.runtimeAnimatorController = m_AnimatorOverrideController;
        }

        public void Update()
        {
            AnimatorObj.speed = Actor.TimeMultiplier;

            // init layers
            if (m_Layers == null && AnimatorObj.layerCount > 0)
            {
                m_Layers = new AnimatorLayer[AnimatorObj.layerCount];
                for (int i = 0; i < AnimatorObj.layerCount; i++)
                {
                    if (i == 0)
                        m_Layers[i] = new AnimatorBaseLayer(AnimatorObj, m_Handler);
                    else
                        m_Layers[i] = new AnimatorMaskLayer(AnimatorObj, i, m_Handler);
                }
            }

            if (m_Layers != null)
            {
                for (int i = 0; i < AnimatorObj.layerCount; i++)
                    m_Layers[i].Update();
            }

            UpdateSmoothFloat();

            UpdateClipPlaying();

            if (m_TimerCartonFrames > 0)
                UpdateCartonFrames();
        }


        #region Play

        public bool HasAnim(string animName, int layer)
        {
            return AnimatorObj.HasState(layer, animName.GetAnimatorHash());
        }

        public void Play(string anim, int layer = 0, bool force = false, float blendTime = 0.1f, float exitTime = 0.9f,
            Action onFinished = null)
        {
            if (anim.IsEmpty() || layer < 0 || m_Layers == null)
            {
                Debug.LogWarning("anim.IsEmpty() || layer < 0 || m_Layers == null");
                return;
            }

            if (layer >= m_Layers.Length)
            {
                Debug.LogError(
                    $"Play anim {anim} failed, error:layer ({layer}) >= m_Layers.Length ({m_Layers.Length})");
                return;
            }

            m_Layers[layer].Play(anim, force, blendTime, exitTime, onFinished);

            // if (Actor.IsPlayer)
            //     Debug.Log($"Play anim:{anim}");
        }

        public bool IsPlayingOrWillPlay(string anim, float exitTime = 0.99f)
        {
            if (string.IsNullOrEmpty(anim))
            {
                return false;
            }

            if (IsTheClipPlaying(anim))
            {
                return true;
            }

            if (m_Layers == null)
            {
                return false;
            }

            for (int i = 0; i < m_Layers.Length; i++)
            {
                var l = m_Layers[i];
                if (l.IsPlayingOrWillPlay(anim, exitTime))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsPlayingOrWillPlay(string anim, int layer, float exitTime = 0.99f)
        {
            if (string.IsNullOrEmpty(anim))
                return false;
            if (m_Layers == null)
            {
                return false;
            }

            var l = m_Layers[layer];
            return l.IsPlayingOrWillPlay(anim, exitTime);
        }

        public bool IsReallyPlaying(string anim)
        {
            if (string.IsNullOrEmpty(anim))
                return false;
            if (m_Layers == null)
            {
                return false;
            }

            for (int i = 0; i < m_Layers.Length; i++)
            {
                var l = m_Layers[i];
                if (l.CurStateInfo.IsName(anim))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsReallyPlaying(string anim, int layer)
        {
            if (string.IsNullOrEmpty(anim))
                return false;
            if (m_Layers != null && layer >= 0 && layer < m_Layers.Length)
                return m_Layers[layer].CurStateInfo.IsName(anim);
            return false;
        }

        public float GetAnimNormalizedTime(string animName, int layer = 0)
        {
            if (m_Layers != null && layer >= 0 && layer < m_Layers.Length)
            {
                var l = m_Layers[layer];
                if (l.IsReallyPlaying(animName))
                    return l.AnimNormalizedTime;
            }

            return 0;
        }

        public float GetAnimNormalizedTime(int layer)
        {
            if (m_Layers != null && layer >= 0 && layer < m_Layers.Length)
                return m_Layers[layer].AnimNormalizedTime;
            return 0;
        }

        public bool IsName(string animName, int layer)
        {
            if (m_Layers != null && layer >= 0 && layer < m_Layers.Length)
            {
                var info = m_Layers[layer].CurStateInfo;
                return info.IsName(animName);
            }

            return false;
        }

        public void StopMaskLayerAnims()
        {
            if (m_Layers != null)
            {
                for (int i = 0; i < m_Layers.Length; i++)
                {
                    m_Layers[i].StopAnim();
                }
            }
        }

        #endregion


        #region set param

        public void ResetTrigger(EAnimatorParams p)
        {
            AnimatorObj.ResetTrigger(p.GetAnimatorHash());
            //Debug.Log("animator set trigger:" + p);
        }

        public void SetTrigger(EAnimatorParams p)
        {
            AnimatorObj.SetTrigger(p.GetAnimatorHash());
            //Debug.Log("animator set trigger:" + p);
        }

        public void SetInt(EAnimatorParams p, int v)
        {
            AnimatorObj.SetInteger(p.GetAnimatorHash(), v);
            //Debug.Log($"animator set int:{p} {v}");
        }

        public void SetFloat(EAnimatorParams p, float v)
        {
            AnimatorObj.SetFloat(p.GetAnimatorHash(), v);
            //Debug.Log($"animator set float:{p} {v}");
        }

        public float GetFloat(EAnimatorParams p)
        {
            return AnimatorObj.GetFloat(p.GetAnimatorHash());
            //Debug.Log($"animator set float:{p} {v}");
        }

        public void SetBool(EAnimatorParams p, bool v)
        {
            AnimatorObj.SetBool(p.GetAnimatorHash(), v);
            //Debug.Log($"animator set bool:{p} {v}");
        }

        public void SetSmoothFloat(EAnimatorParams id, float value)
        {
            if (!m_DicSmoothFloat.TryGetValue(id, out var obj))
            {
                obj = new AnimatorSmoothFloatSetter(this, id);
                m_DicSmoothFloat.Add(id, obj);
                m_ListSmoothFloat.Add(obj);
            }

            obj.Target = value;
        }

        public float GetCurSmoothFloat(EAnimatorParams id)
        {
            if (m_DicSmoothFloat.TryGetValue(id, out var obj))
                return obj.CurValue;
            return 0;
        }

        public void ResetSmoothFloat(EAnimatorParams id, float value)
        {
            SetFloat(id, value);
            if (m_DicSmoothFloat.TryGetValue(id, out var obj))
                obj.ResetValue(value);
        }

        void UpdateSmoothFloat()
        {
            for (int i = 0; i < m_ListSmoothFloat.Count; i++)
            {
                m_ListSmoothFloat[i].Update();
            }
        }

        #endregion


        #region Play Clip

        public void PlayClip(string clipPath, Action onFinished)
        {
            AnimationClip clip = GameApp.Entry.Asset.LoadClip(clipPath);
            if (clip)
                PlayClip(clip, onFinished);
            else
                Debug.LogError($"animClip == null,path:{clipPath}");
        }

        public void PlayClip(AnimationClip animClip, Action onFinished)
        {
            if (animClip == null)
            {
                Debug.LogError("animClip == null");
                return;
            }

            m_CurPlayingClipState = IsReallyPlaying("T1", 0) ? "T2" : "T1";
            m_OnClipFinishedPlay = onFinished;
            m_CurPlayingClip = animClip;

            m_AnimatorOverrideController[m_CurPlayingClipState] = animClip;
            Play(m_CurPlayingClipState, force: true);

            // Debug.Log($"play clip:{animClip.name}, state:{m_CurPlayingClipState}, action:{m_OnClipFinishedPlay != null}");
        }

        private void UpdateClipPlaying()
        {
            // if (IsClipPlaying)
            //     Debug.Log($"cur {PlayingClip.name} progress:{NormalizedTimeOfPlayingClip}");

            if (m_CurPlayingClipState.IsEmpty())
            {
                return;
            }

            if (!IsPlayingOrWillPlay(m_CurPlayingClipState, 0))
            {
                OnClipEndedPlay();
            }
        }

        void OnClipEndedPlay()
        {
            // Debug.Log($"clip end play:{m_CurPlayingClip.name}, state:{m_CurPlayingClipState}, action:{m_OnClipFinishedPlay != null}");

            m_CurPlayingClipState = null;
            m_CurPlayingClip = null;
            if (m_OnClipFinishedPlay != null)
            {
                Action temp = m_OnClipFinishedPlay;
                // 清空必须要在Action之前执行，否则如果Action里调用 public void PlayClip(AnimationClip animClip, Action onFinished)
                // 则下一步Action会被清空，它不应该被清空，因为这是一个新的动画。
                m_OnClipFinishedPlay = null;
                temp();
            }
        }

        public bool IsTheClipPlaying(string clipName)
        {
            return IsClipPlaying && m_CurPlayingClip.name == clipName;
        }

        #endregion


        #region Carton frames

        public void CartonFrames(float time, float speed)
        {
            m_TimerCartonFrames = time;
            m_CartonFrameSpeed = speed;
            this.AnimatorObj.speed = speed;
        }

        void UpdateCartonFrames()
        {
            m_TimerCartonFrames -= Time.deltaTime;
            if (m_TimerCartonFrames > 0)
                this.AnimatorObj.speed = m_CartonFrameSpeed;
            else
                this.AnimatorObj.speed = 1;
        }

        #endregion
    }
}