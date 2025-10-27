using Saber.CharacterController;
using UnityEngine;

//Replace the "Projectile" with the event you want to create
//If you want to create a object with handle in preview, please inherit the "AbilityEventObj" with "AbilityEventObj_CreateObjWithHandle"

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / Projectile")]
    public class AbilityEventObj_Projectile : AbilityEventObj_CreateObjWithHandle
    {
        //Write the data you need here.
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventTime;
        }

        public override AbilityEventEffect Initialize()
        {
            ObjData.PreloadObjects();
            return new AbilityEventEffect_Projectile(this);
        }
#if UNITY_EDITOR
        public override AbilityEventPreview InitializePreview()
        {
            return new AbilityEventPreview_Projectile(this);
        }
#endif
    }

//Write you logic here
    public partial class AbilityEventEffect_Projectile : AbilityEventEffect
    {
        private Projectile m_Projectile;


        public override void StartEffect()
        {
            base.StartEffect();
            m_Projectile = EventObj.ObjData.CreateObject(Actor).GetComponent<Projectile>();
            m_Projectile.Throw(this.Actor, base.Actor.AI.LockingEnemy);
        }
    }

    public partial class AbilityEventEffect_Projectile : AbilityEventEffect
    {
        private AbilityEventObj_Projectile EventObj { get; set; }

        public AbilityEventEffect_Projectile(AbilityEventObj initObj) : base(initObj)
        {
            m_EventObj = initObj;
            EventObj = (AbilityEventObj_Projectile)initObj;
        }
    }
}