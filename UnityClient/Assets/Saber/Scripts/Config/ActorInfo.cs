using System;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;
using YooAsset;

namespace Saber.Config
{
    [CreateAssetMenu(menuName = "Saber/Actor Info", fileName = "ActorInfo", order = 1)]
    public class ActorInfo : ScriptableObject
    {
        public ActorItemInfo[] m_Actors;
    }

    [Serializable]
    public class ActorItemInfo
    {
        public bool m_IsActive = true;
        public int m_ID;
        public string m_Name;
        public EActorType m_ActorType;
        public string m_PrefabName;
        public string m_BossBattleMusic;

        public AssetHandle LoadIcon(Action<Texture2D> onLoaded)
        {
            return GameApp.Entry.Asset.LoadAsset<Texture2D>($"Actor/Icon/{m_PrefabName}", onLoaded);
        }

        public AssetHandle LoadGameObject(Action<GameObject> onLoaded)
        {
            string path = $"Actor/{m_ActorType}/{m_PrefabName}";
            return GameApp.Entry.Asset.LoadGameObject(path, onLoaded);
        }
    }

    [Serializable]
    public enum EActorType
    {
        Monster,
        Boss,
        NPC,
        Player,
    }
}