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
        public EProjectileType m_ProjectileType = EProjectileType.Single;

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

    public enum EProjectileType
    {
        Single, //发射一个
        MultiARow, //发射多个，排成一排
    }

    //Write you logic here
    public partial class AbilityEventEffect_Projectile : AbilityEventEffect
    {
        public override void StartEffect()
        {
            base.StartEffect();
            if (EventObj.m_ProjectileType == EProjectileType.Single)
            {
                Projectile p = EventObj.ObjData.CreateObject(Actor).GetComponent<Projectile>();
                p.Throw(this.Actor, base.Actor.AI.LockingEnemy);
            }
            else if (EventObj.m_ProjectileType == EProjectileType.MultiARow)
            {
                float angle = -15;
                for (int i = 0; i < 7; i++)
                {
                    Projectile p = EventObj.ObjData.CreateObject(Actor).GetComponent<Projectile>();
                    p.Throw(this.Actor, base.Actor.AI.LockingEnemy, angle);
                    angle += 5;
                }
            }
            else
            {
                Debug.LogError($"Unknown projectile type:{EventObj.m_ProjectileType}");
            }
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