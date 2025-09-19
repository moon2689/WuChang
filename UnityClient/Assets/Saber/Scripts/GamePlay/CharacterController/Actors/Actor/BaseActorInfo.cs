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
        BackSocket,
        WeaponLeftHand,
        WeaponRightHand,
    }
}