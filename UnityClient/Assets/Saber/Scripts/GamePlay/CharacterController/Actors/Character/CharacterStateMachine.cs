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
            return TryEnterState<Dodge>(EStateType.Dodge, state =>
            {
                Actor.Invincible = true;
                bool perfectDodge = TryPerfectDodge();
                if (perfectDodge)
                {
                    state.OnPerfectDodge();
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
            return TryEnterState(EStateType.Defense);
            // return TryEnterState<Defense>(EStateType.Defense, null);
        }

        public override bool DefenseEnd()
        {
            if (CurrentStateType == EStateType.Defense)
            {
                ((Defense)CurrentState).TryEndDefense();
            }

            return true;
        }

        public override bool PlayAction_IdolActive(Action onPlayed)
        {
            if (CurrentStateType == EStateType.Idle)
            {
                Idle idle = GetState<Idle>(EStateType.Idle);
                idle.IdolActive(onPlayed);
                return true;
            }

            return false;
        }

        public override bool PlayAction_IdolRest(Action onPlayFinish)
        {
            if (CurrentStateType == EStateType.Idle)
            {
                Idle idle = GetState<Idle>(EStateType.Idle);
                idle.IdolRest(onPlayFinish);
                return true;
            }

            return false;
        }

        public override bool PlayAction_IdolRestEnd(Action onPlayFinish)
        {
            if (CurrentStateType == EStateType.Idle)
            {
                Idle idle = GetState<Idle>(EStateType.Idle);
                idle.IdolRestEnd(onPlayFinish);
                return true;
            }

            return false;
        }

        public override bool PlayAction_BranchTeleport(Action onPlayFinish)
        {
            if (CurrentStateType == EStateType.Idle)
            {
                Idle idle = GetState<Idle>(EStateType.Idle);
                idle.BranchTeleport(onPlayFinish);
                return true;
            }

            return false;
        }

        public override bool PlayAction_GoHome(Action onPlayFinish)
        {
            if (CurrentStateType == EStateType.Idle)
            {
                Idle idle = GetState<Idle>(EStateType.Idle);
                idle.GoHome(onPlayFinish);
                return true;
            }

            return false;
        }

        public override bool OnHit(DamageInfo dmgInfo)
        {
            return TryEnterState<GetHit>(EStateType.GetHit, state => state.Damage = dmgInfo);
        }

        public override bool SetPosAndForward(Vector3 tarPos, Vector3 forward, Action onFinished)
        {
            if (CurrentStateType == EStateType.Idle)
            {
                Idle idle = GetState<Idle>(EStateType.Idle);
                idle.SetPosAndForward(tarPos, forward, onFinished);
                return true;
            }

            return false;
        }
    }
}