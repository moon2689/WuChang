using System.Collections;
using System.Collections.Generic;
using Saber;
using Saber.Frame;
using UnityEngine;

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / SFX")]
    public class AbilityEventObj_SFX : AbilityEventObj
    {
        public List<AudioClip> clips;
        public float m_Volume = 1;
        [Range(0, 100)] public int m_TriggerProbability = 100;

        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventTime;
        }

        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_SFX(this);
        }

#if UNITY_EDITOR
        public override AbilityEventPreview InitializePreview()
        {
            return new AbilityEventPreview_SFX(this);
        }
#endif
    }

    public partial class AbilityEventEffect_SFX : AbilityEventEffect
    {
        private AudioPlayer m_AudioPlayer;

        public override void StartEffect()
        {
            base.StartEffect();
            if (UnityEngine.Random.Range(0, 100) < EventObj.m_TriggerProbability)
            {
                AudioClip clip = EventObj.clips[Random.Range(0, EventObj.clips.Count)];
                //Actor.PlaySound(clip);
                m_AudioPlayer = GameApp.Entry.Game.Audio.Play3DSound(clip, Actor.transform.position);
            }
        }

        /*
        protected override void EndEffect()
        {
            base.EndEffect();
            if (m_AudioPlayer != null && m_AudioPlayer.isActiveAndEnabled)
            {
                m_AudioPlayer.Stop();
            }
        }
        */
    }

    public partial class AbilityEventEffect_SFX : AbilityEventEffect
    {
        private AbilityEventObj_SFX EventObj { get; set; }

        public AbilityEventEffect_SFX(AbilityEventObj obj) : base(obj)
        {
            m_EventObj = obj;
            EventObj = (AbilityEventObj_SFX)obj;
        }

        /*
        public override void StartEffect()
        {
            base.StartEffect();
            var clip = ((AbilityEventObj_SFX)m_EventObj).clips[Random.Range(0, ((AbilityEventObj_SFX)m_EventObj).clips.Count)];
            clip.PlayClip(((AbilityEventObj_SFX)m_EventObj).Volume);
        }
        */
    }
}