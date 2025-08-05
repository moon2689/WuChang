using UnityEngine;

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / CreateObjWithHandle")]
    public class AbilityEventObj_CreateObjWithHandle : AbilityEventObj
    {
        public InsedObject ObjData = new InsedObject();

        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventTime;
        }

        public override AbilityEventEffect Initialize()
        {
            ObjData.PreloadObjects(); //预加载
            return new AbilityEventEffect_CreateObjWithHandle(this);
        }
#if UNITY_EDITOR
        public override AbilityEventPreview InitializePreview()
        {
            return new AbilityEventPreview_CreateObjWithHandle(this);
        }
#endif
    }

    public partial class AbilityEventEffect_CreateObjWithHandle : AbilityEventEffect
    {
        public GameObject m_InstantiatedObj;

        public override void StartEffect()
        {
            base.StartEffect();
            m_InstantiatedObj = EventObj.ObjData.CreateObject(base.Actor);
        }
    }

    public partial class AbilityEventEffect_CreateObjWithHandle : AbilityEventEffect
    {
        AbilityEventObj_CreateObjWithHandle EventObj => (AbilityEventObj_CreateObjWithHandle)m_EventObj;

        public AbilityEventEffect_CreateObjWithHandle(AbilityEventObj InitObj) : base(InitObj)
        {
            m_EventObj = InitObj;
        }
    }
}