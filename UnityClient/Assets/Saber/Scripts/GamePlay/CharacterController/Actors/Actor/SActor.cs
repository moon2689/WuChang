using System;
using System.Collections.Generic;
using System.Linq;
using CombatEditor;
using UnityEngine;
using Saber.Config;
using Saber.Frame;
using Saber.AI;

namespace Saber.CharacterController
{
    public abstract class SActor : MonoBehaviour, AnimatorLayer.IHandler, ActorBaseStats.IHandler, PlayerCamera.ITarget
    {
        public event Action<SActor> Event_OnDead;
        public event Action<SActor, float> Event_OnDamage;


        [SerializeField] public BaseActorInfo m_BaseActorInfo;

        private float m_TimeMultiplier = 1;
        private BaseAI m_AI;
        private Vector3 m_MovementAxis;
        protected EMoveSpeedV m_MoveSpeedV;
        public AnimSpeedExecutor m_AnimSpeedExecutor;
        private List<AudioPlayer> m_PlayingSounds = new();
        private bool m_IsDead;


        public abstract ActorStateMachine CStateMachine { get; }

        public StatsInfo StatsInfo => m_BaseActorInfo.m_StatsInfo;
        public PhysicInfo PhysicInfo => m_BaseActorInfo.m_PhysicInfo;
        public ActorItemInfo BaseInfo { get; set; }

        /// <summary>物理</summary>
        public CharacterPhysic CPhysic { get; protected set; }

        /// <summary>动画</summary>
        public CharacterAnimation CAnim { get; protected set; }

        /// <summary>属性</summary>
        public ActorBaseStats CStats { get; protected set; }

        /// <summary>换装</summary>
        public CharacterDressUp CDressUp { get; protected set; }

        /// <summary>战斗</summary>
        public CharacterMelee CMelee { get; protected set; }

        public ActorBuffManager CBuff { get; protected set; }


        /// <summary> The current value of the Delta time the animal is using (Fixed or not)</summary>
        public float DeltaTime { get; protected set; }

        public float TimeMultiplier
        {
            get => m_TimeMultiplier;
            set => m_TimeMultiplier = value;
        }

        public BaseAI AI
        {
            get => m_AI;
            set
            {
                m_AI = value;
                if (value != null)
                {
                    value.Init(this);
                }
            }
        }


        /// <summary>当前遥杆输入</summary>
        public Vector3 MovementAxis
        {
            get => m_MovementAxis;
            set
            {
                m_MovementAxis = value;
                if (m_MovementAxis != Vector3.zero)
                {
                    m_MovementAxis.Normalize();
                    MovementAxisMagnitude = 1;
                }
                else
                {
                    MovementAxisMagnitude = 0;
                }
            }
        }

        public bool IsDraggingMovementAxis { get; set; }

        /// <summary>当前遥杆输入的强度</summary>
        public float MovementAxisMagnitude { get; private set; }

        /// <summary>当前注视的方向，无锁定时为摄像机前方，如果有锁定敌人，则为指向敌人的方向</summary>
        public Vector3 DesiredLookDir { get; set; }

        public Vector3 DesiredLookDirIn3D { get; set; }

        /// <summary>移动的方向</summary>
        public Vector3 DesiredMoveDir
        {
            get
            {
                if (MovementAxis != Vector3.zero && DesiredLookDir != Vector3.zero)
                    return Quaternion.LookRotation(DesiredLookDir) * MovementAxis;
                return Vector3.zero;
            }
        }

        /// <summary>正前方，装备武器时为摄像机或者敌人方向，未装备时为希望移动的方向</summary>
        //public Vector3 DesiredForwardDir => StrafeMode ? DesiredLookDir : DesiredMoveDir;

        public virtual EMoveSpeedV MoveSpeedV
        {
            get => m_MoveSpeedV;
            set { m_MoveSpeedV = value; }
        }

        /// <summary>处于此种模式时，角色始终面向摄像机前方，用屁股对着摄像机。</summary>
        //public bool StrafeMode { get; set; }

        public bool IsDead
        {
            get => m_IsDead;
            set
            {
                m_IsDead = value;
                CAnim.SetBool(EAnimatorParams.Die, value);
            }
        }

