using CombatEditor;
using UnityEngine;

namespace Saber.CharacterController
{
    public class SkillState : ActorStateBase
    {
        public BaseSkill CurSkill => Actor.CurrentSkill;

        public override bool ApplyRootMotionSetWhenEnter => true;
        public override bool CanExit => CurSkill.CanExit;
        public override bool CanEnter => true;

        public SkillState() : base(EStateType.Skill)
        {
        }

        public override void Enter()
        {
            base.Enter();
            CurSkill.Enter();
            Actor.CAnim.StopMaskLayerAnims();
            Actor.CPhysic.EnableSlopeMovement = false;

            Actor.CurrentResilience = CurSkill.SkillConfig.resilience;

            if (CurSkill.SkillConfig.m_TriggerCondition == ETriggerCondition.InAir)
            {
                Actor.CPhysic.EnablePlatformMovement = false;
                Actor.CPhysic.SetPlatform(null);
            }
        }

        public override void OnStay()
        {
            base.OnStay();
            CurSkill.OnStay();
            if (!CurSkill.IsTriggering)
            {
                Exit();
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            CurSkill.Exit();
            Actor.CPhysic.EnableSlopeMovement = true;
            Actor.CPhysic.EnablePlatformMovement = true;
            Actor.DefaultResilience();
        }

        public override void OnAnimEnter(int nameHash, int layer)
        {
            base.OnAnimEnter(nameHash, layer);
            CurSkill.OnAnimEnter(nameHash, layer);
        }

        public override void OnAnimExit(int nameHash, int layer)
        {
            base.OnAnimExit(nameHash, layer);
            CurSkill.OnAnimExit(nameHash, layer);
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            CurSkill.OnTriggerAnimEvent(eventObj);
        }

        public override void OnTriggerRangeEvent(AnimRangeTimeEvent eventObj, bool enter)
        {
            base.OnTriggerRangeEvent(eventObj, enter);
            CurSkill.OnTriggerRangeEvent(eventObj, enter);
        }
    }
}