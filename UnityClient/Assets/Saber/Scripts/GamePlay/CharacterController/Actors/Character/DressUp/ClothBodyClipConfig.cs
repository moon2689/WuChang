using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>解决身体穿模问题</summary>
    public class ClothBodyClipConfig : MonoBehaviour
    {
        [Serializable]
        public enum EClipArea
        {
            Body,
            Leg,
            Arm,
        }

        [Serializable]
        public enum EClipType
        {
            Hide,
            ShaderClip,
        }

        [Serializable]
        public class ClipItem
        {
            public EClipArea m_ClipArea;
            public EClipType m_ClipType;
            public Texture2D m_ClipMaskMap;
        }

        public ClipItem[] m_ClipItems;
    }
}