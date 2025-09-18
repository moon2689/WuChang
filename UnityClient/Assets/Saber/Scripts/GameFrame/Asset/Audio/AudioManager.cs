using System;
using System.Collections.Generic;
using System.Linq;
using Saber.Frame;
using UnityEngine;
using YooAsset;

namespace Saber
{
    public class AudioManager : MonoBehaviour
    {
        List<AudioPlayer> m_audioPlayers = new List<AudioPlayer>();
        AudioPlayer m_curBGMPlayer;
        Dictionary<string, AudioClip> m_dicAssets = new();


        static AudioManager s_instance;
        

        public bool IsBGMPlaying =>
            m_curBGMPlayer != null && m_curBGMPlayer.Actived && m_curBGMPlayer.AudioSource.isPlaying;

        public string CurBGMName
        {
            get
            {
                if (m_curBGMPlayer != null && m_curBGMPlayer.Actived && m_curBGMPlayer.Clip != null)
                {
                    return m_curBGMPlayer.Clip.name;
                }

                return null;
            }
        }

        public static AudioManager GetInstance()
        {
            if (s_instance == null)
            {
                var go = new GameObject(typeof(AudioManager).ToString());
                go.transform.parent = GameApp.Instance.transform;
                s_instance = go.AddComponent<AudioManager>();
            }

            return s_instance;
        }

        public AudioPlayer Play(AudioClip clip, float volume, Vector3 pos, bool is3D,
            Action<AudioPlayer> onFinished = null)
        {
            if (clip == null)
            {
                Debug.LogError("clip == null");
                return null;
            }

            AudioPlayer player = m_audioPlayers.FirstOrDefault(p => !p.Actived);
            if (player == null)
            {
                player = AudioPlayer.Create(transform);
                m_audioPlayers.Add(player);
            }

            player.Play(clip, volume, false, pos, is3D, onFinished);
            return player;
        }

        public AudioPlayer Play3DSound(AudioClip clip, Vector3 pos, Action<AudioPlayer> onFinished = null)
        {
            return Play(clip, 1, pos, true, onFinished);
        }

        AssetHandle GetClip(string name, Action<AudioClip> onGetted)
        {
            m_dicAssets.TryGetValue(name, out AudioClip asset);
            if (asset)
            {
                onGetted?.Invoke(asset);
            }
            else
            {
                return GameApp.Entry.Asset.LoadAsset<AudioClip>(name, c =>
                {
                    if (c != null)
                    {
                        m_dicAssets.Add(name, c);
                        onGetted?.Invoke(c);
                    }
                    else
                    {
                        Debug.LogError("audio clip is null, name:" + name);
                    }
                });
            }

            return null;
        }

        public void Play(string name, float volume, Vector3 pos, bool is3D, Action<AudioPlayer> onFinished = null)
        {
            GetClip(name, c => Play(c, volume, pos, is3D, onFinished));
        }

        public void Play2DSound(string name, Action<AudioPlayer> onFinished = null)
        {
            Play(name, 1, Vector3.zero, false, onFinished);
        }

        public void PlaySoundSkillFailed()
        {
            Play2DSound("Sound/Skill/SkillFailed");
        }

        public void PlayCommonClick()
        {
            Play2DSound("Sound/UI/ButtonClick");
        }

        public void Play3DSound(string name, Vector3 pos, Action<AudioPlayer> onFinished = null)
        {
            Play(name, 1, pos, true, onFinished);
        }

        public void PlayBGM(AudioClip clip, float volume, bool loop, Action<AudioPlayer> onFinished = null)
        {
            if (m_curBGMPlayer == null)
                m_curBGMPlayer = AudioPlayer.Create(transform);
            m_curBGMPlayer.Play(clip, volume, loop, Vector3.zero, false, onFinished);
        }

        public void PlayBGM(string name, float volume, bool loop, Action<AudioPlayer> onFinished = null)
        {
            GetClip(name, c => PlayBGM(c, volume, loop, onFinished));
        }

        public void SetBGMVolume(float volume)
        {
            if (m_curBGMPlayer != null)
                m_curBGMPlayer.SetVolume(volume);
        }

        public void StopBGM()
        {
            if (m_curBGMPlayer != null)
            {
                m_curBGMPlayer.Stop();
                m_curBGMPlayer = null;
            }
        }

        public void Stop(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            for (int i = 0; i < m_audioPlayers.Count; i++)
            {
                var p = m_audioPlayers[i];
                if (p.Actived && p.Clip.name == name)
                    p.Stop();
            }
        }

        public void Release()
        {
            foreach (var pair in m_dicAssets)
            {
                GameObject.Destroy(pair.Value);
            }

            m_dicAssets.Clear();
        }
    }
}