        //public SkillAnimEvent CurActivingSkillAnimState { get; set; }
        public EActorCamp Camp { get; set; }
        public bool Invincible { get; set; }

        public HurtBox[] HurtBoxes => GetComponentsInChildren<HurtBox>();

        public bool IsPlayer
        {
            get => m_BaseActorInfo.m_IsPlayer;
            set => m_BaseActorInfo.m_IsPlayer = value;
        }

        public EStateType CurrentStateType => CStateMachine.CurrentStateType;
        public bool UpdateMovementAxisAnimatorParams { get; set; } = true;
        public bool WaitStaminaRecoverBeforeSprint { get; set; }
        public WeaponBase[] CurrentWeapons => CMelee.CWeapon.CurWeapons;

        public EWeaponStyle CurrentWeaponStyle => CMelee.CurWeaponStyle;

        public SkillItem[] Skills => CMelee.ValidSkills;

        public SkillConfig SkillConfigs => m_BaseActorInfo.m_SkillConfig;

        public BaseSkill CurrentSkill => CMelee?.CurSkill;
        public virtual bool CanSpeech => false;

        public virtual bool IsSpeeching => false;
        public bool IsStun { get; set; }

        public bool IsTimelineMode
        {
            set
            {
                if (value)
                {
                    enabled = false;
                    CPhysic.RB.isKinematic = true;
                    CPhysic.Active(false);
                }
                else
                {
                    enabled = true;
                    CPhysic.RB.isKinematic = false;
                    CPhysic.Active(true);
                }
            }
        }

        /// <summary>可被处决</summary>
        public bool CanBeDecapitate { get; set; }

        /// <summary>角色是否处于潜行中</summary>
        public bool IsInStealth
        {
            get
            {
                if (CurrentStateType == EStateType.Idle)
                {
                    return true;
                }

                if (CurrentStateType == EStateType.Move)
                {
                    return MoveSpeedV != EMoveSpeedV.Run && MoveSpeedV != EMoveSpeedV.Sprint;
                }

                if (CurrentStateType == EStateType.Skill)
                {
                    return CurrentSkill.IsQuiet;
                }

                return false;
            }
        }


        public static SActor Create(int id, Vector3 pos, Quaternion rot, BaseAI ai, EActorCamp camp)
        {
            var ownerInfo = GameApp.Entry.Config.ActorInfo.m_Actors.FirstOrDefault(a => a.m_ID == id);
            if (ownerInfo == null)
            {
                Debug.LogError("ownerInfo == null,id:" + id);
                return null;
            }

            var actor = Create(ownerInfo, pos, rot);
            actor.Camp = camp;
            actor.AI = ai;
            return actor;
        }

        public static SActor Create(int id, Vector3 pos, Quaternion rot)
        {
            var ownerInfo = GameApp.Entry.Config.ActorInfo.m_Actors.FirstOrDefault(a => a.m_ID == id);
            if (ownerInfo == null)
            {
                Debug.LogError("ownerInfo == null,id:" + id);
                return null;
            }

            var actor = Create(ownerInfo, pos, rot);
            return actor;
        }

        static SActor Create(ActorItemInfo config, Vector3 pos, Quaternion rot)
        {
            GameObject playerGO = config.LoadGameObject();
            playerGO.transform.position = pos;
            playerGO.transform.rotation = rot;
            SActor actor = playerGO.GetComponent<SActor>();
            actor.name = config.m_PrefabName;
            actor.BaseInfo = config;
            return actor;
        }

        protected virtual void Awake()
        {
            gameObject.SetLayerRecursive(EStaticLayers.Actor);

            CBuff = new();
            CAnim = new CharacterAnimation(this, this);
            CPhysic = new CharacterPhysic(this, m_BaseActorInfo.m_PhysicInfo);
            CStats = new ActorBaseStats(this);
            CMelee = new CharacterMelee(this, m_BaseActorInfo.m_SkillConfig);
            CMelee.SetWeapon(m_BaseActorInfo.m_WeaponPrefabs);
        }

        protected virtual void Start()
        {
        }

