using System;
using System.Collections.Generic;
using System.Linq;
using CombatEditor;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>通用技能</summary>
    public class SkillCommon : BaseSkill
    {
        private Dictionary<int, List<AbilityEventWithEffects>> m_DicEventWithEffects;
        private int? m_CurAnimStateID;


        public override bool InPerfectDodgeTime => PerfectDodgeData != null;
        public override bool InTanDaoTime => TanDaoData != null;
        public AbilityEventObj_PerfectDodge PerfectDodgeData { get; set; }
        public AbilityEventObj_TanDao TanDaoData { get; set; }


        public SkillCommon(SActor actor, SkillItem skillConfig) : base(actor, skillConfig)
        {
            InitAnimEventAndEffects();
        }

        void InitAnimEventAndEffects()
        {
            m_DicEventWithEffects = new();
            bool hasEventData = false;
            foreach (var animState in SkillConfig.m_AnimStates)
            {
                if (animState.m_EventData == null)
                {
                    continue;
                }

                hasEventData = true;
                List<AbilityEventWithEffects> list = new();
                m_DicEventWithEffects.Add(animState.m_Name.GetAnimatorHash(), list);
                foreach (var eve in animState.m_EventData.events)
                {
                    AbilityEventEffect abilityEventEffect = eve.Obj.Initialize();
                    abilityEventEffect.AbilityEvent = eve;
                    abilityEventEffect.CurrentSkill = this;
                    abilityEventEffect.AnimObj = animState.m_EventData;

                    AbilityEventWithEffects eveWithEffects = new AbilityEventWithEffects();
                    eveWithEffects.eve = eve;
                    eveWithEffects.effect = abilityEventEffect;
                    list.Add(eveWithEffects);
                }
            }

            if (!hasEventData)
            {
                Debug.LogError($"None event data, id:{SkillConfig.m_ID},player:{Actor.name}");
            }
        }

        /// <summary>Running the target effects on animation clip.</summary>
        void RunningEvents(List<AbilityEventWithEffects> abilityEventWithEffects, float normalizedTime)
        {
            for (int j = 0; j < abilityEventWithEffects.Count; j++)
            {
                var eve = abilityEventWithEffects[j];
                if (!eve.effect.m_EventObj.IsActive)
                    continue;

                var StartTime = eve.eve.GetEventStartTime();
                var EndTime = eve.eve.GetEventEndTime();
                var EveTimeType = eve.eve.GetEventTimeType();

                if (EveTimeType == AbilityEventObj.EventTimeType.EventTime)
                {
                    if (!eve.effect.IsRunning && normalizedTime >= StartTime)
                        eve.effect.StartEffect();
                }

                if (EveTimeType == AbilityEventObj.EventTimeType.EventRange ||
                    EveTimeType == AbilityEventObj.EventTimeType.EventMultiRange)
                {
                    //Start Even if the start frame is jumpped 
                    //  StartTime < CurrentTime < EndTime
                    if (normalizedTime < EndTime && normalizedTime >= StartTime)
                    {
                        if (!eve.effect.IsRunning)
                            eve.effect.StartEffect();

                        if (eve.effect.IsRunning)
                            eve.effect.EffectRunning(normalizedTime);
                    }

                    //If self to self translation, events can't close itself cause clip is not change. So it need to close itself by percentage.
                    if (normalizedTime >= EndTime || normalizedTime < StartTime)
                    {
                        if (eve.effect.IsRunning)
                            eve.effect.TryEnd();
                    }
                }
            }
        }


        public override void Enter()
        {
            m_CurAnimStateID = null;
            PerfectDodgeData = null;

            base.Enter();
        }

        protected override void PlayAnimOnEnter(string firstAnim, string endAnim)
        {
            base.PlayAnimOnEnter(firstAnim, endAnim);
            OnAnimEnter(firstAnim.GetAnimatorHash(), 0); //当切换到同一动画时，这里需要手动调用，否则此方法不会调用
        }

        public override void OnStay()
        {
            if (m_CurAnimStateID != null &&
                m_DicEventWithEffects.TryGetValue(m_CurAnimStateID.Value, out var abilityEventWithEffects))
            {
                float normalizeTime = Actor.CAnim.GetAnimNormalizedTime(0);
                RunningEvents(abilityEventWithEffects, normalizeTime);
            }

            base.OnStay();
        }

        public override void OnAnimEnter(int nameHash, int layer)
        {
            if (layer != 0)
                return;

            if (m_DicEventWithEffects.ContainsKey(nameHash))
            {
                m_CurAnimStateID = nameHash;
                //Debug.Log("skill enter:" + m_CurAnimStateID.Value);
            }
        }

        // End last frame events if needed.
        private void TryEndLastEvents()
        {
            if (m_CurAnimStateID != null)
            {
                m_DicEventWithEffects.TryGetValue(m_CurAnimStateID.Value, out var abilityEventWithEffects);
                for (int i = 0; i < abilityEventWithEffects.Count; i++)
                {
                    abilityEventWithEffects[i].effect.TryEnd();
                }
                
                m_CurAnimStateID = null;
            }
        }

        public override void OnAnimExit(int nameHash, int layer)
        {
            if (layer != 0)
                return;

            // End last frame events if needed.
            TryEndLastEvents();

            base.OnAnimExit(nameHash, layer);
        }

        public override void Exit()
        {
            if (!IsTriggering)
            {
                return;
            }

            base.Exit();

            PerfectDodgeData = null;

            // End last frame events if needed.
            TryEndLastEvents();

            // release events
            foreach (var pair in m_DicEventWithEffects)
            {
                foreach (var pair2 in pair.Value)
                    pair2.effect.Release();
            }
        }

        /// <summary>在完美闪避范围内</summary>
        public override bool InPerfectDodgeRange(SActor target)
        {
            // TODO 这里只处理了球形
            Transform node = Actor.GetNodeTransform(PerfectDodgeData.ObjData.TargetNode);
            Vector3 pos = node.position + node.rotation * PerfectDodgeData.ObjData.Offset + PerfectDodgeData.ColliderOffset;
            float radius = PerfectDodgeData.Radius;
            int layerMask = EStaticLayers.Actor.GetLayerMask();
            Collider[] colliders = Physics.OverlapSphere(pos, radius, layerMask, QueryTriggerInteraction.Ignore);

            SDebug.DrawWireSphere(pos, Color.green, radius, 3f);

            foreach (var col in colliders)
            {
                if (col.gameObject == target.gameObject)
                    return true;
            }

            return false;
        }
        
        /// <summary>在完美闪避范围内</summary>
        public override bool InTanDaoRange(SActor target)
        {
            // TODO 这里只处理了球形
            Transform node = Actor.GetNodeTransform(TanDaoData.ObjData.TargetNode);
            Vector3 pos = node.position + node.rotation * TanDaoData.ObjData.Offset + TanDaoData.ColliderOffset;
            float radius = TanDaoData.Radius;
            int layerMask = EStaticLayers.Actor.GetLayerMask();
            Collider[] colliders = Physics.OverlapSphere(pos, radius, layerMask, QueryTriggerInteraction.Ignore);

            SDebug.DrawWireSphere(pos, Color.green, radius, 3f);

            foreach (var col in colliders)
            {
                if (col.gameObject == target.gameObject)
                    return true;
            }

            return false;
        }
    }
}