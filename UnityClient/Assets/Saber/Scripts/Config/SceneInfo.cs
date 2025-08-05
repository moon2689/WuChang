using System;
using System.Collections.Generic;
using Saber.AI;
using Saber.CharacterController;
using Saber.World;
using UnityEngine;

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
        public string m_SceneName;
        public ESkyboxType m_SkyboxType;
        public bool m_OpenPostprocess;
        public bool m_ActiveOtherActors = true;
        public float m_ShadowDistance = 20;
        public OtherActorBornPoint[] m_OtherActorBornPoints;
        public PortalPoint[] m_PortalPoints;
        public GodStatuePoint[] m_GodStatuePoint;
    }

    [Serializable]
    public enum ESkyboxType
    {
        None,
        DynamicSkybox,
        StaticSkybox,
    }

    [Serializable]
    public class OtherActorBornPoint
    {
        public Vector3 m_Position;
        public float m_RotationY;
        public int m_EnemyID;
        public EAIType m_AI;

        public Vector3 FixedBornPos { get; set; }
        public Quaternion BornRot => Quaternion.Euler(0, m_RotationY, 0);
    }

    [Serializable]
    public class PortalPoint
    {
        public bool m_Active = true;
        public Vector3 m_Position;
        public float m_RotationY;
        public int m_TargetSceneID;
        public int m_TargetPortalIndex;

        public Portal PortalObject { get; set; }
    }

    [Serializable]
    public class GodStatuePoint
    {
        public string m_Name;
        public Vector3 m_Position;
        public float m_RotationY;

        public GodStatue GodStatueObject { get; set; }
    }
}