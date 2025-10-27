using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CombatEditor;
using Saber.Frame;
using TMPro;
using UnityEngine;

namespace Saber.CharacterController
{
    public class CharacterWeapon
    {
        private SActor m_Actor;
        private WeaponBase[] m_CurWeapons;
        private Dictionary<ENodeType, WeaponBase> m_DicWeapons;

        public WeaponBase[] CurWeapons => m_CurWeapons;


        public CharacterWeapon(SActor actor)
        {
            m_Actor = actor;
        }

        public async void CreateWeapons(WeaponPrefab[] prefabs)
        {
            if (m_CurWeapons != null)
            {
                foreach (var w in m_CurWeapons)
                {
                    GameObject.Destroy(w.gameObject);
                }
            }

            m_CurWeapons = new WeaponBase[prefabs.Length];
            m_DicWeapons = new();
            for (int i = 0; i < prefabs.Length; i++)
            {
                var prefabInfo = prefabs[i];
                if (prefabInfo.m_WeaponPrefabResPath.IsNotEmpty())
                {
                    await GameApp.Entry.Asset.LoadGameObject(prefabInfo.m_WeaponPrefabResPath, go => { m_CurWeapons[i] = go.GetComponent<WeaponBase>(); }).Task;
                }
                else
                {
                    Transform armTrans = m_Actor.GetNodeTransform(prefabInfo.m_ArmBoneType);
                    WeaponBase weaponBase = armTrans.GetComponentsInTopChildren<WeaponBase>().FirstOrDefault();
                    if (weaponBase)
                    {
                        m_CurWeapons[i] = weaponBase;
                    }
                    else
                    {
                        Debug.LogError($"Weapon error, weapon index:{i}");
                    }
                }

                m_CurWeapons[i].Init(m_Actor, prefabInfo);
                m_DicWeapons.Add(prefabInfo.m_ArmBoneType, m_CurWeapons[i]);
            }

            EquipWeapons();
        }

        private void EquipWeapons()
        {
            for (int i = 0; i < m_CurWeapons.Length; i++)
            {
                if (m_CurWeapons[i] != null)
                    m_CurWeapons[i].EquipWeapon();
            }
        }

        public WeaponBase GetWeaponByPos(ENodeType bone)
        {
            m_DicWeapons.TryGetValue(bone, out var w);
            if (w == null)
            {
                Debug.LogError($"Weapon is null,bone:{bone},actor:{m_Actor.name}", m_Actor);
            }

            return w;
        }

        public void ToggleDamage(WeaponDamageSetting damage, bool enable)
        {
            var w = GetWeaponByPos(damage.m_WeaponBone);
            if (w != null)
                w.ToggleDamage(damage, enable);
        }

        public void ShowWeaponTrail(ENodeType bone)
        {
            var w = GetWeaponByPos(bone);
            if (w != null)
                w.ShowTrail();
        }

        public void HideWeaponTrail(ENodeType bone)
        {
            var w = GetWeaponByPos(bone);
            if (w != null)
                w.HideTrail();
        }

        public void ShowOrHideWeapon(bool show)
        {
            for (int i = 0; i < m_CurWeapons.Length; i++)
            {
                var w = m_CurWeapons[i];
                if (w != null)
                    w.ShowOrHideWeapon(show);
            }

            if (show)
            {
                EquipWeapons();
            }
        }

        public void WeaponFallToGround()
        {
            foreach (var w in CurWeapons)
            {
                w.FallToGround();
            }
        }
    }
}