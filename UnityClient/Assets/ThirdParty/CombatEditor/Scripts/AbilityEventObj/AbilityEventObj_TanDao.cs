using System.Collections.Generic;
using Saber;
using Saber.CharacterController;
using UnityEngine;

//Replace the "TanDao" with the event you want to create
//If you want to create a object with handle in preview, please inherit the "AbilityEventObj" with "AbilityEventObj_CreateObjWithHandle"

namespace CombatEditor
{
    [AbilityEvent]
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
            }

            defenseState = null;
            return false;
        }

        public override void StartEffect()
        {
            base.StartEffect();

            bool beParried = WhetherBeParried(out var defenseState);
            if (beParried)
            {
                Actor.OnParried();
                defenseState.OnTanFanSucceed(Actor);
            }
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