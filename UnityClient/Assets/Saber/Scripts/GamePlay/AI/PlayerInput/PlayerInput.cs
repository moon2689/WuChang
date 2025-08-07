using System;
using System.Collections;
using CombatEditor;
using Saber.Config;
using Saber.Frame;
using Saber.World;
using UnityEngine;
using Saber.CharacterController;

namespace Saber.AI
{
    public abstract class PlayerInput : BaseAI
    {
        #region 单例

        private static PlayerInput s_Instance;

        public static PlayerInput Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    var gameSetting = GameApp.Entry.Config.GameSetting;
                    if (gameSetting.PlayerInputType == GameSettingInfo.EPlayerInputType.PC)
                    {
                        s_Instance = new PlayerPCInput();
                    }
                    else if (gameSetting.PlayerInputType == GameSettingInfo.EPlayerInputType.Phone)
                    {
                        s_Instance = new PlayerPhoneInput();
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown input type:" + gameSetting.PlayerInputType);
                    }
                }

                return s_Instance;
            }
        }

        #endregion


        private AheadInputData m_AheadInput = new();
        private bool m_PlayingAction;
        private float m_TimerCheckLockEnemy;
        private float m_DistanceToLockEnemy;


        protected PlayerCamera PlayerCameraObj => GameApp.Entry.Game.PlayerCamera;


        public abstract bool Active { set; }
        public abstract void OnPlayerExitPortal(Portal portal);
        public abstract void OnPlayerEnterPortal(Portal portal);
        public abstract void OnPlayerEnterGodStatue(GodStatue godStatue);
        public abstract void OnPlayerExitGodStatue(GodStatue godStatue);


        public override void Init(SActor actor)
        {
            bool haveOwnerBefore = Actor != null;

            base.Init(actor);

            PlayerCameraObj.SetTarget(actor);
            if (!haveOwnerBefore)
                PlayerCameraObj.transform.position = actor.transform.position;

            Actor.CStateMachine.Event_OnStateChange += OnStateChange;
            Actor.Event_OnDead += OnPlayerDead;
        }

        private void OnPlayerDead(SActor obj)
        {
            ClearLockEnemy();
        }

        private void OnStateChange(EStateType from, EStateType to)
        {
            if (from == EStateType.Skill && to != EStateType.Skill)
            {
                if (GameApp.Entry.Game.PlayerCamera.LockTarget == null)
                {
                    LockingEnemy = null;
                }
            }
        }

        protected void OnTriggerSkill(ESkillType key)
        {
            var tarSkill = GameApp.Entry.Game.Player.CMelee.GetSkillObject(key);
            if (tarSkill.SkillConfig.CostStrength > 0 && Actor.CStats.CurrentStamina <= 0)
            {
                GameApp.Entry.UI.ShowTips("体力不足");
                GameApp.Entry.Game.Audio.PlaySoundSkillFailed();
            }
            else if (!tarSkill.IsCDCooldown)
            {
                GameApp.Entry.Game.Audio.PlaySoundSkillFailed();
                GameApp.Entry.UI.ShowTips("技能正在冷却中", 0.1f);
            }

            bool needFindEnemy = LockingEnemy == null || Actor.CurrentStateType != EStateType.Skill;
            if (needFindEnemy)
            {
                TryLockEnemy(2, true);
            }

            bool succeed = Actor.TryTriggerSkill(key);
            // Debug.Log($"try trigger skill {key} {succeed}");
            if (!succeed)
            {
                m_AheadInput.SetData_Skill(key);
            }
        }

        // 处理预输入
        void UpdateAheadInput()
        {
            if (!m_AheadInput.IsEnabled)
            {
                return;
            }

            if (Actor.CStats.CurrentStamina <= 0)
            {
                ClearAheadInput();
            }

            if (m_AheadInput.TryTrigger(Actor))
            {
                ClearAheadInput();
            }
        }

        protected void ClearAheadInput()
        {
            m_AheadInput.Clear();
        }

        protected void OnDefenseDown()
        {
            Actor.DefenseStart();
        }

        protected void OnDefenseUp()
        {
            Actor.DefenseEnd();
        }

        private bool TryLockEnemy()
        {
            return TryLockEnemy(10, true);
        }

        protected bool TryLockEnemy(float radius, bool lockCamera)
        {
            Collider[] colliders = new Collider[100];
            int count = Physics.OverlapSphereNonAlloc(Actor.transform.position, radius, colliders,
                EStaticLayers.Actor.GetLayerMask());
            SActor tarEnemy = null;
            float minDis = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                Collider tar = colliders[i];
                var enemy = tar.GetComponent<SActor>();
                if (enemy != null && enemy != Actor && !enemy.IsDead && enemy.Camp != Actor.Camp)
                {
                    Vector3 dirToEnemy = enemy.transform.position - Actor.transform.position;
                    /*
                    if (Vector3.Dot(dirToEnemy, Actor.transform.forward) <= 0)
                        continue;
                    */

                    float dis = dirToEnemy.sqrMagnitude;
                    if (dis < minDis)
                    {
                        minDis = dis;
                        tarEnemy = enemy;
                    }
                }
            }

            if (tarEnemy != null)
            {
                LockEnemy(tarEnemy, lockCamera);
                return true;
            }

            return false;
        }

        void LockEnemy(SActor tarEnemy, bool lockCamera)
        {
            if (tarEnemy == null)
            {
                return;
            }

            LockingEnemy = tarEnemy;
            Actor.EyeLockAt(tarEnemy);
            if (lockCamera)
            {
                GameApp.Entry.Game.PlayerCamera.LockTarget = tarEnemy;
                tarEnemy.Event_OnDead += obj => ClearLockEnemy();
            }
        }

        public override void ClearLockEnemy()
        {
            base.ClearLockEnemy();
            Actor?.EyeLockAt(null);
            GameApp.Entry.Game.PlayerCamera.LockTarget = null;
        }

        protected void OnDodgeDown()
        {
            if (Actor.CStats.CurrentStamina <= 0)
            {
                GameApp.Entry.UI.ShowTips("体力不足");
                GameApp.Entry.Game.Audio.PlaySoundSkillFailed();
            }

            if (!Actor.Dodge())
                m_AheadInput.SetData_Dodge(Actor.MovementAxis);
        }

        public override void Update()
        {
            base.Update();

            if (m_PlayingAction)
            {
                return;
            }

            if (GameApp.Entry.Game.PlayerCamera.Target != null && GameApp.Entry.Game.PlayerCamera.Target.Equals(Actor))
            {
                Vector3 lookDir;
                if (LockingEnemy != null)
                {
                    lookDir = LockingEnemy.transform.position - Actor.transform.position;
                }
                else
                {
                    lookDir = GameApp.Entry.Game.PlayerCamera.CamT.forward;
                }

                Actor.DesiredLookDirIn3D = lookDir;
                lookDir = Vector3.ProjectOnPlane(lookDir, Vector3.up).normalized;
                Actor.DesiredLookDir = lookDir;
            }

            UpdateAheadInput();

            if (LockingEnemy)
                CheckLockEnemy();
        }

        void CheckLockEnemy()
        {
            if (m_TimerCheckLockEnemy >= 0)
            {
                m_TimerCheckLockEnemy -= Time.deltaTime;
                if (m_TimerCheckLockEnemy < 0)
                {
                    m_TimerCheckLockEnemy = 0.5f;
                    Vector3 disV3 = LockingEnemy.transform.position - Actor.transform.position;
                    m_DistanceToLockEnemy = disV3.magnitude;
                    if (m_DistanceToLockEnemy > Actor.m_BaseActorInfo.m_AIInfo.m_WarningRange)
                    {
                        ClearLockEnemy();
                    }
                }
            }
        }

        protected void OnLockEnemyDown()
        {
            if (LockingEnemy != null && GameApp.Entry.Game.PlayerCamera.LockTarget != null)
            {
                ClearLockEnemy();
            }
            else
            {
                bool lockSucceed = TryLockEnemy();
                if (!lockSucceed)
                {
                    GameApp.Entry.Game.PlayerCamera.LookAtTargetBack();
                }
            }
        }

        public override void Release()
        {
            base.Release();
            PlayerCameraObj?.ClearTarget();
        }

        public Coroutine PlayActionMoveToTargetPos(Vector3 targetPos, float distanceOffset, Action onReached)
        {
            return GameApp.Entry.Unity.StartCoroutine(MoveToTargetPosItor(targetPos, distanceOffset, onReached));
        }

        IEnumerator MoveToTargetPosItor(Vector3 targetPos, float distanceOffset, Action onReached)
        {
            m_PlayingAction = true;

            yield return new WaitForSeconds(1);

            distanceOffset = Mathf.Max(0.1f, distanceOffset);

            while (true)
            {
                Vector3 dir = targetPos - Actor.transform.position;
                dir.y = 0;

                if (dir.magnitude > distanceOffset)
                {
                    Actor.DesiredLookDir = dir;
                    Actor.StartMove(EMoveSpeedV.Walk, new Vector3(0, 0, 1));
                }
                else
                {
                    break;
                }

                yield return null;
            }

            Actor.StopMove();

            yield return new WaitForSeconds(0.3f);
            m_PlayingAction = false;
            yield return null;

            onReached?.Invoke();
        }

        public void WorshipGodStatue(GodStatue godStatue, Action onWorshiped)
        {
            GameApp.Entry.Unity.StartCoroutine(WorshipGodStatueItor(godStatue, onWorshiped));
        }

        IEnumerator WorshipGodStatueItor(GodStatue godStatue, Action onWorshiped)
        {
            m_PlayingAction = true;

            while (true)
            {
                Vector3 dirToStatue = godStatue.transform.position - Actor.transform.position;
                if (Actor.CPhysic.AlignForwardTo(dirToStatue, 720))
                    break;

                yield return null;
            }

            yield return null;

            Actor.CStateMachine.PlayAction_BranchRepair(onWorshiped);

            m_PlayingAction = false;
        }
    }
}