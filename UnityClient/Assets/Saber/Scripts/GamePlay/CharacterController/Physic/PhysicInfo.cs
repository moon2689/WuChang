using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.CharacterController
{
    [Serializable]
    public class PhysicInfo
    {
        public float m_Mass = 60;
        public float m_Height = 1.72f;
        public float m_HipHeight = 0.1f;
        public float m_Radius = 0.3f;
        public float m_CapsuleOffsetZ;
        public float m_GroundOffset;

        [Tooltip("是否是直立行走")] public bool m_IsWalkingUpright = true;
        [Tooltip("是否体型巨大")] public bool m_IsBodyHuge;

        public EPhysicMaterialType m_PhysicMaterialType;
        public float m_ChestHeight = 1;

        public ETurnRotationSpeed m_TurnRotSpeed;

        public float m_JumpForceVertical = 10f;
        public bool m_OpenSlopeMovement;
        public bool m_OpenPlatformMovement;
        public bool m_CanClimb;
        

        public float TurnRotSpeedRate
        {
            get
            {
                return m_TurnRotSpeed switch
                {
                    ETurnRotationSpeed.Fast => 1,
                    ETurnRotationSpeed.Medium => 0.75f,
                    ETurnRotationSpeed.Slow => 0.5f,
                    _ => throw new InvalidOperationException(),
                };
            }
        }
    }

    [Serializable]
    public enum ETurnRotationSpeed
    {
        Fast,
        Medium,
        Slow,
    }
    
    [Serializable]
    public enum EPhysicMaterialType
    {
        BodyFlesh,
        BodyStone,
    }
}