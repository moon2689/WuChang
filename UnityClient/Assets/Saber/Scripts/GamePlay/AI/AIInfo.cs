using System;
using UnityEngine;

namespace Saber.AI
{
    [Serializable]
    public class AIInfo
    {
        public EAIType m_DefaultAI = EAIType.EnemyCommonAI;
        public float m_WarningRange = 10;
        public float m_LostFocusRange = 10;

        public Vector2 m_StalemateStayTime = new(0.3f, 3f);
        public Vector2 m_RandomMoveTime = new(1f, 3f);
        public Vector2 m_SprintTimeBeforeAttack = new(1f, 3f);

        public int m_DodgeDamagePercent = 50;
        public int m_DodgePercentAfterAttack = 50;
        public int m_DodgePercentAfterStalemate = 50;

        public bool m_TurnDirWhenNotFaceToEnemy;
        public int m_ContinueAttackPercentAfterAttack = 50;
        public EAIAttackStyleWhenTooFar m_AIAttackStyleWhenTooFar;

        public MonsterFightingEvent[] m_EventsBeforeFighting;
        public MonsterFightingEvent[] m_EventsOnBossStageToTwo;
    }

    [Flags]
    public enum EDodgeType
    {
        Back = 1,
        Left = 2,
        Right = 4,
        Front = 8,
    }

    public enum EAIAttackStyleWhenTooFar
    {
        ToStalemate,
        UseLongestRangeSkill,
        UseRandomSkill,
    }

    public enum EMonsterFightingEvent
    {
        HideWeapon,
        ShowWeapon,
    }

    [Serializable]
    public class MonsterFightingEvent
    {
        public EMonsterFightingEvent EventType;
        public float m_ParamFloat;
        public int m_ParamInt;
        public string m_ParamStrng;
    }
}