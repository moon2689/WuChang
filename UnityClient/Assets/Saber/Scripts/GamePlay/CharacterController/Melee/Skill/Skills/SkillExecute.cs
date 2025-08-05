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
        public enum EExecuteType
        {
            Decapitate, //处决
            BackStab, //背刺
        }

        private string m_Anim;
        private bool m_AlignDirection;
        private bool m_DecapitateEnemy;
        private bool m_DecapitateEnemyFinshed;
        private GameObject m_EquipExecutionObj;
        private List<Material> m_DissolveMaterials;
        private Coroutine m_CoroutineDissolve;
        private bool m_EquipExecutionShowing;


        SkillDecapitateConfig Config => GameApp.Entry.Config.SkillDecapitateConfig;

        public SActor Target { get; set; }
        public EExecuteType ExecuteType { get; set; }
        public override bool IsQuiet => true;


        public SkillExecute(SActor actor, SkillItem skillConfig) : base(actor, skillConfig)
        {
        }

        /// <summary>是否可以处决，包括斩首和背刺</summary>
        public static bool CanExecute(SActor owner, SActor enemy)
        {
            return enemy.CanBeDecapitate && CanBeDecapitate(owner, enemy) || CanBackStab(owner, enemy);
        }

        /// <summary>是否可以斩首</summary>
        static bool CanBeDecapitate(SActor owner, SActor enemy)
        {
            if (enemy == null || enemy == owner || enemy.IsDead || enemy.Camp == owner.Camp ||
                !enemy.CanBeDecapitate)
            {
                return false;
            }

            // 一定距离内可处决
            float maxDis = GameApp.Entry.Config.SkillDecapitateConfig.DecapitateMaxDistance;
            Vector3 dirToEnemy = enemy.transform.position - owner.transform.position;
            if (dirToEnemy.magnitude > maxDis)
            {
                return false;
            }

            // 面对面才可处决
            if (Vector3.Dot(enemy.transform.forward, dirToEnemy) >= 0)
            {
                return false;
            }

            if (Vector3.Dot(enemy.transform.forward, owner.transform.forward) >= 0)
            {
                return false;
            }

            // 一定角度内才可处决
            float minAngle = 180 - GameApp.Entry.Config.SkillDecapitateConfig.DecapitateMaxAngle;
            float angle = Vector3.Angle(enemy.transform.forward, dirToEnemy);
            if (angle < minAngle)
            {
                return false;
            }

            return true;
        }

        /// <summary>获取可以背刺的目标</summary>
        public SActor GetCanBackStabTarget()
        {
            if (CanBackStab(Actor, Actor.AI.LockingEnemy))
            {
                return Actor.AI.LockingEnemy;
            }

            Collider[] colliders = new Collider[10];
            float radius = GameApp.Entry.Config.SkillDecapitateConfig.DecapitateMaxDistance;
            int layerMask = EStaticLayers.Actor.GetLayerMask();
            int count = Physics.OverlapSphereNonAlloc(Actor.transform.position, radius, colliders, layerMask);
            for (int i = 0; i < count; i++)
            {
                var enemy = colliders[i].GetComponent<SActor>();
                if (CanBackStab(Actor, enemy))
                {
                    return enemy;
                }
            }

            return null;
        }

        /// <summary>是否可以背刺</summary>
        static bool CanBackStab(SActor owner, SActor enemy)
        {
            if (enemy == null || enemy == owner || enemy.IsDead || enemy.Camp == owner.Camp)
            {
                return false;
            }

            if (!owner.IsInStealth)
            {
                return false;
            }

            if (enemy.AI.LockingEnemy != null)
            {
                return false;
            }

            // 一定距离内可背刺
            float maxDis = GameApp.Entry.Config.SkillDecapitateConfig.DecapitateMaxDistance;
            Vector3 dirToEnemy = enemy.transform.position - owner.transform.position;
            if (dirToEnemy.magnitude > maxDis)
            {
                return false;
            }

            // 背对时才可背刺
            if (Vector3.Dot(enemy.transform.forward, dirToEnemy) <= 0)
            {
                return false;
            }

            if (Vector3.Dot(enemy.transform.forward, owner.transform.forward) <= 0)
            {
                return false;
            }

            // 一定角度内才可背刺
            float maxAngle = GameApp.Entry.Config.SkillDecapitateConfig.DecapitateMaxAngle;
            float angle = Vector3.Angle(enemy.transform.forward, dirToEnemy);
            if (angle > maxAngle)
            {
                return false;
            }

            return true;
        }

        /// <summary>获取可以斩首的对象</summary>
        public SActor GetDecapitateTarget()
        {
            if (CanBeDecapitate(Actor, Actor.AI.LockingEnemy))
            {
                return Actor.AI.LockingEnemy;
            }

            Collider[] colliders = new Collider[10];
            float radius = GameApp.Entry.Config.SkillDecapitateConfig.DecapitateMaxDistance;
            int layerMask = EStaticLayers.Actor.GetLayerMask();
            int count = Physics.OverlapSphereNonAlloc(Actor.transform.position, radius, colliders, layerMask);
            for (int i = 0; i < count; i++)
            {
                var enemy = colliders[i].GetComponent<SActor>();
                if (CanBeDecapitate(Actor, enemy))
                {
                    return enemy;
                }
            }

            return null;
        }

        public override void Enter()
        {
            base.Enter();
            m_Anim = Target.CPhysic.Height > Actor.CPhysic.Height * 1.5f ? "Decapitate" : "Decapitate_low";
            base.PlayAnimOnEnter(m_Anim, m_Anim);

            // 播放处决音效
            GameApp.Entry.Game.Audio.Play3DSound(Config.m_MagicCounterSuccessSound, Actor.transform.position);

            // 显示处决武器
            ShowEquipExecution();

            m_DecapitateEnemy = false;
            m_DecapitateEnemyFinshed = false;
            m_AlignDirection = true;
        }

        public override void OnStay()
        {
            base.OnStay();

            float curTime = Actor.CAnim.GetAnimNormalizedTime(m_Anim);

            // 敌人播放处决动画
            if (!m_DecapitateEnemy)
            {
                if (curTime >= Config.m_DecapitateEnemyTime)
                {
                    m_DecapitateEnemy = true;
                    StartDecapitate();
                }
            }

            // 敌人处决结束
            if (!m_DecapitateEnemyFinshed)
            {
                if (curTime >= Config.m_DecapitateEnemyFinishTime)
                {
                    m_DecapitateEnemyFinshed = true;
                    EndDecapitate();
                }
            }

            // 对准敌人方向
            if (m_AlignDirection)
            {
                AlignDirection();

                if (curTime >= Config.m_AlignDirectionEndTime)
                {
                    m_AlignDirection = false;
                }
            }

            // 溶解处决武器
            if (m_EquipExecutionShowing)
            {
                if (curTime >= Config.m_EquipExecutionDisappearTime)
                {
                    m_EquipExecutionShowing = false;
                    HideEquipExecution();
                }
            }
        }

        /// <summary>开始处决，敌人播放处决动画，流血，声音，扣血</summary>
        void StartDecapitate()
        {
            Target.BeExecute(ExecuteType);

            ShowBlood(Config.m_BloodStartDecapitate);
            GameApp.Entry.Game.Audio.Play3DSound(Config.m_EquipExeInsertBodySound, Actor.transform.position);
            Target.CStats.TakeDamage(50);
        }

        /// <summary>处决结束，敌人被推远，流血，声音</summary>
        void EndDecapitate()
        {
            Vector3 dir = Target.transform.position - Actor.transform.position;
            Target.CPhysic.Force_Add(dir, 10, 0, false);

            ShowBlood(Config.m_BloodFinishDecapitate);
            GameApp.Entry.Game.Audio.Play3DSound(Config.m_EquipExeLeaveBodySound, Actor.transform.position);
        }

        void ShowBlood(GameObject bloodPrefab)
        {
            Vector3 pos = Target.transform.position;
            pos.y = m_EquipExecutionObj.transform.position.y;
            Quaternion rot = Quaternion.LookRotation(Actor.transform.position - Target.transform.position);
            GameApp.Entry.Game.Effect.CreateEffect(bloodPrefab, null, pos, rot, Config.BloodPlayTime);
        }

        /// <summary>对齐方向</summary>
        void AlignDirection()
        {
            Vector3 dir = Target.transform.position - base.Actor.transform.position;
            Actor.CPhysic.AlignForwardTo(dir, 1080f);

            Vector3 targetForward = ExecuteType switch
            {
                EExecuteType.Decapitate => -dir,
                EExecuteType.BackStab => dir,
                _ => throw new InvalidOperationException($"Unknown execute type:{ExecuteType}"),
            };
            Target.CPhysic.AlignForwardTo(targetForward, 1080f);
        }

        public override void Exit()
        {
            base.Exit();
            if (m_EquipExecutionShowing)
            {
                HideEquipExecution();
            }
        }

        #region EquipExecution

        void ShowEquipExecution()
        {
            m_EquipExecutionShowing = true;

            if (!m_EquipExecutionObj)
            {
                Transform parent = Actor.GetNodeTransform(Config.m_EquipExecutionParentBone);
                m_EquipExecutionObj = GameObject.Instantiate(Config.m_EquipExecution);
                m_EquipExecutionObj.transform.SetParent(parent);
            }

            m_EquipExecutionObj.transform.localPosition = Config.m_EquipExecutionLocalPosition;
            m_EquipExecutionObj.transform.localRotation = Quaternion.Euler(Config.m_EquipExecutionLocalRotation);
            m_EquipExecutionObj.SetActive(true);

            // 溶解
            if (m_DissolveMaterials == null)
                m_DissolveMaterials = GetDissolveMaterials();
            foreach (var m in m_DissolveMaterials)
                m.DisableKeyword("_DISSOLVE_ON");
            if (m_CoroutineDissolve != null)
            {
                m_CoroutineDissolve.StopCoroutine();
                m_CoroutineDissolve = null;
            }
        }

        List<Material> GetDissolveMaterials()
        {
            List<Material> list = new();
            Renderer[] renderers = m_EquipExecutionObj.transform.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                foreach (var m in r.materials)
                {
                    if (m.HasProperty("_Dissolve"))
                        list.Add(m);
                }
            }

            return list;
        }

        void HideEquipExecution()
        {
            m_EquipExecutionShowing = false;
            if (m_CoroutineDissolve != null)
            {
                m_CoroutineDissolve.StopCoroutine();
            }

            m_CoroutineDissolve = DissolveItor().StartCoroutine();
        }

        // 溶解
        IEnumerator DissolveItor()
        {
            float weight = 0;
            while (true)
            {
                weight += Time.deltaTime * 1.2f;

                foreach (var m in m_DissolveMaterials)
                {
                    m.EnableKeyword("_DISSOLVE_ON");
                    m.SetFloat("_Dissolve", 1);
                    m.SetFloat("_DissolveWeight", weight);
                }

                if (weight >= 1)
                {
                    m_EquipExecutionObj.SetActive(false);
                    break;
                }

                yield return null;
            }

            m_CoroutineDissolve = null;
        }

        #endregion
    }
}