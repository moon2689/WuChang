using System;
using System.Collections.Generic;
using System.Linq;
using CombatEditor;
using Saber.Frame;
using UnityEngine;
using System.Collections;

namespace Saber.CharacterController
{
    /// <summary>处决</summary>
    public class SkillExecute : BaseSkill
    {
        private string m_Anim;
        private int m_CurDmgIndex;
        private bool m_IsFromBack;


        SkillCommonConfig Config => GameApp.Entry.Config.SkillCommon;

        public SActor Target { get; set; }
        public override bool IsQuiet => true;


        public SkillExecute(SActor actor, SkillItem skillConfig) : base(actor, skillConfig)
        {
        }

        /// <summary>是否可以斩首</summary>
        private static bool CanDoExecute(SActor owner, SActor enemy)
        {
            if (enemy == null || enemy == owner || enemy.IsDead || enemy.Camp == owner.Camp || !enemy.CanBeExecuted)
            {
                return false;
            }

            // 一定距离内可处决
            float maxDis = GameApp.Entry.Config.SkillCommon.ExecuteMaxDistance;
            Vector3 dirToEnemy = enemy.transform.position - owner.transform.position;
            float curDis = dirToEnemy.magnitude - owner.CPhysic.Radius - enemy.CPhysic.Radius;
            if (curDis > maxDis)
            {
                return false;
            }

            /*
            bool isFromBack = Vector3.Dot(enemy.transform.forward, dirToEnemy) > 0;
            if (isFromBack)
            {
                // 面对背
                if (Vector3.Dot(enemy.transform.forward, owner.transform.forward) <= 0)
                {
                    return false;
                }

                // 一定角度内才可背刺
                float maxAngle = GameApp.Entry.Config.SkillCommon.ExecuteMaxAngle;
                float angle = Vector3.Angle(enemy.transform.forward, dirToEnemy);
                if (angle > maxAngle)
                {
                    return false;
                }
            }
            else
            {
                // 面对面
                if (Vector3.Dot(enemy.transform.forward, owner.transform.forward) >= 0)
                {
                    return false;
                }

                // 一定角度内才可处决
                float minAngle = 180 - GameApp.Entry.Config.SkillCommon.ExecuteMaxAngle;
                float angle = Vector3.Angle(enemy.transform.forward, dirToEnemy);
                if (angle < minAngle)
                {
                    return false;
                }
            }
            */

            return true;
        }

        /// <summary>获取可以斩首的对象</summary>
        public SActor GetCanBeExecutedEnemy()
        {
            if (CanDoExecute(Actor, Actor.AI.LockingEnemy))
            {
                return Actor.AI.LockingEnemy;
            }

            Collider[] colliders = new Collider[10];
            float radius = GameApp.Entry.Config.SkillCommon.ExecuteMaxDistance;
            int layerMask = EStaticLayers.Actor.GetLayerMask();
            int count = Physics.OverlapSphereNonAlloc(Actor.transform.position, radius, colliders, layerMask);
            for (int i = 0; i < count; i++)
            {
                var enemy = colliders[i].GetComponent<SActor>();
                if (CanDoExecute(Actor, enemy))
                {
                    return enemy;
                }
            }

            return null;
        }

        public override void Enter()
        {
            base.Enter();
            m_Anim = "ExecuteForward";
            base.PlayAnimOnEnter(m_Anim, m_Anim);

            Target.BeExecute(Actor);

            foreach (var dmg in Config.ExecuteDamages)
            {
                dmg.IsDmgDone = false;
            }

            m_CurDmgIndex = 0;

            Vector3 dirToEnemy = Target.transform.position - Actor.transform.position;
            m_IsFromBack = Vector3.Dot(Target.transform.forward, dirToEnemy) > 0;
            
            GameApp.Entry.Game.Audio.Play3DSound(Config.ExecuteStartSound, Actor.transform.position);
        }

        public override void OnStay()
        {
            base.OnStay();

            if (m_CurDmgIndex < Config.ExecuteDamages.Length)
            {
                var dmgItem = Config.ExecuteDamages[m_CurDmgIndex];
                Vector3 dirToEnemy = Target.transform.position - base.Actor.transform.position;

                if (dmgItem.IsDmgDone)
                {
                    ++m_CurDmgIndex;
                }
                else
                {
                    float curTime = Actor.CAnim.GetAnimNormalizedTime(m_Anim);
                    if (curTime >= dmgItem.m_DamageTime)
                    {
                        dmgItem.IsDmgDone = true;
                        Target.CStats.TakeDamage(dmgItem.m_Damage);

                        GameApp.Entry.Game.Audio.Play3DSound(dmgItem.m_Sound, Actor.transform.position);

                        Vector3 pos = Target.transform.position;
                        pos.y = Actor.CMelee.CWeapon.CurWeapons[0].transform.position.y;
                        Quaternion rot = Quaternion.LookRotation(-dirToEnemy);
                        GameApp.Entry.Game.Effect.CreateEffect(dmgItem.m_Blood, null, pos, rot, 10);

                        Actor.CStats.StaminaRecSpeed = ActorBaseStats.EStaminaRecSpeed.Fast;

                        /*
                        if (m_CurDmgIndex == Config.ExecuteDamages.Length - 1)
                        {
                            Target.CPhysic.Force_Add(dirToEnemy, Config.ExecuteLastHitForce, 1, true);
                        }
                        */
                    }
                }

                // 对准方向
                Actor.CPhysic.AlignForwardTo(dirToEnemy, 1080f);

                if (Config.ExecuteDamages[0].IsDmgDone)
                {
                    Target.CPhysic.AlignForwardTo(m_IsFromBack ? dirToEnemy : -dirToEnemy, 1080f);
                }
            }
            else if (!base.CanExit)
            {
                float curTime = Actor.CAnim.GetAnimNormalizedTime(m_Anim);
                if (curTime >= Config.ExecuteSkillCanExitTime)
                {
                    base.CanExit = true;
                }
            }
        }
    }
}