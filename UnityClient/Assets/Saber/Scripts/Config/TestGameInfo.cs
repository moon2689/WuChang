using System.Collections;
using System.Collections.Generic;
using Saber.AI;
using UnityEngine;
using Saber.Frame;
using UnityEngine.Serialization;

namespace Saber.Config
{
    [CreateAssetMenu(menuName = "Saber/Test Game Info", fileName = "TestGame", order = 1)]
    public class TestGameInfo : ScriptableObject
    {
        public bool DebugFPS;
        [SerializeField] private bool m_DebugFight;
        public EAIType EnemyAI;
        public int[] TestingSkillID;


        public bool DebugFight
        {
            get
            {
#if UNITY_EDITOR
                return m_DebugFight;
#else
                return false;
#endif
            }
        }
    }
}