using System;
using System.Collections.Generic;


using UnityEngine;

namespace Saber.CharacterController
{
    public class HumanObstruct : ObstructStateBase
    {
        private HitFloating m_HitFloating;
        private HitRecovery m_HitRecovery;
        private ObstructParried m_ObstructParried;
        private ObstructDefenseBroken m_ObstructDefenseBroken;
        private ObstructTired m_ObstructTired;
        private ObstructStun m_ObstructStun;

        protected override ObstructBase GetCurrentHitObj()
        {
            bool toFloating = Damage.Attacker &&
                              Damage.DamageConfig != null &&
                              Actor.PhysicInfo.m_Mass < Damage.Attacker.PhysicInfo.m_Mass * 2f &&
                              ((m_CurrentObstruct is HitFloating hitFloating && hitFloating.IsRunning) ||
                               Damage.DamageConfig.m_DamageLevel == DamageLevel.HitToAir);

            if (toFloating)
            {
                return m_HitFloating ??= new HitFloating(Actor, Exit);
            }
            else if (Damage.ObstructType == EObstructType.Normal)
            {
                return m_HitRecovery ??= new HitRecovery(Actor, Exit);
            }
            else if (Damage.ObstructType == EObstructType.Parried)
            {
                return m_ObstructParried ??= new ObstructParried(Actor, Exit);
            }
            else if (Damage.ObstructType == EObstructType.DefenseBroken)
            {
                return m_ObstructDefenseBroken ??= new ObstructDefenseBroken(Actor, Exit);
            }
            else if (Damage.ObstructType == EObstructType.Tired)
            {
                return m_ObstructTired ??= new ObstructTired(Actor, Exit);
            }
            else if (Damage.ObstructType == EObstructType.Stun)
            {
                return m_ObstructStun ??= new ObstructStun(Actor, Exit);
            }
            else
            {
                throw new InvalidOperationException($"Unknown damage type:{Damage.ObstructType}");
            }
        }
    }
}