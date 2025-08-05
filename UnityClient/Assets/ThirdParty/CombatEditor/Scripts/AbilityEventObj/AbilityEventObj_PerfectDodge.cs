using Saber;
using UnityEngine;

//Replace the "PerfectDodge" with the event you want to create
//If you want to create a object with handle in preview, please inherit the "AbilityEventObj" with "AbilityEventObj_CreateObjWithHandle"

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / PerfectDodge")]
    public class AbilityEventObj_PerfectDodge : AbilityEventObj_CreateObjWithHandle
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
            return new AbilityEventEffect_PerfectDodge(this);
        }

#if UNITY_EDITOR
        public override AbilityEventPreview InitializePreview()
        {
            return new AbilityEventPreview_PerfectDodge(this);
        }
#endif
    }

    //Write you logic here
    public partial class AbilityEventEffect_PerfectDodge : AbilityEventEffect
    {
        public override void StartEffect()
        {
            base.StartEffect();
            base.CurrentSkill.PerfectDodgeData = EventObj;
            
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
            base.CurrentSkill.PerfectDodgeData = null;
        }
    }

    public partial class AbilityEventEffect_PerfectDodge : AbilityEventEffect
    {
        private AbilityEventObj_PerfectDodge EventObj { get; set; }

        public AbilityEventEffect_PerfectDodge(AbilityEventObj initObj) : base(initObj)
        {
            m_EventObj = initObj;
            EventObj = (AbilityEventObj_PerfectDodge)initObj;
        }
    }
}