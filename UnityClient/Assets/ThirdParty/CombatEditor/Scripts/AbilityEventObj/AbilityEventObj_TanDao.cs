using Saber;
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
            return EventTimeType.EventRange;
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
        public override void StartEffect()
        {
            base.StartEffect();
            base.CurrentSkill.TanDaoData = EventObj;
            
            /*
            // debug
            Transform node = base.Actor.GetNodeTranform(EventObj.ObjData.TargetNode);
            Vector3 pos = node.position + node.rotation * EventObj.ObjData.Offset + EventObj.ColliderOffset;
            float radius = EventObj.Radius;
            SDebug.DrawWireSphere(pos, Color.red, radius, 3f);
            */
        }

        protected override void EndEffect()
        {
            base.EndEffect();
            base.CurrentSkill.TanDaoData = null;
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