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
        public HitRecInfo m_HitRecInfo;
    }

    [Serializable]
    public class CharacterNode
    {
        public ENodeType m_Type;
        public Transform m_NodeTrans;
    }

    [Serializable]
    public class HitRecInfo
    {
        public bool CanBeBackstab = true;
        public bool CanBeUppercut = true;
        public bool CanBeStrikeDown = true;
        public bool CanBeKnockOffLongDis = true;
        public bool CanBeStunLarge = true;
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
        Back,
        WeaponLeftHand,
        WeaponRightHand,
    }
}