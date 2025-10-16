using System;

namespace Saber.AI
{
    [Serializable]
    public enum EAIType
    {
        None,
        OnlyIdle,
        Stalemate,
        TestSkill,
        EnemyCommonAI,
    }

    [Serializable]
    public enum EAITriggerSkillCondition
    {
        None,
        OnEnterBossStageTwo,
        BossStageTwo,
    }

    public static class EnemyAITypeHelper
    {
        public static EnemyAIBase CreateEnemyAI(this EAIType aiType)
        {
            return aiType switch
            {
                EAIType.None => null,
                //EAIType.OnlyIdle => new OnlyIdle(),
                //EAIType.Stalemate => new Stalemate(),
                EAIType.TestSkill => new TestSkill(),
                // EAIType.OnlyDefense => new FightDefense(),
                // EAIType.MonsterAttack => new MonsterAttack(),
                EAIType.EnemyCommonAI => new EnemyCommonAI(),
                // EAIType.Follow => new Follow(),
                // EAIType.RandomMove => new RandomMove(),
                // EAIType.SimpleAttack => new SimpleAttack(),
                _ => throw new InvalidOperationException("Unknown ai:" + aiType),
            };
        }
    }
}