using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Saber.Frame;
using UnityEngine;

namespace Saber.Config
{
    [CreateAssetMenu(menuName = "Saber/Cloth Info", fileName = "ClothInfo", order = 1)]
    public class ClothInfo : ScriptableObject
    {
        public ClothItemInfo[] m_Clothes;

        private Dictionary<int, ClothItemInfo> m_DicClothes;

        public ClothItemInfo GetClothByID(int id)
        {
            if (m_DicClothes == null)
            {
                m_DicClothes = new();
                foreach (var c in m_Clothes)
                {
                    m_DicClothes.Add(c.m_ID, c);
                }
            }

            m_DicClothes.TryGetValue(id, out var v);
            return v;
        }

        public List<int> GetAllClothesID()
        {
            return m_Clothes.Select(a => a.m_ID).ToList();
        }
    }

    [Serializable]
    public class ClothItemInfo
    {
        public int m_ID;
        public bool m_IsActive = true;
        public string m_Name;
        public EClothType m_ClothType;
        public string m_PrefabName;
        public float m_ShoesHeight;

        public GameObject LoadGameObject()
        {
            string res = $"Cloth/Prefab/{m_PrefabName}";
            return GameApp.Entry.Asset.LoadGameObject(res);
        }

        public Texture2D LoadIcon()
        {
            string res = $"Cloth/Icon/{m_PrefabName}";
            return GameApp.Entry.Asset.LoadTexture(res);
        }
    }

    [Serializable]
    public enum EClothType
    {
        Hair,
        FullNoHair,
        TopDown,
        Shoes,
        Full,
        FullNoShoes,
        Chain,
        Earrings,
    }
}