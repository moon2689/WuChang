using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.CharacterController
{
    [Serializable]
    public class CharacterInfo
    {
        public CharacterDressUp.CharacterClothInfo m_ClothInfo;
         //public CharacterSpeech.SpeechInfo m_SpeechInfo;
        public CharacterIK.IKInfo m_IKInfo;
        public CharacterExpressionInfo m_ExpressionInfo;
        
        public float m_SpeedWalk;
        public float m_SpeedRun;
        public float m_SpeedSprint;
        public float m_SpeedDefenseWalk;
    }
}