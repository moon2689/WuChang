using System.Collections.Generic;
using Saber.CharacterController;
using Saber.Frame;
using Unity.Mathematics;
using UnityEngine;

//Replace the "MagicCounter" with the event you want to create
//If you want to create a object with handle in preview, please inherit the "AbilityEventObj" with "AbilityEventObj_CreateObjWithHandle"

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / MagicCounter")]
    public class AbilityEventObj_MagicCounter : AbilityEventObj
    {
        //Write the data you need here.
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventTime;
        }

        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_MagicCounter(this);
        }
    }

//Write you logic here
    public partial class AbilityEventEffect_MagicCounter : AbilityEventEffect
    {
        private List<SActor> m_ParriedEnemies = new();

        public override void StartEffect()
        {
            base.StartEffect();
            if (TryParry(out var parriedEnemies))
            {
                var config = GameApp.Entry.Config.SkillDecapitateConfig;
                AudioClip sound = config.m_MagicCounterHitSound;
                GameApp.Entry.Game.Audio.Play3DSound(sound, Actor.transform.position);
                foreach (var e in parriedEnemies)
                {
                    e.BeWeek();

                    GameObject effect = config.m_EffectMagicCounterSuccess;
                    Quaternion rot = Quaternion.identity;
                    Vector3 pos = e.transform.position + Vector3.up * Actor.CPhysic.Height * 0.8f;
                    GameApp.Entry.Game.Effect.CreateEffect(effect, e.transform, pos, rot, 3f);
                }
            }
        }

        /// <summary>尝试弹反</summary>
        bool TryParry(out List<SActor> parriedEnemies)
        {
            m_ParriedEnemies.Clear();
            Collider[] colliders = new Collider[10];
            float radius = 10f;
            int layerMask = EStaticLayers.Actor.GetLayerMask();
            int count = Physics.OverlapSphereNonAlloc(Actor.transform.position, radius, colliders, layerMask);
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
    }

    public partial class AbilityEventEffect_MagicCounter : AbilityEventEffect
    {
        private AbilityEventObj_MagicCounter EventObj { get; set; }

        public AbilityEventEffect_MagicCounter(AbilityEventObj initObj) : base(initObj)
        {
            m_EventObj = initObj;
            EventObj = (AbilityEventObj_MagicCounter)initObj;
        }
    }
}