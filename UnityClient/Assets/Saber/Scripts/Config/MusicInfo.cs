using System;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.Config
{
    [CreateAssetMenu(menuName = "Saber/Music Info", fileName = "MusicInfo", order = 8)]
    public class MusicInfo : ScriptableObject
    {
        public string[] m_BGMs;
        public string m_LoginMusic;
        public string m_CommonBattleMusic;

        public string GetRandomBGM()
        {
            int index = UnityEngine.Random.Range(0, m_BGMs.Length);
            return m_BGMs[index];
        }
    }
}