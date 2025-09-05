using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Saber
{
    [Serializable]
    public class GameProgressData
    {
        public List<SceneProgressData> m_SceneProgress;
        public int m_LastStayingSceneID;
        [FormerlySerializedAs("m_lastStayingGodStateIndex")] public int m_lastStayingIdolID;
        //public int[] m_Clothes;
    }

    [Serializable]
    public class SceneProgressData
    {
        public int m_SceneID;
        [FormerlySerializedAs("m_FiredGodStatues")] public List<int> m_FiredIdols;
    }
}