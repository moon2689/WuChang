using System;
using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterHitRec4Side : ObstructBase
    {
        public static string GetAnimName(DamageInfo damageInfo, SActor actor)
        {
            string animName = "Hit";
            if (damageInfo.DamageConfig.m_DamageLevel == DamageLevel.Normal)
            {
                float angleFromAttacker = Vector3.SignedAngle(damageInfo.Attacker.transform.forward,
                    actor.transform.forward, Vector3.up);

                if (angleFromAttacker > -45 && angleFromAttacker <= 45)
                    animName += "B";
                else if (angleFromAttacker > 45 && angleFromAttacker <= 135)
                    animName += "R";
                else if (angleFromAttacker > -135 && angleFromAttacker <= -45)
                    animName += "L";
                else
                    animName += "F";
            }
            else
            {
                animName += "Large";
            }

            return animName;
        }

        public MonsterHitRec4Side(SActor actor, Action actionExit) : base(actor, actionExit)
        {
        }

        public override void Enter(DamageInfo damageInfo)
        {
            string animName = GetAnimName(damageInfo, base.Actor);
            Actor.CAnim.Play(animName, force: true, onFinished: Exit);

            if (damageInfo.DamageConfig.m_DamageLevel != DamageLevel.Normal)
            {
                Vector3 directionToAttacker = damageInfo.Attacker.transform.position - Actor.transform.position;
                directionToAttacker.y = 0;
                Actor.transform.rotation = Quaternion.LookRotation(directionToAttacker);
            }

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