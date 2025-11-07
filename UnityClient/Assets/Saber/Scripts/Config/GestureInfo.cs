using System;
using Saber.Frame;
using UnityEngine;
using YooAsset;

namespace Saber.Config
{
    [CreateAssetMenu(menuName = "Saber/Gesture Info", fileName = "GestureInfo", order = 1)]
    public class GestureInfo : ScriptableObject
    {
        public GestureItemInfo[] m_Gestures;
    }

    [Serializable]
    public class GestureItemInfo
    {
        public int m_ID;
        public string m_Name;
        public string m_Icon;
        public string m_Anim;
    }
}