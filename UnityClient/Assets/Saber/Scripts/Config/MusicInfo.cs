using System;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.Config
{
    [CreateAssetMenu(menuName = "Saber/Music Info", fileName = "MusicInfo", order = 8)]
    public class MusicInfo : ScriptableObject
    {
        public AudioClip m_LoginBGMStart;
        public AudioClip m_LoginBGMLoop;
        
        public AudioClip m_BattleCommon;
        public AudioClip[] m_ExploreMusics;


        public AudioClip GetRandomExploreMusic()
        {
            int index = UnityEngine.Random.Range(0, m_ExploreMusics.Length);
            return m_ExploreMusics[index];
        }
    }
}