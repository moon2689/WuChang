using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Saber.Frame;
using UnityEngine.Serialization;

namespace Saber.Config
{
    [CreateAssetMenu(menuName = "Saber/Test Game Info", fileName = "TestGame", order = 1)]
    public class TestGameInfo : ScriptableObject
    {
        public bool DebugFPS;
        public bool DebugFight;

        public int TestingSkillID;
        public int TriggerSkillInterval = 6;
    }
}