        protected virtual void OnAnimatorMove()
        {
            DeltaTime = Time.fixedDeltaTime;

            CPhysic.ResetMotionValues();
            CAnim.Update();
            CStateMachine.Update();
            CPhysic.Update();
            CMelee.Update();
        }

        protected virtual void Update()
        {
            CStats?.Update(Time.deltaTime);
            AI?.Update();

            if (m_AnimSpeedExecutor == null)
                m_AnimSpeedExecutor = new AnimSpeedExecutor(this);
            m_AnimSpeedExecutor.Execute();

            UpdateTimeMultiplier();
        }

        public void Destroy()
        {
            GameObject.Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            StopAllCoroutines();
            AI?.Release();
        }


        #region Animation Event

        public virtual void OnAnimEnter(int nameHash, int layer)
        {
            CStateMachine?.CurrentState?.OnAnimEnter(nameHash, layer);
        }

        public virtual void OnAnimExit(int nameHash, int layer)
        {
            CStateMachine?.CurrentState?.OnAnimExit(nameHash, layer);
        }

        /// <summary>当触发动画事件</summary>
        public virtual void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            //Debug.Log($"OnTriggerAnimEvent {eventItem.m_AnimEvent}");
            this.CStateMachine.CurrentState.OnTriggerAnimEvent(eventObj);
        }

        /// <summary>当触发动画事件</summary>
        public virtual void OnTriggerAnimRangeTimeEvent(AnimRangeTimeEvent eventObj, bool enter)
        {
            //Debug.Log($"OnTriggerAnimEvent {eventItem.m_AnimEvent}");
            this.CStateMachine.CurrentState.OnTriggerRangeEvent(eventObj, enter);
        }

        public void OnTriggerAnimClipEvent(string str)
        {
            //Debug.Log($"OnTriggerAnimClipEvent,param:{str}", gameObject);
            this.CStateMachine.CurrentState.OnTriggerAnimClipEvent(str);
        }

        #endregion

        public virtual void ToggleTrailShadowEffect(bool show)
        {
        }

        public virtual void SetGroundOffset(float offset)
        {
            m_BaseActorInfo.m_PhysicInfo.m_GroundOffset = offset;
        }

        public Transform GetNodeTransform(ENodeType type)
        {
            if (type == ENodeType.Animator)
            {
                return transform;
            }

            return m_BaseActorInfo.GetNode(type);
        }

        public virtual void OnPlayDamageEffect(Vector3 pos)
        {
        }

        public void Rebirth()
        {
            gameObject.SetActive(true);
            CStats.Reset();
            IsDead = false;

            StopMove();
            CStateMachine.ForceEnterState(EStateType.Idle);

            AI?.Init(this);
        }

        #region States

        public bool TryTriggerSkill(SkillItem skillItem)
        {
            return CMelee != null && CMelee.TryTriggerSkill(skillItem);
        }

        public bool TryTriggerSkill(ESkillType type)
        {
            return CMelee != null && CMelee.TryTriggerSkill(type);
        }

        public bool StartMove(EMoveSpeedV moveSpeedV, Vector3 movementAxis)
        {
            MovementAxis = movementAxis;
            MoveSpeedV = moveSpeedV;
            return CStateMachine.StartMove();
        }

        public bool StartMove()
        {
            return CStateMachine.StartMove();
        }

        public void StopMove()
        {
            MoveSpeedV = EMoveSpeedV.None;
            MovementAxis = Vector3.zero;
        }

        public bool OnHit(DamageInfo dmgInfo)
        {
            return CStateMachine.OnHit(dmgInfo);
        }

        public bool Fall()
        {
            return CStateMachine.Fall();
        }

        public bool Dodge(Vector3 axis)
        {
            return CStateMachine.Dodge(axis);
        }

        public bool Dodge()
        {
            return CStateMachine.Dodge(MovementAxis);
        }

        public void OnParried()
        {
            CStateMachine.OnParried();
        }

        public bool DefenseStart()
        {
            return CStateMachine.DefenseStart();
        }

        public bool DefenseEnd()
        {
            return CStateMachine.DefenseEnd();
        }

        public void DefenseHit(DamageInfo dmgInfo)
        {
            CStateMachine.DefenseHit(dmgInfo);
        }

