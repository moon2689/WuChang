using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.CharacterController
{
    [Serializable]
    public class WeaponItem
    {
        public int m_SkillID;
        public WeaponPrefab[] m_WeaponPrefabs;
    }

    [Serializable]
    public class WeaponPrefab
    {
        /// <summary>武器prefab</summary>
        public string m_WeaponPrefabResPath;
        public ENodeType m_ArmBoneType;
    }

    // 武器类型
    [Serializable]
    public enum EWeaponType
    {
        None = 0,
        Boxing, //拳脚
        Claw, //爪

        Sword, //剑
        YueYaChan, //月牙铲
        MiaoDao, //苗刀

        // GreatSword, //大剑
        // Axe, //斧
        // Mace, //狼牙棒
        // Dagger, //匕首
        // Shield, //盾
        // Nodachi, //野太刀
        // GirlGreatSword, //女战士大剑
        // LongSword,
        // Scythe,
    }

    // 武器风格
    [Serializable]
    public enum EWeaponStyle
    {
        Unarmed = 0, //赤手空拳
        SingleHandSword,

        // LongSword, //长剑
        // TwinSwords, //双剑
        // TwoHandedGreatSword, //双手大剑
        // SwordShield, //剑盾
        // AxeShield, //斧盾
        // MaceShield, //锤盾
        // TwinDaggers, //双匕首
        // Nodachi, //野太刀
        // GirlGreatSword, //女战士大剑
        // OneHandAxe,
        // Hammer,
        // OneHandSword,
        // Scythe, //镰刀
    }
}