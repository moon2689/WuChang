using System;
using System.Collections;
using Saber.Frame;
using Saber.UI;
using Saber.World;
using UnityEngine;
using Saber.CharacterController;

namespace Saber.AI
{
    public class PlayerPhoneInput : BaseAI, Wnd_JoyStick.IHandler
    {
        // 单例
        private static PlayerPhoneInput s_Instance;
        public static PlayerPhoneInput Instance => s_Instance ??= new PlayerPhoneInput();

        private Wnd_JoyStick m_WndJoyStick;
        private AheadInputData m_AheadInput;
        private bool m_PlayingAction;
        private float m_TimerCheckLockEnemy;
        private float m_DistanceToLockEnemy;
        private Portal m_CurrentStayingPortal;
        private Idol m_CurrentStayingIdol;
        private float? m_oldTouchDis;
        private bool m_Sprint;
        private bool m_PressDefense;
        private Vector3 m_Stick;
        private float m_StickLength;
        private bool m_ToCheckChargeAttack;
        private float m_PressDownHeavyAttackTime;
        private bool m_IsTryMoving;
        private float m_PressDodgeDownTime;
        private bool m_PressingLockOn;
        private float m_PressDownLockOnTime;


        private PlayerCamera PlayerCameraObj => GameApp.Entry.Game.PlayerCamera;

        public bool Active
        {
            set => m_WndJoyStick.IsShow = value;
        }

        public override void Init(SActor actor)
        {
            bool haveOwnerBefore = Actor != null;

            base.Init(actor);

            PlayerCameraObj.SetTarget(actor);
            if (!haveOwnerBefore)
                PlayerCameraObj.transform.position = actor.transform.position;

            Actor.CStateMachine.Event_OnStateChange += OnStateChange;

            m_AheadInput = new(Actor);

            // wnd
            if (m_WndJoyStick == null)
            {
                m_WndJoyStick = GameApp.Entry.UI.GetWnd<Wnd_JoyStick>();
                m_WndJoyStick.Handler = this;
            }

            GameApp.Entry.Unity.DoActionOneFrameLater(() =>
            {
                this.Actor.CStats.EventOnHPPointCountChange += RefreshHPPointCount;
                RefreshHPPointCount();
            });

            actor.CStateMachine.Event_OnStateChange += OnStateChange;
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

        private void OnTriggerSkill(ESkillType key)
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

        private void ClearAheadInput()
        {
            m_AheadInput.Clear();
        }

        private bool TryLockEnemy()
        {
            return TryLockEnemy(Actor.m_BaseActorInfo.m_AIInfo.m_WarningRange, true);
        }

        private bool TryLockEnemy(float radius, bool lockCamera)
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
            }
        }

        protected override void OnDead(SActor owner)
        {
            ClearLockEnemy();
        }

        private void ClearLockEnemy()
        {
            LockingEnemy = null;
            Actor.EyeLockAt(null);
            GameApp.Entry.Game.PlayerCamera.LockTarget = null;
        }

