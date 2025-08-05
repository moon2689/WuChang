using System;
using System.Collections.Generic;
using CombatEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.CharacterController
{
    [CreateAssetMenu(menuName = "Saber/Skill decapitate config", fileName = "SkillDecapitate", order = 1)]
    public class SkillDecapitateConfig : ScriptableObject
    {
        public float DecapitateMaxDistance = 3;
        public float DecapitateMaxAngle = 30;
        public float m_DecapitateEnemyTime = 0.25f;
        public float m_DecapitateEnemyFinishTime = 0.5f;
        public AudioClip m_MagicCounterSuccessSound;
        public AudioClip m_MagicCounterHitSound;
        public AudioClip m_EquipExeInsertBodySound;
        public AudioClip m_EquipExeLeaveBodySound;
        public GameObject m_EquipExecution;
        public ENodeType m_EquipExecutionParentBone = ENodeType.LeftLowerArm;
        public Vector3 m_EquipExecutionLocalPosition;
        public Vector3 m_EquipExecutionLocalRotation;
        public float m_EquipExecutionDisappearTime = 0.9f;
        public float m_AlignDirectionEndTime = 0.4f;
        public GameObject m_BloodStartDecapitate;
        public float BloodPlayTime = 10f;
        public GameObject m_BloodFinishDecapitate;
        public GameObject m_EffectMagicCounterSuccess;
    }
}