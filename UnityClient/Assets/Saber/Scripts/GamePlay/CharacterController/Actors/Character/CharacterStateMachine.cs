using System;
using System.Collections;
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
            base.RegisterState(new Dodge());
            base.RegisterState(new SkillState());
            base.RegisterState(new GetHit());
            base.RegisterState(new Defense());
            base.RegisterState(new PlayActionState());
        }

        public override bool Fall(bool playFallAnim = true)
        {
            return TryEnterState<Fall>(EStateType.Fall, state => state.PlayFallAnim = playFallAnim);
        }

        public override void ForceFall(bool playFallAnim = true)
        {
            GetState<Fall>(EStateType.Fall).PlayFallAnim = playFallAnim;
            ForceEnterState(EStateType.Fall);
        }

        /*
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
        */

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

        public override bool SetPosAndForward(Vector3 tarPos, Vector3 forward, Action onFinished)
        {
            return TryEnterState<PlayActionState>(EStateType.PlayAction,
                state => state.SetPosAndForward(tarPos, forward, onFinished));
        }
    }
}