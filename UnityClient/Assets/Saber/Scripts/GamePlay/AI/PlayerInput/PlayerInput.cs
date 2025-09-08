using System;
using System.Collections;
using CombatEditor;
using Saber.Config;
using Saber.Frame;
using Saber.World;
using UnityEngine;
using Saber.CharacterController;
using Saber.UI;

namespace Saber.AI
{
    public abstract class PlayerInput : BaseAI, Wnd_Rest.IHandler
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


        private AheadInputData m_AheadInput;
        private bool m_PlayingAction;
        private float m_TimerCheckLockEnemy;
        private float m_DistanceToLockEnemy;
        protected Portal m_CurrentStayingPortal;
        protected Idol m_CurrentStayingIdol;

        protected PlayerCamera PlayerCameraObj => GameApp.Entry.Game.PlayerCamera;


        public abstract bool Active { set; }
        public abstract void OnPlayerExitPortal(Portal portal);
        public abstract void OnPlayerEnterPortal(Portal portal);
        public abstract void OnPlayerEnterGodStatue(Idol idol);
        public abstract void OnPlayerExitGodStatue(Idol idol);


        public override void Init(SActor actor)
        {
            bool haveOwnerBefore = Actor != null;

            base.Init(actor);

            PlayerCameraObj.SetTarget(actor);
            if (!haveOwnerBefore)
                PlayerCameraObj.transform.position = actor.transform.position;

            Actor.CStateMachine.Event_OnStateChange += OnStateChange;
            Actor.Event_OnDead += OnPlayerDead;

            m_AheadInput = new(Actor);
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

            if (to == EStateType.GetHit)
            {
                ClearAheadInput();
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

            if (m_AheadInput.TryTrigger())
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

        public Coroutine ActiveIdol(Idol idol, Action onWorshiped)
        {
            return GameApp.Entry.Unity.StartCoroutine(ActiveIdolItor(idol, onWorshiped));
        }

        IEnumerator ActiveIdolItor(Idol idol, Action onWorshiped)
        {
            m_PlayingAction = true;

            Actor.CMelee.CWeapon.ToggleWeapon(false);

            bool wait = true;
            Vector3 idolRestPos = idol.Point.GetIdolFixedPos(out _);
            Actor.CStateMachine.SetPosAndForward(idolRestPos, -idol.Point.transform.forward, 0.2f, () => wait = false);
            while (wait)
            {
                yield return null;
            }

            while (true)
            {
                Vector3 dirToStatue = idol.transform.position - Actor.transform.position;
                if (Actor.CPhysic.AlignForwardTo(dirToStatue, 720))
                    break;

                yield return null;
            }

            yield return null;

            Actor.CStateMachine.PlayAction_IdolActive(() =>
            {
                Actor.CMelee.CWeapon.ToggleWeapon(true);
                onWorshiped?.Invoke();
            });


            m_PlayingAction = false;
        }

        public Coroutine PlayerRestBeforeIdol(Idol idol)
        {
            return PlayerRestBeforeIdolItor(idol).StartCoroutine();
        }

        IEnumerator PlayerRestBeforeIdolItor(Idol idol)
        {
            if (Actor.CurrentStateType != EStateType.Idle)
            {
                yield break;
            }

            GameProgressManager.Instance.OnGodStatueRest(idol.SceneID, idol.ID);

            OnPlayerRest();

            Wnd_Rest wndRest = null;
            yield return GameApp.Entry.UI.CreateWnd<Wnd_Rest>(null, this, w => wndRest = w);
            wndRest.ActiveRoot = false;

            bool wait = true;
            Vector3 idolRestPos = idol.Point.GetIdolFixedPos(out _);
            Actor.CStateMachine.SetPosAndForward(idolRestPos, -idol.transform.forward, 0.2f, () => wait = false);
            while (wait)
            {
                yield return null;
            }

            Actor.CMelee.CWeapon.ToggleWeapon(false);
            wait = true;
            Actor.CStateMachine.PlayAction_IdolRest(() => wait = false);
            while (wait)
            {
                yield return null;
            }

            wndRest.ActiveRoot = true;
            yield return null;

            Actor.OnGodStatueRest();

            yield return null;

            GameApp.Entry.Game.World.RecorverOtherActors();
        }

        protected virtual void OnPlayerRest()
        {
        }

        #region Wnd_Rest.IHandler

        void Wnd_Rest.IHandler.OnClickQuit()
        {
            OnClickWndRestQuit();
        }

        protected virtual void OnClickWndRestQuit()
        {
            GameApp.Entry.Game.PlayerAI.Active = true;
            GameApp.Entry.Game.PlayerAI.OnPlayerEnterGodStatue(m_CurrentStayingIdol);

            Actor.CStateMachine.PlayAction_IdolRestEnd(() => { Actor.CMelee.CWeapon.ToggleWeapon(true); });
        }

        void Wnd_Rest.IHandler.OnClickTransmit(int sceneID, int idolID)
        {
            GameApp.Entry.Game.World.Transmit(sceneID, idolID);
        }

        void Wnd_Rest.IHandler.CreateEnemy(int actorID)
        {
            GameApp.Entry.Game.World.CreateEnemy(actorID);
        }

        #endregion
    }
}