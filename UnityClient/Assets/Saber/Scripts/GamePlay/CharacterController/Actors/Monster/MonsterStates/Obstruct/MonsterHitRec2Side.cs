using System;
using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterHitRec2Side : ObstructBase
    {
        public static string GetAnimName(DamageInfo damageInfo, SActor actor)
        {
            float angleFromAttacker = Vector3.SignedAngle(damageInfo.Attacker.transform.forward,
                actor.transform.forward, Vector3.up);
            bool right = angleFromAttacker > 0 && angleFromAttacker <= 180;

            string animName;
            if (damageInfo.DamageConfig.m_DamageLevel == DamageLevel.Normal)
            {
                animName = "SmallHurt";
            }
            else
            {
                animName = "BigHurt";
            }

            if (right)
            {
                animName += "Right";
            }
            else
            {
                animName += "Left";
            }

            return animName;
        }

        public MonsterHitRec2Side(SActor actor, Action actionExit) : base(actor, actionExit)
        {
        }

        public override void Enter(DamageInfo damageInfo)
        {
            string animName = GetAnimName(damageInfo, base.Actor);
            Actor.CAnim.Play(animName, force: true, onFinished: Exit);

            // add force
            if (damageInfo.DamageConfig.m_ForceWhenGround.x > 0)
            {
                Vector3 directionToAttacker = damageInfo.Attacker.transform.position - Actor.transform.position;
                directionToAttacker.y = 0;
                Actor.CPhysic.Force_Add(-directionToAttacker, damageInfo.DamageConfig.m_ForceWhenGround.x, 0, false);
            }
        }

        public override void ReEnter(DamageInfo damageInfo)
        {
            Enter(damageInfo);
        }
    }
}