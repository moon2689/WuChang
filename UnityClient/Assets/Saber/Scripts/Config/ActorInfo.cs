using System;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;

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

        public Texture2D LoadIcon()
        {
            return GameApp.Entry.Asset.LoadTexture($"Actor/Icon/{m_PrefabName}");
        }

        public GameObject LoadGameObject()
        {
            string path = $"Actor/{m_ActorType}/{m_PrefabName}";
            return GameApp.Entry.Asset.LoadGameObject(path);
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