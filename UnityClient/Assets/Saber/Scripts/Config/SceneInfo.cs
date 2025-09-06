using System;
using System.Collections.Generic;
using Saber.AI;
using Saber.CharacterController;
using Saber.World;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.Config
{
    [CreateAssetMenu(menuName = "Saber/Scene Info", fileName = "SceneInfo", order = 1)]
    public class SceneInfo : ScriptableObject
    {
        public SceneBaseInfo[] m_Scenes;

        private Dictionary<int, SceneBaseInfo> m_DicSceneInfos;

        public SceneBaseInfo GetSceneInfoByID(int id)
        {
            if (m_DicSceneInfos == null)
            {
                m_DicSceneInfos = new();
                foreach (var s in m_Scenes)
                {
                    m_DicSceneInfos.Add(s.m_ID, s);
                }
            }

            m_DicSceneInfos.TryGetValue(id, out var tar);
            return tar;
        }
    }

    [Serializable]
    public class SceneBaseInfo
    {
        public string m_Name;
        public int m_ID;
        public string m_ResName;
        public ESkyboxType m_SkyboxType;
        public bool m_OpenPostprocess;
        public float m_ShadowDistance = 20;
        
        // 通过工具填充
        public IdolInfo[] m_Idols;
        public int[] m_Portals;
    }

    [Serializable]
    public enum ESkyboxType
    {
        None,
        DynamicSkybox,
        StaticSkybox,
    }
    
    [Serializable]
    public class IdolInfo
    {
        public int m_ID;
        public string m_Name;
    }
}