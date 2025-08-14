using System;
using System.Collections.Generic;
using System.Linq;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class CharacterStateMachine : ActorStateMachine
    {
        private SCharacter m_Character;
        private List<SActor> m_ParriedEnemies = new();


        public CharacterStateMachine(SCharacter actor) : base(actor)
        {
            m_Character = actor;
        }

        protected override void RegisterStates()
        {
            base.RegisterState(new Idle());
            base.RegisterState(new Move());
            base.RegisterState(new Fall());
            base.RegisterState(new Die());
            //base.RegisterState(new Jump());
            base.RegisterState(new Dodge());
            base.RegisterState(new SkillState());
            base.RegisterState(new GetHit());
            base.RegisterState(new Defense());
            base.RegisterState(new UseItem());
            //base.RegisterState(new Glide());
            //base.RegisterState(new Slide());
            //base.RegisterState(new Climb());
            //base.RegisterState(new Swim());
            //base.RegisterState(new WallRun());
            //base.RegisterState(new Fly());
        }

        /*
        public override void Update()
        {
            if (m_Character.CRender.IsInWater && m_Character.CurrentStateType != EStateType.Swim)
            {
                TryEnterState(EStateType.Swim);
            }
            else if (Actor.CPhysic.GroundDistance > WallRun.StartHeight)
            {
                TryEnterState(EStateType.WallRun);
            }

            TryEnterState(EStateType.Slide);
            base.Update();
        }

        public bool Fly()
        {
            return TryEnterState(EStateType.Fly);
        }


        public void StopFly()
        {
            if (CurrentStateType == EStateType.Fly)
            {
                Fly fly = (Fly)CurrentState;
                fly.StopFly();
            }
        }

        public override bool ToggleFly()
        {
            bool isFlying = CurrentStateType == EStateType.Fly;
            if (isFlying)
            {
                Fly fly = (Fly)CurrentState;
                isFlying = fly.IsFlying;
            }

            if (isFlying)
            {
                StopFly();
                return true;
            }
            else
            {
                return Fly();
            }
        }


        public override bool Slide()
        {
            bool entered = false;
            float slopeLimit = 10;
            if (Actor.CPhysic.SlopeDirectionAngle > slopeLimit)
            {
                Actor.CPhysic.SetSlopeLimit(slopeLimit);
                entered = TryEnterState(EStateType.Slide);
                // if (!entered)
                //     Actor.CPhysic.DefaultSlopeLimit();
            }

            return entered;
        }
        
        public override bool Climb()
        {
            return TryEnterState(EStateType.Climb);
        }

        public override bool StopClimb()
        {
            if (CurrentStateType == EStateType.Climb)
            {
                Climb climb = GetState<Climb>(EStateType.Climb);
                climb.EndClimb();
                return true;
            }

            return false;
        }

        public bool Glide()
        {
            return TryEnterState(EStateType.Glide);
        }

        public void StopGlide()
        {
            if (CurrentStateType == EStateType.Glide)
            {
                Glide glide = (Glide)CurrentState;
                glide.StopGlide();
            }
        }

        public override bool ToggleGlide()
        {
            bool isGliding = CurrentStateType == EStateType.Glide;
            if (isGliding)
            {
                Glide glide = (Glide)CurrentState;
                isGliding = glide.IsGliding;
            }

            if (isGliding)
            {
                StopGlide();
                return true;
            }
            else
            {
                return Glide();
            }
        }

        public override bool Jump(Vector3 axis)
        {
            if (CurrentStateType == EStateType.Jump)
            {
                Jump jump = GetState<Jump>(EStateType.Jump);
                return jump.DoubleJump();
            }

            return TryEnterState(EStateType.Jump);
        }
        */

        public override bool Fall(bool playFallAnim = true)
        {
            return TryEnterState<Fall>(EStateType.Fall, state => state.PlayFallAnim = playFallAnim);
        }

        public override void ForceFall(bool playFallAnim = true)
        {
            GetState<Fall>(EStateType.Fall).PlayFallAnim = playFallAnim;
            ForceEnterState(EStateType.Fall);
        }

        public override bool Dodge(Vector3 axis)
        {
            /*
            if (CurrentStateType == EStateType.Jump)
            {
                Jump jump = GetState<Jump>(EStateType.Jump);
                return jump.Dodge();
            }
            */

            return TryEnterState<Dodge>(EStateType.Dodge, state =>
            {
                bool perfectDodge = TryPerfectDodge();
                if (perfectDodge)
                {
                    m_Character.CRender.ShowCharacterShadowEffect();
                    Actor.Invincible = true;
                    GameApp.Entry.Game.Audio.Play3DSound("Sound/Skill/PerfectDodge", Actor.transform.position);
                }

                state.DodgeAxis = axis;
            });
        }

        /// <summary>尝试完美闪避</summary>
        bool TryPerfectDodge()
        {
            Collider[] colliders = new Collider[10];
            float radius = 10f;
            int count = Physics.OverlapSphereNonAlloc(Actor.transform.position, radius, colliders,
                EStaticLayers.Actor.GetLayerMask());
            for (int i = 0; i < count; i++)
            {
                Collider tar = colliders[i];

                // 是否正在释放技能
                SActor enemy = tar.GetComponent<SActor>();
                if (enemy != null && DamageHelper.CanDamageEnemy(enemy, Actor))
                {
                    bool perfectDodge = enemy.CurrentStateType == EStateType.Skill &&
                                        enemy.CurrentSkill != null &&
                                        enemy.CurrentSkill.InPerfectDodgeTime &&
                                        enemy.CurrentSkill.InPerfectDodgeRange(Actor);

                    if (perfectDodge)
                    {
                        return true;
                    }
                }

                // 是否是抛射物
                Projectile projectile = tar.GetComponent<Projectile>();
                if (projectile != null &&
                    projectile.IsFlying(out enemy) &&
                    DamageHelper.CanDamageEnemy(enemy, Actor))
                {
                    bool perfectDodge = projectile.InPerfectDodgeRange(Actor);
                    if (perfectDodge)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool DefenseStart()
        {
            //return TryEnterState(EStateType.Defense);
            return TryEnterState<Defense>(EStateType.Defense, state =>
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
                }

                state.ParriedSucceed = parriedSucceed;
            });
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
                                      enemy.CurrentSkill.InPerfectDodgeTime &&
                                      enemy.CurrentSkill.InPerfectDodgeRange(Actor);

                if (parriedSucceed)
                {
                    m_ParriedEnemies.Add(enemy);
                }
            }

            parriedEnemies = m_ParriedEnemies;
            return m_ParriedEnemies.Count > 0;
        }

        public override bool DefenseEnd()
        {
            if (CurrentStateType == EStateType.Defense)
            {
                if (CurrentState.CanExit)
                {
                    ((Defense)CurrentState).EndDefense();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public override bool PlayAction_BranchRepair(Action onPlayed)
        {
            if (CurrentStateType == EStateType.Idle)
            {
                Idle idle = GetState<Idle>(EStateType.Idle);
                idle.BranchRepair(onPlayed);
                return true;
            }

            return false;
        }

        public override bool PlayAction_BranchRest()
        {
            if (CurrentStateType == EStateType.Idle)
            {
                Idle idle = GetState<Idle>(EStateType.Idle);
                idle.BranchRest();
                return true;
            }

            return false;
        }

        public override bool PlayAction_BranchRestEnd()
        {
            if (CurrentStateType == EStateType.Idle)
            {
                Idle idle = GetState<Idle>(EStateType.Idle);
                idle.BranchRestEnd();
                return true;
            }

            return false;
        }

        public override bool PlayAction_BranchTeleport()
        {
            if (CurrentStateType == EStateType.Idle)
            {
                Idle idle = GetState<Idle>(EStateType.Idle);
                idle.BranchTeleport();
                return true;
            }

            return false;
        }

        public override bool PlayAction_GoHome()
        {
            if (CurrentStateType == EStateType.Idle)
            {
                Idle idle = GetState<Idle>(EStateType.Idle);
                idle.GoHome();
                return true;
            }

            return false;
        }

        public override bool OnHit(DamageInfo dmgInfo)
        {
            return TryEnterState<GetHit>(EStateType.GetHit, state => state.Damage = dmgInfo);
        }

        public override bool UseItem(UseItem.EItemType itemType)
        {
            return TryEnterState<UseItem>(EStateType.UseItem, state => state.ItemType = itemType);
        }
    }
}