        void CheckLockEnemy()
        {
            if (LockingEnemy != null && LockingEnemy.IsDead)
            {
                ClearLockEnemy();
                TryLockEnemy();
                return;
            }

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

        public Coroutine PlayActionMoveToTargetPos(Vector3 targetPos, Action onReached)
        {
            return GameApp.Entry.Unity.StartCoroutine(MoveToTargetPosItor(targetPos, onReached));
        }

        IEnumerator MoveToTargetPosItor(Vector3 targetPos, Action onReached)
        {
            m_PlayingAction = true;
            Vector3 startDir = targetPos - Actor.transform.position;
            startDir.y = 0;

            while (true)
            {
                Vector3 dir = targetPos - Actor.transform.position;
                dir.y = 0;

                if (Vector3.Dot(dir, startDir) > 0)
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
            bool wait = true;
            Vector3 idolRestPos = idol.Point.GetIdolFixedPos(out _);
            bool succeed = Actor.CStateMachine.SetPosAndForward(idolRestPos, -idol.Point.transform.forward, () => wait = false);
            if (!succeed)
            {
                GameApp.Entry.UI.ShowTips("当前状态不能执行该操作");
                yield break;
            }

            m_PlayingAction = true;
            Actor.CMelee.CWeapon.ShowOrHideWeapon(false);
            GameApp.Entry.Game.World.SetFilmEffect(true);

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

            Actor.PlayAction(PlayActionState.EActionType.IdolActive, () =>
            {
                Actor.CMelee.CWeapon.ShowOrHideWeapon(true);
                GameApp.Entry.Game.World.SetFilmEffect(false);
                onWorshiped?.Invoke();
            });

            m_PlayingAction = false;
        }

        void RefreshHPPointCount()
        {
            int count = GameApp.Entry.Game.Player.CStats.HPPotionCount;
            m_WndJoyStick.RefreshMedicineCount(count);
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

            UpdateCamera();

            if (m_PressDefense)
            {
                if (Actor.CurrentStateType != EStateType.Defense)
                {
                    Actor.DefenseStart();
                    ClearAheadInput();
                }
            }
            else
            {
                if (Actor.CurrentStateType == EStateType.Defense)
                    Actor.DefenseEnd();
            }

            UpdateMovement();

            UpdateHeavyAttack();

            UpdateLockOnInput();
        }

        private void UpdateLockOnInput()
        {
            if (m_PressingLockOn)
            {
                if (Time.time - m_PressDownLockOnTime > 0.3f)
                {
                    ClearLockEnemy();
                }
            }
        }

        // 镜头
        void UpdateCamera()
        {
#if UNITY_EDITOR
            // 缩放
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            float scale = GameHelper.IsAndroid ? 2f : 0.2f;
            float offset = scrollWheel * scale;
            if (Mathf.Abs(offset) > 0.001f)
            {
                PlayerCameraObj.Zoom(offset);
            }
#else
            // 缩放
            if (Input.touchCount == 2 && !Widget_JoyStick.IsDragging)
            {
                var touch0 = Input.GetTouch(0);
                var touch1 = Input.GetTouch(1);
                if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                {
                    Vector2 v2Dis = touch0.position - touch1.position;
                    float dis = v2Dis.magnitude;

                    if (m_oldTouchDis != null)
                    {
                        float offset = (m_oldTouchDis.Value - dis) * 0.005f;
                        if (Mathf.Abs(offset) > 0.001f)
                        {
                            PlayerCameraObj.Zoom(-offset);
                        }
                    }

                    m_oldTouchDis = dis;
                }
            }
            else
            {
                m_oldTouchDis = null;
            }
#endif
        }


        #region Scene Interact

        public void OnPlayerEnterPortal(Portal portal)
        {
            m_CurrentStayingPortal = portal;
            m_WndJoyStick.ShowButtonInteract(ESceneInteractType.Portal);
        }

        public void OnPlayerExitPortal(Portal portal)
        {
            m_CurrentStayingPortal = null;
            m_WndJoyStick.HideButtonInteract();
        }

        public void OnPlayerEnterGodStatue(Idol idol)
        {
            m_CurrentStayingIdol = idol;

            ESceneInteractType interactType = idol.IsFired ? ESceneInteractType.Rest : ESceneInteractType.ActiveIdol;
            m_WndJoyStick.ShowButtonInteract(interactType);
        }

        public void OnPlayerExitGodStatue(Idol idol)
        {
            m_CurrentStayingIdol = null;
            m_WndJoyStick.HideButtonInteract();
        }

        #endregion


        #region Wnd_JoyStick.IHandler

        void Wnd_JoyStick.IHandler.OnClickInteract(ESceneInteractType interactType)
        {
            if (interactType == ESceneInteractType.Portal)
            {
                if (m_CurrentStayingPortal)
                    m_CurrentStayingPortal.Transmit();
            }
            else if (interactType == ESceneInteractType.ActiveIdol)
            {
                if (m_CurrentStayingIdol)
                {
                    m_CurrentStayingIdol.Active();
                    m_WndJoyStick.ShowButtonInteract(ESceneInteractType.Rest);
                }
            }
            else if (interactType == ESceneInteractType.Rest)
            {
                if (m_CurrentStayingIdol)
                    m_CurrentStayingIdol.Rest();
            }
            else
            {
                throw new InvalidOperationException($"Unknown ESceneInteractType:{interactType}");
            }
        }

        void Wnd_JoyStick.IHandler.OnPressDefense(bool value)
        {
            m_PressDefense = value;
            if (value)
            {
                Actor.DefenseStart();
                ClearAheadInput();
            }
            else
            {
                Actor.DefenseEnd();
            }
        }

        void Wnd_JoyStick.IHandler.OnUseCamStick(float x, float y)
        {
            PlayerCameraObj.MovementAxis = new Vector2(x, y);
        }

        void Wnd_JoyStick.IHandler.OnUseStick(Vector2 axisV2, bool isDragging)
        {
            m_Stick = new Vector3(axisV2.x, 0, axisV2.y);
            m_StickLength = m_Stick.magnitude;
            Actor.IsDraggingMovementAxis = isDragging;
        }

        void UpdateMovement()
        {
            EMoveSpeedV moveSpeedV;
            if (m_StickLength <= 0.1f)
            {
                moveSpeedV = EMoveSpeedV.None;
            }
            else if (m_StickLength > 0.5f)
            {
                moveSpeedV = m_Sprint ? EMoveSpeedV.Sprint : EMoveSpeedV.Run;
            }
            else if (m_StickLength > 0.1f)
            {
                moveSpeedV = EMoveSpeedV.Walk;
            }
            else
            {
                moveSpeedV = EMoveSpeedV.None;
            }

            if (m_Sprint)
            {
                if (moveSpeedV != EMoveSpeedV.Sprint)
                {
                    m_Sprint = false;
                }
                else if (Actor.CStats.CurrentStamina <= 0)
                {
                    m_Sprint = false;
                }
                else if (Actor.CurrentStateType == EStateType.Move && Actor.MoveSpeedV != EMoveSpeedV.Sprint)
                {
                    m_Sprint = false;
                }
            }

            if (moveSpeedV != EMoveSpeedV.None)
            {
                if (!m_IsTryMoving)
                {
                    ClearAheadInput();
                }

                m_IsTryMoving = true;
                Actor.StartMove(moveSpeedV, m_Stick);
            }
            else
            {
                m_IsTryMoving = false;
                Actor.StopMove();
            }
        }

        void Wnd_JoyStick.IHandler.OnClickLightAttack()
        {
            OnTriggerSkill(ESkillType.LightAttack);
        }

        void Wnd_JoyStick.IHandler.OnPressDodge(bool value)
        {
            //m_Sprint = value;
            if (value)
            {
                m_Sprint = true;
                m_PressDodgeDownTime = Time.time;

                ClearAheadInput();

                if (Actor.CStats.CurrentStamina <= 0)
                {
                    GameApp.Entry.UI.ShowTips("体力不足");
                    GameApp.Entry.Game.Audio.PlaySoundSkillFailed();
                }

                if (!Actor.Dodge())
                    m_AheadInput.SetData_Dodge(Actor.MovementAxis);
            }
            else
            {
                if (Time.time - m_PressDodgeDownTime < 1f)
                {
                    m_Sprint = false;
                }
            }
        }

        void Wnd_JoyStick.IHandler.OnPressLockOn(bool value)
        {
            m_PressingLockOn = value;
            if (value)
            {
                m_PressDownLockOnTime = Time.time;
            }
            else if (Time.time - m_PressDownLockOnTime <= 0.3f)
            {
                bool lockSucceed = TryLockEnemy();
                if (!lockSucceed)
                {
                    GameApp.Entry.Game.PlayerCamera.LookAtTargetBack();
                }
            }
        }

        void Wnd_JoyStick.IHandler.OnClickDrinkMedicine()
        {
            ClearAheadInput();
            if (!Actor.DrinkPotion())
            {
                m_AheadInput.SetData_DrinkPotion();
            }
        }

        void Wnd_JoyStick.IHandler.OnClickSkill(ESkillType type)
        {
            OnTriggerSkill(type);
        }

        void Wnd_JoyStick.IHandler.OnPressHeavyAttack(bool press)
        {
            m_ToCheckChargeAttack = press;
            if (press)
            {
                m_PressDownHeavyAttackTime = Time.time;
            }
            else
            {
                if (Time.time - m_PressDownHeavyAttackTime < 0.2f)
                {
                    OnTriggerSkill(ESkillType.HeavyAttack);
                }
            }
        }

        void UpdateHeavyAttack()
        {
            if (m_ToCheckChargeAttack)
            {
                if (Time.time - m_PressDownHeavyAttackTime >= 0.2f)
                {
                    m_ToCheckChargeAttack = false;
                    OnTriggerSkill(ESkillType.ChargeAttack);
                }
            }
        }

        void Wnd_JoyStick.IHandler.OnClickBag()
        {
            GameApp.Entry.UI.CreateWnd<Wnd_Bag>(null);
        }

        #endregion


        public override void Release()
        {
            base.Release();
            PlayerCameraObj?.ClearTarget();
            this.Actor.CStats.EventOnHPPointCountChange -= RefreshHPPointCount;
        }
    }
}