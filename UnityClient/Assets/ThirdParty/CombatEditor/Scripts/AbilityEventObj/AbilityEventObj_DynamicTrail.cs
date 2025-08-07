using Saber.CharacterController;
using UnityEngine;

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / DynamicTrail")]
    public class AbilityEventObj_DynamicTrail : AbilityEventObj
    {
        /*
        public ENodeType BaseNode;
        public ENodeType TipNode;
        public Material TrailMat;
        public int MaxFrame = 50;
        public int StopMultiplier = 4;
        [Range(2, 8)] public int TrailSubs = 2;
        [HideInInspector] public int NUM_VERTICES = 12;
        
        //[SerializeField]
        //public TrailBehavior uvMethod;
        */
        public ENodeType m_WeaponBone = ENodeType.RightHand;
        public bool m_NotTriggerWhenPowerEnough;

        //Write the data you need here.
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventRange;
        }

        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_DynamicTrail(this);
        }
#if UNITY_EDITOR
        public override AbilityEventPreview InitializePreview()
        {
            return new AbilityEventPreview_DynamicTrail(this);
        }
#endif
    }

    //Write you logic here
    public partial class AbilityEventEffect_DynamicTrail : AbilityEventEffect
    {
        /*
        DynamicTrailGenerator trail;
        Transform _base;
        Transform _tip;
        DynamicTrailExecutor executor;
        */

        public override void StartEffect()
        {
            base.StartEffect();


            /*
            _base = base.Actor.GetNodeTranform(EventObj.BaseNode);
            _tip = base.Actor.GetNodeTranform(EventObj.TipNode);
            if (_base == null || _tip == null)
            {
                return;
            }

            trail = new DynamicTrailGenerator(_base, _tip, EventObj.MaxFrame, EventObj.TrailSubs, EventObj.StopMultiplier, EventObj.TrailMat, DynamicTrailGenerator.TrailBehavior.FlowUV);
            trail.InitTrailMesh();
            executor = trail._trailMeshObj.AddComponent<DynamicTrailExecutor>();
            executor.trail = trail;
            executor.StartTrail();
            */
            
            bool trigger = true;
            if (EventObj.m_NotTriggerWhenPowerEnough)
            {
                if (base.CurrentSkill.IsPowerEnough)
                    trigger = false;
            }

            if (trigger)
                base.Actor.CMelee.CWeapon.ShowWeaponTrail(EventObj.m_WeaponBone);
        }

        protected override void EndEffect()
        {
            /*
            if (executor != null)
            {
                executor.StopTrail();
            }
            */
            base.Actor.CMelee.CWeapon.HideWeaponTrail(EventObj.m_WeaponBone);

            base.EndEffect();
        }
    }

    public partial class AbilityEventEffect_DynamicTrail : AbilityEventEffect
    {
        AbilityEventObj_DynamicTrail EventObj => (AbilityEventObj_DynamicTrail)m_EventObj;

        public AbilityEventEffect_DynamicTrail(AbilityEventObj InitObj) : base(InitObj)
        {
            m_EventObj = InitObj;
        }
    }
}