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

        public WeaponBase this[int index] => m_CurWeapons[index];
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
            for (int i = 0; i < prefabs.Length; i++)
            {
                var prefabInfo = prefabs[i];
                if (prefabInfo.m_WeaponPrefabResPath.IsNotEmpty())
                {
                    await GameApp.Entry.Asset.LoadGameObject(prefabInfo.m_WeaponPrefabResPath, go =>
                    {
                        m_CurWeapons[i] = go.GetComponent<WeaponBase>();
                        prefabInfo.m_WeaponObj = m_CurWeapons[i];
                        m_CurWeapons[i].ResetLocation = true;
                    }).Task;
                }
                else if (prefabInfo.m_WeaponObj)
                {
                    m_CurWeapons[i] = prefabInfo.m_WeaponObj.GetComponent<WeaponBase>();
                    m_CurWeapons[i].ResetLocation = false;
                }
                else
                {
                    Transform armTrans = prefabInfo.m_WeaponParentInfo.m_ArmBone;
                    WeaponBase weaponBase = armTrans.GetComponentInChildren<WeaponBase>();
                    if (weaponBase)
                    {
                        m_CurWeapons[i] = weaponBase;
                        m_CurWeapons[i].ResetLocation = false;
                        prefabInfo.m_WeaponObj = weaponBase;
                    }
                    else
                    {
                        Debug.LogError($"Weapon error, weapon index:{i}");
                    }
                }

                m_CurWeapons[i].Init(m_Actor, prefabInfo.m_WeaponParentInfo);
            }

            ResetParent();
        }

        private void ResetParent()
        {
            for (int i = 0; i < m_CurWeapons.Length; i++)
            {
                if (m_CurWeapons[i] != null)
                    m_CurWeapons[i].ResetParent();
            }
        }

        public WeaponBase GetWeaponByPos(ENodeType bone)
        {
            return Array.Find(m_CurWeapons, w => w.WeaponBone == bone);
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
                if (m_CurWeapons[i] != null)
                    m_CurWeapons[i].gameObject.SetActive(show);
            }
        }
    }
}