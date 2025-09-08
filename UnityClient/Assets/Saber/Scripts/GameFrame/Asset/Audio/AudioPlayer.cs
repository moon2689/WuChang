using System;
using UnityEngine;

namespace Saber
{
    public class AudioPlayer : MonoBehaviour
    {
        AudioSource m_AudioSource;
        Action<AudioPlayer> m_onFinished;


        public bool Actived
        {
            private set => gameObject.SetActive(value);
            get => gameObject.activeSelf;
        }

        public AudioSource AudioSource => m_AudioSource;
        public AudioClip Clip => m_AudioSource.clip;


        public static AudioPlayer Create(Transform parent)
        {
            var go = new GameObject("AudioPlayer", typeof(AudioSource));
            go.transform.parent = parent;
            AudioPlayer ap = go.AddComponent<AudioPlayer>();
            ap.Init();
            return ap;
        }

        void Init()
        {
            m_AudioSource = gameObject.GetComponent<AudioSource>();
            m_AudioSource.playOnAwake = false;
            m_AudioSource.maxDistance = 50;
            m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
            Actived = false;
        }

        void Update()
        {
            if (m_AudioSource.clip != null && !m_AudioSource.isPlaying)
            {
                m_AudioSource.clip = null;
                Actived = false;

                if (m_onFinished != null)
                {
                    m_onFinished(this);
                    m_onFinished = null;
                }
            }
        }

        public void Play(AudioClip clip, float volume, bool loop, Vector3 pos, bool is3D, Action<AudioPlayer> onFinished)
        {
            if (clip == null)
                return;

            m_onFinished = onFinished;

            Actived = true;

            transform.name = clip.name;
            transform.position = pos;

            m_AudioSource.clip = clip;
            m_AudioSource.volume = volume;
            m_AudioSource.loop = loop;
            m_AudioSource.pitch = 1;
            m_AudioSource.spatialBlend = is3D ? 1 : 0;
            m_AudioSource.Play();
        }

        public void Stop()
        {
            m_AudioSource.Stop();
        }

        public void SetVolume(float volume)
        {
            m_AudioSource.volume = volume;
        }
    }
}