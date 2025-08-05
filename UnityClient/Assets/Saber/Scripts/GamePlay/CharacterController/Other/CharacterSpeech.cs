using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

namespace Saber.CharacterController
{
    public class CharacterSpeech
    {
        /*
        [Serializable]
        public class SpeechInfo
        {
            public BakedData[] m_Clips;
        }

        private SCharacter m_Character;
        private uLipSyncBakedDataPlayer m_LipSync;
        private SpeechInfo m_SpeechInfo;

        public bool IsValid => m_LipSync != null && m_SpeechInfo != null && m_SpeechInfo.m_Clips.Length > 0;
        public bool IsSpeeching => m_LipSync != null && m_LipSync.isPlaying;

        public CharacterSpeech(SCharacter character, SpeechInfo speechInfo)
        {
            m_Character = character;
            m_SpeechInfo = speechInfo;
            m_LipSync = character.gameObject.GetComponent<uLipSyncBakedDataPlayer>();
        }

        public void Speech(int index)
        {
            if (IsValid && index >= 0 && index < m_SpeechInfo.m_Clips.Length)
            {
                var clip = m_SpeechInfo.m_Clips[index];
                m_LipSync.bakedData = clip;
                m_LipSync.Play();

                if (m_Character.CurrentStateType == EStateType.Idle)
                {
                    Idle idle = (Idle)m_Character.CStateMachine.CurrentState;
                    idle.PlayTalkingGesture();
                }
            }
        }

        public void RandomSpeech()
        {
            if (IsValid)
            {
                int index = UnityEngine.Random.Range(0, m_SpeechInfo.m_Clips.Length);
                Speech(index);
            }
        }
        */


        /*
        IEnumerator Synthesis(string content, int voiceID)
        {
            string url = $"http://1.94.131.28:19463/tts?content={content}&id={voiceID}";
            UnityEngine.Debug.Log($"#1 Synthesis,content:{content},url:{url}");
            using (var request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return request.SendWebRequest();

                if (!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogError($"Error:{request.error},url:{url}");
                    yield break;
                }

                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);
                m_AudioSource.clip = audioClip;
                m_AudioSource.Play();
            }
        }
        */
    }
}