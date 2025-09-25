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
        Custom1,
        Custom2,

        Sword, //剑
        YueYaChan, //月牙铲
        MiaoDao, //苗刀
    }

    // 武器风格
    [Serializable]
    public enum EWeaponStyle
    {
        Unarmed = 0, //赤手空拳
        SingleHandSword,
        TwoHandMiaoDao,
        TwoHandAxe,
        ChangQiang,
        DoubleSwords,
    }
}