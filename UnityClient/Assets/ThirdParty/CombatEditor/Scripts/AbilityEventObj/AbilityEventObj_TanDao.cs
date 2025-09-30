using System.Collections.Generic;
using Saber;
using Saber.CharacterController;
using UnityEngine;

//Replace the "TanDao" with the event you want to create
//If you want to create a object with handle in preview, please inherit the "AbilityEventObj" with "AbilityEventObj_CreateObjWithHandle"

namespace CombatEditor
{
    //[AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / TanDao")]
    public class AbilityEventObj_TanDao : AbilityEventObj_CreateObjWithHandle
    {
        public Vector3 ColliderOffset = new Vector3(0, 0, 0);
        public Vector3 ColliderSize = new Vector3(1, 1, 1);
        public float Radius = 1;
        public float Height = 1;

        //Write the data you need here.
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventTime;
        }

        public override AbilityEventEffect Initialize()
        {
            ObjData.PreloadObjects();
            return new AbilityEventEffect_TanDao(this);
        }

#if UNITY_EDITOR
        public override AbilityEventPreview InitializePreview()
        {
            return new AbilityEventPreview_TanDao(this);
        }
#endif
    }

    //Write you logic here
    public partial class AbilityEventEffect_TanDao : AbilityEventEffect
    {
        public static bool WhetherBeparried(SActor defenser, SActor attacker, out Defense defenseState)
        {
            defenseState = null;
            if (defenser != null && defenser != attacker && !defenser.IsDead && defenser.Camp != attacker.Camp &&
                defenser.CurrentStateType == EStateType.Defense)
            {
                defenseState = (Defense)defenser.CStateMachine.CurrentState;
                if (!defenseState.InTanFanTime)
                {
                    return false;
                }

                Vector3 dirToMe = attacker.transform.position - defenser.transform.position;
                if (Vector3.Dot(dirToMe, defenser.transform.forward) > 0)
                    return true;
            }

            return false;
        }

        /// <summary>尝试弹反</summary>
        bool WhetherBeParried(out Defense defenseState)
        {
            // TODO 这里只处理了球形
            Transform node = Actor.GetNodeTransform(EventObj.ObjData.TargetNode);
            Vector3 pos = node.position + node.rotation * EventObj.ObjData.Offset + EventObj.ColliderOffset;
            float radius = EventObj.Radius;
            int layerMask = EStaticLayers.Actor.GetLayerMask();
            Collider[] colliders = Physics.OverlapSphere(pos, radius, layerMask, QueryTriggerInteraction.Ignore);

            SDebug.DrawWireSphere(pos, Color.green, radius, 3f);

            foreach (var col in colliders)
            {
                var enemy = col.GetComponent<SActor>();
                /*
                if (enemy != null && enemy != Actor && !enemy.IsDead && enemy.Camp != Actor.Camp &&
                    enemy.CurrentStateType == EStateType.Defense)
                {
                    defenseState = (Defense)enemy.CStateMachine.CurrentState;
                    if (!defenseState.InTanFanTime)
                    {
                        continue;
                    }

                    Vector3 dirToMe = Actor.transform.position - enemy.transform.position;
                    if (Vector3.Dot(dirToMe, enemy.transform.forward) > 0)
                        return true;
                }
                */
                bool beParried = WhetherBeparried(enemy, Actor, out defenseState);
                if (beParried)
                {
                    return true;
                }
            }

            defenseState = null;
            return false;
        }

        public override void StartEffect()
        {
            base.StartEffect();

            /*
            if (base.CurrentSkill.SkillConfig.CanBeTanFan)
            {
                bool beParried = WhetherBeParried(out var defenseState);
                if (beParried)
                {
                    Actor.OnParried(defenseState.Actor);
                    defenseState.OnTanFanSucceed(Actor);
                }
            }
            */
        }
    }

    public partial class AbilityEventEffect_TanDao : AbilityEventEffect
    {
        private AbilityEventObj_TanDao EventObj { get; set; }

        public AbilityEventEffect_TanDao(AbilityEventObj initObj) : base(initObj)
        {
            m_EventObj = initObj;
            EventObj = (AbilityEventObj_TanDao)initObj;
        }
    }
}