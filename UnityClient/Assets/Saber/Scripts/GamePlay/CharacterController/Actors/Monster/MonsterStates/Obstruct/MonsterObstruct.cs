using System;
using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterObstruct : ObstructStateBase
    {
        private SMonster m_Monster;
        private ObstructBase m_HitRec;
        private MonsterObstructStun m_Stun;
        private ObstructParried m_ObstructParried;

        SMonster Monster => m_Monster ??= (SMonster)Actor;

        protected override ObstructBase GetCurrentHitObj()
        {
            if (this.Damage.ObstructType == EObstructType.Stun)
            {
                return m_Stun ??= new MonsterObstructStun(Actor, Exit);
            }
            else if (Damage.ObstructType == EObstructType.Parried)
            {
                return m_ObstructParried ??= new ObstructParried(Actor, Exit);
            }
            else if (Damage.ObstructType == EObstructType.Normal)
            {
                if (m_HitRec == null)
                {
                    if (Monster.m_MonsterInfo.m_HitRecType == MonsterInfo.EHitRecType._2Side)
                    {
                        return m_HitRec = new MonsterHitRec2Side(Actor, Exit);
                    }
                    else if (Monster.m_MonsterInfo.m_HitRecType == MonsterInfo.EHitRecType._4Side)
                    {
                        return m_HitRec = new MonsterHitRec4Side(Actor, Exit);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Unknown hit rec type:{Monster.m_MonsterInfo.m_HitRecType}");
                    }
                }

                return m_HitRec;
            }
            else
            {
                Debug.LogError($"Unknown ObstructType:{Damage.ObstructType}");
                return null;
            }
        }
    }
}