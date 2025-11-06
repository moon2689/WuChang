using System;
using System.Collections.Generic;
using System.Linq;
using CombatEditor;
using UnityEngine;
using Saber.Config;
using Saber.Frame;
using Saber.AI;
using YooAsset;

namespace Saber.CharacterController
{
    public abstract class SActor : MonoBehaviour,
        AnimatorLayer.IHandler,
        ActorBaseStats.IHandler,
        PlayerCamera.ITarget,
        IDamageMaker
    {
        public event Action<SActor> Event_OnDead;
        public event Action<SActor> Event_OnDeadAnimPlayFinished;
        public event Action<SActor, float> Event_OnDamage;


        [SerializeField] private List<CharacterNode> m_Nodes;

        private float m_TimeMultiplier = 1;
        private BaseAI m_AI;
        private Vector3 m_MovementAxis;
        protected EMoveSpeedV m_MoveSpeedV;
        public AnimSpeedExecutor m_AnimSpeedExecutor;
        private List<AudioPlayer> m_PlayingSounds = new();
        private bool m_IsDead;
        private Dictionary<ENodeType, Transform> m_DicNodes;


        public abstract BaseActorInfo m_BaseActorInfo { get; }
        public abstract ActorStateMachine CStateMachine { get; }

        public StatsInfo StatsInfo => m_BaseActorInfo.m_StatsInfo;
        public PhysicInfo PhysicInfo => m_BaseActorInfo.m_PhysicInfo;
        public List<CharacterNode> Nodes => m_Nodes;
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

        public ActorBuff CBuff { get; protected set; }

        /// <summary>能力</summary>
        public CharacterAbility CAbility { get; protected set; }

        /// <summary> The current value of the Delta time the animal is using (Fixed or not)</summary>
        public float DeltaTime { get; protected set; }

        public float TimeMultiplier
        {
            get => m_TimeMultiplier;
            set
            {
                m_TimeMultiplier = value;
                //Debug.Log($"time:{value}");
            }
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
            set => m_IsDead = value;
        }

        //public SkillAnimEvent CurActivingSkillAnimState { get; set; }
        public EActorCamp Camp { get; set; }
        public bool Invincible { get; set; }
        public bool AddYuMaoWhenHitted { get; set; }

        public HurtBox[] HurtBoxes => GetComponentsInChildren<HurtBox>();

        public bool IsPlayer
        {
            get => BaseInfo.m_ActorType == EActorType.Player;
        }

        public EStateType CurrentStateType => CStateMachine.CurrentStateType;
        public bool UpdateMovementAxisAnimatorParams { get; set; } = true;
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
        public bool CanBeExecuted
        {
            get
            {
                if (CurrentStateType == EStateType.GetHit && this.CStateMachine.CurrentState is IHitRecovery hitrec)
                {
                    return hitrec.CanBeExecute;
                }

                return false;
            }
        }

        /// <summary>可被处决</summary>
        public bool IsBlockBrokenWaitExecute
        {
            get
            {
                if (CurrentStateType == EStateType.GetHit && this.CStateMachine.CurrentState is IHitRecovery hitrec)
                {
                    return hitrec.IsBlockBrokenWaitExecute;
                }

                return false;
            }
        }

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

        public EResilience CurrentResilience { get; set; }

        public bool IsInSpecialStun
        {
            get
            {
                if (CurrentStateType == EStateType.GetHit)
                {
                    var getHit = CStateMachine.CurrentState as GetHit;
                    return getHit != null &&
                           (getHit.HitRecHurtType == GetHit.EHitRecHurtType.SpecialStun ||
                            getHit.HitRecHurtType == GetHit.EHitRecHurtType.BlockBroken);
                }

                return false;
            }
        }


        public static AssetHandle Create(int id, Vector3 pos, Quaternion rot, BaseAI ai, EActorCamp camp, Action<SActor> onCreated)
        {
            var ownerInfo = GameApp.Entry.Config.ActorInfo.m_Actors.FirstOrDefault(a => a.m_ID == id);
            if (ownerInfo == null)
            {
                Debug.LogError("ownerInfo == null,id:" + id);
                return null;
            }

            return Create(ownerInfo, pos, rot, actor =>
            {
                actor.Camp = camp;
                actor.AI = ai;
                onCreated?.Invoke(actor);
            });
        }

        public static AssetHandle Create(int id, Vector3 pos, Quaternion rot, Action<SActor> onCreated)
        {
            var ownerInfo = GameApp.Entry.Config.ActorInfo.m_Actors.FirstOrDefault(a => a.m_ID == id);
            if (ownerInfo == null)
            {
                Debug.LogError("ownerInfo == null,id:" + id);
                return null;
            }

            return Create(ownerInfo, pos, rot, onCreated);
        }

        static AssetHandle Create(ActorItemInfo config, Vector3 pos, Quaternion rot, Action<SActor> onCreated)
        {
            return config.LoadGameObject(playerGO =>
            {
                playerGO.transform.position = pos;
                playerGO.transform.rotation = rot;
                SActor actor = playerGO.GetComponent<SActor>();
                actor.name = config.m_PrefabName;
                actor.BaseInfo = config;
                onCreated?.Invoke(actor);
            });
        }

        protected virtual void Awake()
        {
            gameObject.SetLayerRecursive(EStaticLayers.Actor);
            gameObject.SetRenderingLayerRecursive(ERenderingLayers.Actor);

            CBuff = new(this);
            CAnim = new CharacterAnimation(this, this);
            CPhysic = new CharacterPhysic(this, m_BaseActorInfo.m_PhysicInfo);
            CStats = new ActorBaseStats(this);
            CMelee = new CharacterMelee(this, m_BaseActorInfo.m_SkillConfig);
            CMelee.SetWeapon(m_BaseActorInfo.m_WeaponPrefabs);

            DefaultResilience();
        }

        public void DefaultResilience()
        {
            CurrentResilience = m_BaseActorInfo.m_StatsInfo.m_Resilience;
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
            if (IsDead)
            {
                return;
            }
            CBuff?.Update(Time.deltaTime);
            CStats?.Update(Time.deltaTime);
            AI?.Update();

            if (m_AnimSpeedExecutor == null)
                m_AnimSpeedExecutor = new AnimSpeedExecutor(this);
            m_AnimSpeedExecutor.Execute();

            ActorSoundFollowAnim();
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

        public void OnAnimFootStep()
        {
            var sounds = GameApp.Entry.Config.GameSetting.m_SoundFootStepGround;
            var clip = sounds[UnityEngine.Random.Range(0, sounds.Length)];
            GameApp.Entry.Game.Audio.Play3DSound(clip, transform.position);
        }

        #endregion

        public virtual void ToggleTrailShadowEffect(bool show)
        {
        }

        public virtual void SetGroundOffset(float offset)
        {
            m_BaseActorInfo.m_PhysicInfo.m_GroundOffset = offset;
        }

        public Transform GetNodeTransform(ENodeType nodeType)
        {
            if (m_DicNodes == null)
            {
                m_DicNodes = new();
                foreach (var n in m_Nodes)
                {
                    m_DicNodes.Add(n.m_Type, n.m_NodeTrans);
                }
            }

            m_DicNodes.TryGetValue(nodeType, out var t);
            if (t == null)
            {
                Debug.LogError($"Node is null,type:{nodeType}");
            }

            return t;
        }

        public virtual void OnPlayDamageEffect(Vector3 pos)
        {
        }

        /// <summary>恢复原状</summary>
        public virtual void RecoverOrigin()
        {
            IsDead = false;

            gameObject.SetActive(true);
            CStats.RecoverOrigin();

            StopMove();
            CStateMachine.ForceEnterState(EStateType.Idle);

            AI?.Init(this);

            CMelee.CWeapon.ShowOrHideWeapon(true);
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

        public bool Die(string specialAnim = null)
        {
            return CStateMachine.Die(specialAnim);
        }

        public bool Dodge(Vector3 axis)
        {
            return CStateMachine.Dodge(axis);
        }

        public bool Dodge()
        {
            return CStateMachine.Dodge(MovementAxis);
        }

        public void OnParried(SActor defenser)
        {
            CStateMachine.OnParried(defenser);
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

        public virtual bool DrinkPotion()
        {
            return false;
        }


        /// <summary>被处决</summary>
        public bool BeExecute(SActor executioner)
        {
            return CStateMachine.BeExecute(executioner);
        }

        public virtual void OnShenKanRest()
        {
            Heal(CStats.MaxHp);
            CStats.DefaultHPPointCount();
            CStats.ClearPower();
        }

        public bool PlayAction(PlayActionState.EActionType action, string animName, Action onPlayFinished)
        {
            return CStateMachine.PlayAction(action, animName, onPlayFinished);
        }

        public bool PlayAction(PlayActionState.EActionType action, Action onPlayFinished)
        {
            return PlayAction(action, null, onPlayFinished);
        }

        public bool PlayAction(PlayActionState.EActionType action)
        {
            return PlayAction(action, null, null);
        }

        #endregion


        #region PlayerCamera.ITarget

        Vector3 PlayerCamera.ITarget.Position => transform.position;

        Vector3 PlayerCamera.ITarget.LockPosition
        {
            get
            {
                Transform lockUINode = GetNodeTransform(ENodeType.LockUIPos);
                return lockUINode ? lockUINode.position : transform.position;
            }
        }

        Quaternion PlayerCamera.ITarget.Rotation => transform.rotation;
        float PlayerCamera.ITarget.Height => CPhysic.Height;

        bool PlayerCamera.ITarget.IsMoving => CurrentStateType == EStateType.Move || CurrentStateType == EStateType.Dodge;

        #endregion


        #region ActorBaseStats.IHandler

        public virtual void OnHpChange(float curHp)
        {
            if (curHp <= 0)
            {
                if (GameApp.Entry.Config.TestGame.DebugFight)
                {
                    GameApp.Entry.Unity.DoDelayAction(3, CStats.RecoverOrigin);
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

        void ActorSoundFollowAnim()
        {
            for (int i = 0; i < m_PlayingSounds.Count; i++)
            {
                var p = m_PlayingSounds[i];
                if (p != null)
                {
                    p.AudioSource.pitch = CAnim.AnimatorObj.speed;
                }
            }
        }

        public virtual void AddYuMao(int value)
        {
        }

        public virtual void CostYuMao(int value)
        {
        }

        public bool Heal(float value)
        {
            if (IsDead || value <= 0)
            {
                return false;
            }

            if (!CStats.IsHPFull)
            {
                GameApp.Entry.Game.Effect.CreateEffect("Particles/Healing", transform, 5);
                CStats.AddHp(value);
            }

            return true;
        }

        public void OnDieAnimPlayFinished()
        {
            Event_OnDeadAnimPlayFinished?.Invoke(this);
        }
    }
}