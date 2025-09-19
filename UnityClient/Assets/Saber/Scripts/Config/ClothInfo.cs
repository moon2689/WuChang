using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Saber.Frame;
using UnityEngine;
using YooAsset;

namespace Saber.Config
{
    [CreateAssetMenu(menuName = "Saber/Cloth Info", fileName = "ClothInfo", order = 1)]
    public class ClothInfo : ScriptableObject
    {
        public ClothItemInfo[] m_Clothes;
        public ClothClassify[] m_Classfies;

        private Dictionary<int, ClothItemInfo> m_DicClothes;

        public void Init()
        {
            foreach (var c in m_Clothes)
            {
                c.Classify = m_Classfies.FirstOrDefault(a => a.m_ID == c.m_ClassifyID);
            }
        }

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
        public int m_ClassifyID;

        public ClothClassify Classify { get; set; }

        public string PrefabName => $"{Classify.m_ResName}_{m_ClothType}";

        public AssetHandle LoadGameObject(Action<GameObject> onLoaded)
        {
            string res = $"Actor/Player/Clothes/{Classify.m_ResName}/{PrefabName}";
            return GameApp.Entry.Asset.LoadGameObject(res, onLoaded);
        }

        public AssetHandle LoadIcon(Action<Texture2D> onLoaded)
        {
            string res = $"Actor/Player/Clothes/{Classify.m_ResName}/Icon_{PrefabName}";
            return GameApp.Entry.Asset.LoadAsset(res, onLoaded);
        }
    }


    [Serializable]
    public class ClothClassify
    {
        public int m_ID;
        public string m_ResName;
        public string m_Name;
    }

    [Serializable]
    public enum EClothType
    {
        Head,
        Hat,
        Jacket,
        Hands,
        Pants,
    }
}