using System;
using System.Collections;
using UnityEngine;
using Saber.CharacterController;

namespace Saber.AI
{
    public abstract class EnemyAIBase : BaseAI
    {
        public enum EFoundEnemyType
        {
            InFieldOfView,
            NotInStealth,
            Attacked,
        }

        private Coroutine m_MainCoroutine;
        private Collider[] m_Colliders = new Collider[10];
        protected Vector3 m_OriginPos;
        protected Vector3 m_DirToEnemy;
        protected float m_DistanceToEnemy = float.MaxValue;
        private SMonster m_Monster;

        public SMonster Monster => m_Monster ??= (SMonster)Actor;


        protected abstract void OnFoundEnemy(EFoundEnemyType foundType);

        public override void Init(SActor actor)
        {
            base.Init(actor);
            OnStart();
        }

        protected virtual void OnStart()
        {
            m_OriginPos = Actor.transform.position;
        }

        protected void SwitchCoroutine(IEnumerator routine)
        {
            //Debug.Log(routine.GetType().Name);
            if (m_MainCoroutine != null)
                Actor.StopCoroutine(m_MainCoroutine);
            m_MainCoroutine = Actor.StartCoroutine(routine);
        }

        protected bool TryLockEnemy(out EFoundEnemyType foundType)
        {
            if (Actor.CMelee.AttackedDamageInfo != null)
            {
                foundType = EFoundEnemyType.Attacked;
                LockingEnemy = Actor.CMelee.AttackedDamageInfo.Attacker;
                return true;
            }

            float range = Actor.m_BaseActorInfo.m_AIInfo.m_WarningRange;
            int layerMask = EStaticLayers.Actor.GetLayerMask();
            int count = Physics.OverlapSphereNonAlloc(Actor.transform.position, range, m_Colliders, layerMask);
            foundType = EFoundEnemyType.InFieldOfView;

            for (int i = 0; i < count; i++)
            {
                Collider tar = m_Colliders[i];
                var enemy = tar.GetComponent<SActor>();
                if (enemy != null && enemy != Actor && !enemy.IsDead && enemy.Camp != Actor.Camp)
                {
                    Vector3 dirToEnemy = enemy.transform.position - Actor.transform.position;
                    bool inFieldOfView = Vector3.Dot(dirToEnemy, Actor.transform.forward) > 0;
                    if (inFieldOfView)
                    {
                        foundType = EFoundEnemyType.InFieldOfView;
                        LockingEnemy = enemy;
                        return true;
                    }
                    else if (!enemy.IsInStealth)
                    {
                        foundType = EFoundEnemyType.NotInStealth;
                        LockingEnemy = enemy;
                        return true;
                    }
                }
            }

            return false;
        }

        protected void ToSearchEnemy()
        {
            SwitchCoroutine(SearchEnemy());
        }

        protected IEnumerator SearchEnemy()
        {
            Actor.StopMove();
            LockingEnemy = null;

            while (true)
            {
                //Debug.Log("SearchEnemy");

                if (TryLockEnemy(out var foundType))
                {
                    yield return null;
                    OnFoundEnemy(foundType);
                    yield break;
                }

                OnSearchEnemy();

                yield return new WaitForSeconds(0.3f);
            }
        }

        protected virtual void OnSearchEnemy()
        {
        }

        public override void Update()
        {
            base.Update();

            if (LockingEnemy)
            {
                m_DirToEnemy = base.LockingEnemy.transform.position - Actor.transform.position;
                m_DistanceToEnemy = m_DirToEnemy.magnitude;
                Actor.DesiredLookDir = Vector3.ProjectOnPlane(m_DirToEnemy, Vector3.up).normalized;
            }
        }

        /// <summary>
        /// 计算概率
        /// </summary>
        /// <param name="percent">百分之{percent}的概率（0-100）</param>
        /// <returns>是否发生</returns>
        protected bool CalcProbability(int percent)
        {
            return UnityEngine.Random.Range(0, 100) < percent;
        }

        protected override void OnDead(SActor owner)
        {
            LockingEnemy = null;
        }
    }
}