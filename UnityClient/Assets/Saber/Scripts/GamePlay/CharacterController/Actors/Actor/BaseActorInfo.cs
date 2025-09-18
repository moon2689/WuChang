using System;
using System.Collections;
using System.Collections.Generic;
using CombatEditor;
using UnityEngine;
using Saber.AI;
using UnityEngine.Serialization;

namespace Saber.CharacterController
{
    [Serializable]
    public class BaseActorInfo
    {
        public StatsInfo m_StatsInfo;
        public PhysicInfo m_PhysicInfo;
        public SkillConfig m_SkillConfig;
        public WeaponPrefab[] m_WeaponPrefabs;
        public AIInfo m_AIInfo;
        public List<CharacterNode> m_Nodes;

        private Dictionary<ENodeType, Transform> m_DicNodes;

        public Transform GetNode(ENodeType nodeType)
        {
            if (m_DicNodes == null)
            {
                m_DicNodes = new();
                foreach (var n in m_Nodes)
                {
                    m_DicNodes.Add(n.m_Type, n.m_NodeTrans);
                }
            }

            m_DicNodes.TryGetValue(nodeType, out var t);
            if (t == null)
            {
                Debug.LogError($"Node is null,type:{nodeType}");
            }
            return t;
        }
    }

    [Serializable]
    public class CharacterNode
    {
        public ENodeType m_Type;
        public Transform m_NodeTrans;
    }

    public enum ENodeType
    {
        Animator,
        LockUIPos,
        RootBone,

        Hips,
        Spine,
        Chest,
        UpperChest,
        Neck,
        Head,

        LeftUpperLeg,
        RightUpperLeg,
        LeftLowerLeg,
        RightLowerLeg,
        LeftFoot,
        RightFoot,
        LeftToes,
        RightToes,

        LeftShoulder,
        RightShoulder,
        LeftUpperArm,
        RightUpperArm,
        LeftLowerArm,
        RightLowerArm,
        LeftHand,
        RightHand,

        Tail,
        YuMao,
    }
}