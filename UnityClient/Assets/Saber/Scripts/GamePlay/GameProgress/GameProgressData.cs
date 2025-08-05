using System;
using System.Collections.Generic;

namespace Saber
{
    [Serializable]
    public class GameProgressData
    {
        public List<SceneProgressData> m_SceneProgress;
        public int m_LastStayingSceneID;
        public int m_lastStayingGodStateIndex;
        //public int[] m_Clothes;
    }

    [Serializable]
    public class SceneProgressData
    {
        public int m_SceneID;
        public List<int> m_FiredGodStatues;
    }
}