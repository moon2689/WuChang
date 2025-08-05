using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.CharacterController
{
    [Serializable]
    public class CharacterExpressionInfo
    {
        public bool m_Enable;
        public SkinnedMeshRenderer[] m_FaceSMRs;
        public int m_BlendShapeIndex_Blink;
        public int m_BlendShapeIndex_OpenMouth;
    }

    [Serializable]
    public enum EExpressionType
    {
        OpenMouth,
    }
}