        public WeaponBase GetWeaponByPos(ENodeType bone)
        {
            return CMelee?.CWeapon?.GetWeaponByPos(bone);
        }

        /// <summary>变湿</summary>
        public virtual void GetWet(bool wet)
        {
        }

        /// <summary>眼睛看向某目标</summary>
        public virtual void EyeLockAt(SActor actor)
        {
        }

        public virtual void Speech()
        {
        }

        public virtual void UseItem(UseItem.EItemType itemType)
        {
        }

        /// <summary>变得虚弱，可被处决</summary>
        public bool BeWeek()
        {
            return CStateMachine.BeWeek();
        }


        /// <summary>被处决</summary>
        public bool BeExecute(SkillExecute.EExecuteType executeType)
        {
            return CStateMachine.BeExecute(executeType);
        }

        public virtual void OnGodStatueRest()
        {
            CStats.PlayHealingEffect(CStats.MaxHp);
            CStats.ResetHPPointCount();
            CStats.ResetPower();

            if (CStats.CurrentPower < 10)
            {
                CStats.CurrentPower = 10;
            }
        }

        #endregion


        #region PlayerCamera.ITarget

        Vector3 PlayerCamera.ITarget.Position => transform.position;
        Quaternion PlayerCamera.ITarget.Rotation => transform.rotation;
        float PlayerCamera.ITarget.Height => CPhysic.Height;

        bool PlayerCamera.ITarget.IsMoving =>
            CurrentStateType == EStateType.Move || CurrentStateType == EStateType.Dodge;

        #endregion


        #region ActorBaseStats.IHandler

        public virtual void OnHpChange(float curHp)
        {
            if (curHp <= 0)
            {
                if (GameApp.Entry.Config.GameSetting.DebugFight)
                {
                    GameApp.Entry.Unity.DoDelayAction(3, CStats.Reset);
                }
                else
                {
                    IsDead = true;
                    Event_OnDead?.Invoke(this);
                }
            }
        }

        void ActorBaseStats.IHandler.OnDamaged(float damage)
        {
            Event_OnDamage?.Invoke(this, damage);
        }

        #endregion

        public void PlaySound(AudioClip clip)
        {
            AudioPlayer audioPlayer = GameApp.Entry.Game.Audio.Play3DSound(clip, transform.position,
                player => { m_PlayingSounds.Remove(player); });
            m_PlayingSounds.Add(audioPlayer);
        }

        void UpdateTimeMultiplier()
        {
            for (int i = 0; i < m_PlayingSounds.Count; i++)
            {
                var p = m_PlayingSounds[i];
                if (p != null)
                {
                    p.AudioSource.pitch = TimeMultiplier;
                }
            }
        }

        #region 动画事件

        public void FootL()
        {
            OnTriggerAnimClipEvent(EAnimClipEventType.FootL);
        }

        public void FootR()
        {
            OnTriggerAnimClipEvent(EAnimClipEventType.FootR);
        }

        public void ActionFootL()
        {
            OnTriggerAnimClipEvent(EAnimClipEventType.ActionFootL);
        }

        public void ActionFootR()
        {
            OnTriggerAnimClipEvent(EAnimClipEventType.ActionFootR);
        }

        public void RushStopLeft()
        {
            OnTriggerAnimClipEvent(EAnimClipEventType.RushStopLeft);
        }

        public void RollEvent()
        {
            OnTriggerAnimClipEvent(EAnimClipEventType.RollEvent);
        }

        public void PlayAudio()
        {
            OnTriggerAnimClipEvent(EAnimClipEventType.PlayAudio);
        }

        public void Foot1()
        {
            OnTriggerAnimClipEvent(EAnimClipEventType.Foot1);
        }

        public void Foot2()
        {
            OnTriggerAnimClipEvent(EAnimClipEventType.Foot2);
        }

        public void Foot3()
        {
            OnTriggerAnimClipEvent(EAnimClipEventType.Foot3);
        }

        public void Foot4()
        {
            OnTriggerAnimClipEvent(EAnimClipEventType.Foot4);
        }

        void OnTriggerAnimClipEvent(EAnimClipEventType clipEventType)
        {
            //Debug.Log($"OnTriggerAnimClipEvent:{clipEventType}");
        }

        #endregion
    }
}