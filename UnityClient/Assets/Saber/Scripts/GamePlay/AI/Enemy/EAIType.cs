using System;

namespace Saber.AI
{
    [Serializable]
    public enum EAIType
    {
        None,
        OnlyIdle,
        OnlyDefense,
        MonsterAttack,
        BossAttack,
        Follow,
        RandomMove,
        SimpleAttack,
    }

    public static class EnemyAITypeHelper
    {
        public static EnemyAIBase CreateEnemyAI(this EAIType aiType)
        {
            return aiType switch
            {
                EAIType.None => null,
                EAIType.OnlyIdle => new OnlyIdle(),
                EAIType.OnlyDefense => new FightDefense(),
                EAIType.MonsterAttack => new MonsterAttack(),
                EAIType.BossAttack => new BossAttack(),
                EAIType.Follow => new Follow(),
                EAIType.RandomMove => new RandomMove(),
                EAIType.SimpleAttack => new SimpleAttack(),
                _ => throw new InvalidOperationException("Unknown ai:" + aiType),
            };
        }
    }
}