using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Defense : ActorStateBase, ISkillCanTrigger
    {
        protected enum EState
        {
            None,
            DefenseStart,
            DefenseLoop,
            DefenseEnd,
            DefenseHit,
            TanDao,
        }

        private float m_TimerAlign;
        protected EState m_CurState;
        private SCharacter m_Character;
        private float m_TimerCanTanFan;
        private List<SActor> m_ParriedEnemies = new();
        private bool m_AutoExitOnAnimFinished;
        private bool m_CanExit;
        private bool m_CanTriggerSkill;


        public override bool ApplyRootMotionSetWhenEnter => true;
        public override bool CanEnter => Actor.CPhysic.Grounded;
        public override bool CanExit => m_CanExit;
        public SCharacter Character => m_Character ??= (SCharacter)Actor;


        public Defense() : base(EStateType.Defense)
        {
        }

        public bool CanDefense(SActor enemy)
        {
            if (m_CurState == EState.DefenseEnd)
            {
                return false;
            }

            bool isFaceToFace = Vector3.Dot(Actor.transform.forward, enemy.transform.forward) < 0;
            if (!isFaceToFace)
            {
                return false;
            }

            return true;
        }

        public override void Enter()
        {
            base.Enter();

            Actor.CAnim.StopMaskLayerAnims();

            Actor.UpdateMovementAxisAnimatorParams = false;

            OnEnter();
        }

        public override void ReEnter()
        {
            base.ReEnter();
            OnEnter();
        }

        void OnEnter()
        {
            m_CanTriggerSkill = false;

            if (m_CurState == EState.None)
            {
                Actor.CAnim.Play("DefenseStart");
                m_CurState = EState.DefenseStart;
                m_TimerAlign = 0.1f;
                m_TimerCanTanFan = GameApp.Entry.Config.SkillCommon.CanTanFanSecondsFromDefenseStart;
                m_CanExit = false;
            }
            else if (m_CurState == EState.DefenseStart)
            {
            }
            else if (m_CurState == EState.DefenseLoop)
            {
            }
            else if (m_CurState == EState.DefenseEnd)
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay("DefenseEnd", 0.25f))
                {
                    m_CurState = EState.DefenseLoop;
                    Actor.CAnim.Play("DouDao");
                    m_TimerCanTanFan = GameApp.Entry.Config.SkillCommon.CanTanFanSecondsFromDefenseStart;
                    m_CanExit = false;
                }
            }
            else if (m_CurState == EState.DefenseHit)
            {
            }
            else if (m_CurState == EState.TanDao)
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay("TanFan", 0.3f))
                    m_TimerCanTanFan = GameApp.Entry.Config.SkillCommon.CanTanFanSecondsFromDefenseStart;
            }
            else
            {
                Debug.LogError($"Unknown state:{m_CurState}");
            }
        }

        /// <summary>尝试弹反</summary>
        bool TryParry(out List<SActor> parriedEnemies)
        {
            m_ParriedEnemies.Clear();
            Collider[] colliders = new Collider[10];
            float radius = 10f;
            int count = Physics.OverlapSphereNonAlloc(Actor.transform.position, radius, colliders,
                EStaticLayers.Actor.GetLayerMask());
            for (int i = 0; i < count; i++)
            {
                Collider tar = colliders[i];
                var enemy = tar.GetComponent<SActor>();
                if (enemy == null || enemy == Actor || enemy.IsDead || enemy.Camp == Actor.Camp)
                    continue;

                Vector3 dirToEnemy = enemy.transform.position - Actor.transform.position;
                if (Vector3.Dot(dirToEnemy, Actor.transform.forward) <= 0)
                    continue;

                bool parriedSucceed = enemy.CurrentStateType == EStateType.Skill &&
                                      enemy.CurrentSkill != null &&
                                      enemy.CurrentSkill.InTanDaoTime &&
                                      enemy.CurrentSkill.InTanDaoRange(Actor);

                if (parriedSucceed)
                {
                    m_ParriedEnemies.Add(enemy);
                }
            }

            parriedEnemies = m_ParriedEnemies;
            return m_ParriedEnemies.Count > 0;
        }

        void CheckTanFan()
        {
            bool parriedSucceed = TryParry(out List<SActor> parriedEnemies);
            //Debug.Log($"TryParry:{parriedSucceed}  {parriedEnemies.Count}");
            if (parriedSucceed)
            {
                Actor.CStateMachine.ParriedSuccssSkills.Clear();
                foreach (var e in parriedEnemies)
                {
                    e.OnParried();
                    Actor.CStateMachine.ParriedSuccssSkills.Add(e.CurrentSkill);
                }

                Actor.CAnim.Play("TanFan", force: true);

                m_CurState = EState.TanDao;
                m_TimerCanTanFan = 0;
            }
        }

        public override void OnStay()
        {
            base.OnStay();

            if (m_TimerAlign > 0)
            {
                m_TimerAlign -= base.DeltaTime;
                Actor.CPhysic.AlignForwardTo(Actor.DesiredLookDir, 1080f);
            }

            if (m_TimerCanTanFan > 0)
            {
                m_TimerCanTanFan -= base.DeltaTime;
                CheckTanFan();
            }

            if (m_CurState == EState.TanDao)
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay("TanFan"))
                {
                    Exit();
                }
            }
            else if (m_CurState == EState.DefenseHit)
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay("DefenseHit"))
                {
                    if (m_AutoExitOnAnimFinished)
                    {
                        Actor.CAnim.Play("Idle");
                        Exit();
                    }
                    else
                    {
                        m_CurState = EState.DefenseLoop;
                    }
                }
            }
            else if (m_CurState == EState.DefenseStart)
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay("DefenseStart"))
                {
                    if (m_AutoExitOnAnimFinished)
                    {
                        Actor.CAnim.Play("DefenseEnd");
                        m_CurState = EState.DefenseEnd;
                        m_CanExit = true;
                    }
                    else
                    {
                        m_CurState = EState.DefenseLoop;
                    }
                }
            }
            else if (m_CurState == EState.DefenseLoop)
            {
                if (Actor.MovementAxisMagnitude >= 0.1f)
                {
                    Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, Actor.MovementAxis.x);
                    Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, Actor.MovementAxis.z);

                    // 位移
                    float speed = GetSpeed2D();
                    Actor.CPhysic.AdditivePosition += Actor.DesiredMoveDir * speed * base.DeltaTime;
                    Actor.CPhysic.AlignForwardTo(Actor.DesiredLookDir, 720);

                    // 播放移动动画
                    Actor.CAnim.Play("DefenseMove");
                }
                else
                {
                    Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
                    Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, 0);
                }
            }
            else if (m_CurState == EState.DefenseEnd)
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay("DefenseEnd"))
                {
                    Exit();
                }
            }
        }

        float GetSpeed2D()
        {
            float curSmoothFloatH = Actor.CAnim.GetCurSmoothFloat(EAnimatorParams.Horizontal);
            float curSmoothFloatV = Actor.CAnim.GetCurSmoothFloat(EAnimatorParams.Vertical);
            float curSmoothFloat = Mathf.Sqrt(curSmoothFloatH * curSmoothFloatH + curSmoothFloatV * curSmoothFloatV);
            // float curSmoothFloat = 2 * GameHelper.GetStickLength(curSmoothFloatH * 0.5f, curSmoothFloatV * 0.5f);
            float speed = Mathf.Lerp(0, Character.m_CharacterInfo.m_SpeedDefenseWalk, curSmoothFloat);
            return speed;
        }

        protected override void OnExit()
        {
            base.OnExit();
            Actor.UpdateMovementAxisAnimatorParams = true;
            m_CurState = EState.None;
            m_AutoExitOnAnimFinished = false;
        }

        public void TryEndDefense()
        {
            if (m_CurState == EState.None)
            {
            }
            else if (m_CurState == EState.DefenseStart)
            {
                m_AutoExitOnAnimFinished = true;
            }
            else if (m_CurState == EState.DefenseLoop)
            {
                Actor.CAnim.Play("DefenseEnd");
                m_CurState = EState.DefenseEnd;
                m_CanExit = true;
            }
            else if (m_CurState == EState.DefenseEnd)
            {
            }
            else if (m_CurState == EState.DefenseHit)
            {
                m_AutoExitOnAnimFinished = true;
            }
            else if (m_CurState == EState.TanDao)
            {
                m_AutoExitOnAnimFinished = true;
            }
            else
            {
                Debug.LogError($"Unknown state:{m_CurState}");
            }
        }

        public void OnHit(DamageInfo dmgInfo)
        {
            m_CurState = EState.DefenseHit;

            Actor.CStats.TakeDamage(dmgInfo.DamageValue * 0.3f);
            Actor.CAnim.Play("DefenseHit", force: true);

            // force
            if (dmgInfo.DamageConfig.m_ForceWhenGround.x > 0)
            {
                Vector3 dir = dmgInfo.Attacker.transform.position - Actor.transform.position;
                dir.y = 0;
                Actor.CPhysic.Force_Add(-dir, dmgInfo.DamageConfig.m_ForceWhenGround.x, 1, false);
            }
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.AnimCanExit)
            {
                if (m_CurState == EState.TanDao)
                {
                    Exit();
                }
            }
        }

        public override void OnTriggerRangeEvent(AnimRangeTimeEvent eventObj, bool enter)
        {
            base.OnTriggerRangeEvent(eventObj, enter);
            if (eventObj.EventType == EAnimRangeEvent.CanTriggerSkill)
            {
                m_CanTriggerSkill = enter;
            }
        }

        public bool CanTriggerSkill(SkillItem skill)
        {
            return m_CurState == EState.TanDao &&
                   skill.m_TriggerCondition == ETriggerCondition.AfterTanFanSucceed &&
                   m_CanTriggerSkill;
        }
    }
}