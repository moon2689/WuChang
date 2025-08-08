using System;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Weak : ActorStateBase
    {
        public enum EWeakType
        {
            Weak,
            BeDecapitate,
            BeBackStab,
        }

        static string[] s_LieOnGroundAnims =
        {
            "GetHitDownByBackward",
            "GetHitDownByForward",
            "GetHitFlyByBackward",
            "GetHitFlyByForward",
            "BeBackStab",
            "BeDecapitate",
        };

        private string m_CurAnim;

        public EWeakType WeakType { get; set; }


        public static bool IsLieOnGround(SActor actor)
        {
            for (int i = 0; i < s_LieOnGroundAnims.Length; i++)
            {
                string anim = s_LieOnGroundAnims[i];
                if (actor.CAnim.IsPlayingOrWillPlay(anim) && actor.CAnim.GetAnimNormalizedTime(anim) > 0.9f)
                {
                    return true;
                }
            }

            return false;
        }


        public Weak() : base(EStateType.Weak)
        {
        }

        public override void Enter()
        {
            base.Enter();
            OnEnter();
        }

        public override void ReEnter()
        {
            base.ReEnter();
            OnEnter();
        }

        void OnEnter()
        {
            Actor.CanBeDecapitate = false;

            m_CurAnim = WeakType switch
            {
                EWeakType.Weak => "Weak",
                EWeakType.BeDecapitate => "BeDecapitate",
                EWeakType.BeBackStab => "BeBackStab",
                _ => throw new InvalidOperationException($"Unknown state:{WeakType}"),
            };

            Actor.CAnim.Play(m_CurAnim);
        }

        public override void OnTriggerRangeEvent(AnimRangeTimeEvent eventObj, bool enter)
        {
            base.OnTriggerRangeEvent(eventObj, enter);
            if (eventObj.EventType == EAnimRangeEvent.CanBeDecapitate)
            {
                Actor.CanBeDecapitate = enter;
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            Actor.CanBeDecapitate = false;
        }

        public override void OnStay()
        {
            base.OnStay();
            if (Actor.IsDead)
            {
                if (IsLieOnGround(Actor))
                {
                    base.Exit();
                    return;
                }
            }

            if (Actor.CAnim.IsReallyPlaying("Idle"))
            {
                base.Exit();
                return;
            }
        }
    }
}