using System;
using System.Collections.Generic;
using Saber.Config;
using UnityEngine.Serialization;

namespace Saber
{
    [Serializable]
    public class GameProgressData
    {
        public List<SceneProgressData> m_SceneProgress;
        public int m_LastStayingSceneID;
        public int m_LastStayingShenKanID;
        public int[] m_Clothes;
        public PlayerPropItemInfo[] m_Items;
    }

    [Serializable]
    public class SceneProgressData
    {
        public int m_SceneID;
        public List<int> m_ActivedShenKan;
    }
}