using System;

using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>破防</summary>
    public class ObstructDefenseBroken : ObstructBase
    {
        public ObstructDefenseBroken(SActor actor, Action actionExit) : base(actor, actionExit)
        {
        }

        public override void Enter(DamageInfo damageInfo)
        {
            Vector3 directionToAttacker = damageInfo.Attacker.transform.position - Actor.transform.position;
            directionToAttacker.y = 0;

            Actor.transform.rotation = Quaternion.LookRotation(directionToAttacker); //face to attacher

            if (damageInfo.DamageConfig.m_ForceWhenGround.x > 0)
            {
                Actor.CPhysic.Force_Add(-directionToAttacker, damageInfo.DamageConfig.m_ForceWhenGround.x, 0, false);
            }

            Actor.CAnim.Play("DefenseBroken", force: true, onFinished: Exit);
        }

        public override void ReEnter(DamageInfo damageInfo)
        {
        }
    }
}