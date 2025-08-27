using System;
using CombatEditor;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.CharacterController
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / CreateHitBox")]
    public class AbilityEventObj_CreateHitBox : AbilityEventObj_CreateObjWithHandle
    {
        public Vector3 ColliderOffset = new Vector3(0, 0, 0);
        public Vector3 ColliderSize = new Vector3(1, 1, 1);
        public float Radius = 1;
        public float Height = 1;
        public WeaponDamageSetting m_WeaponDamageSetting;

        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventRange;
        }

        public override AbilityEventEffect Initialize()
        {
            ObjData.PreloadObjects();
            return new AbilityEventEffect_CreateHitBox(this);
        }
#if UNITY_EDITOR
        public override AbilityEventPreview InitializePreview()
        {
            return new AbilityEventPreview_CreateHitBox(this);
        }
#endif
    }

    public partial class AbilityEventEffect_CreateHitBox : AbilityEventEffect
    {
        public HitBox CurrentHitBox;

        public override void StartEffect()
        {
            base.StartEffect();
            var Obj = EventObj.ObjData.CreateObject(Actor);
            if (Obj == null)
            {
                return;
            }

            CurrentHitBox = Obj.GetComponent<HitBox>();
            if (CurrentHitBox != null)
            {
                CurrentHitBox.Init(this);
            }

            BoxCollider boxCollider = Obj.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.center = EventObj.ColliderOffset;
                boxCollider.size = EventObj.ColliderSize;
            }

            SphereCollider sphereCollider = Obj.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                sphereCollider.center = EventObj.ColliderOffset;
                sphereCollider.radius = EventObj.Radius;
            }

            CapsuleCollider capsuleCollider = Obj.GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                capsuleCollider.center = EventObj.ColliderOffset;
                capsuleCollider.radius = EventObj.Radius;
                capsuleCollider.height = EventObj.Height;
            }

            BoxCollider2D boxCollider2D = Obj.GetComponent<BoxCollider2D>();
            if (boxCollider2D != null)
            {
                boxCollider2D.transform.rotation = Quaternion.identity;
                boxCollider2D.offset = new Vector2(EventObj.ColliderOffset.z, EventObj.ColliderOffset.y);
                boxCollider2D.size = new Vector2(EventObj.ColliderSize.z, EventObj.ColliderSize.y);
            }

            CapsuleCollider2D capsuleCollider2D = Obj.GetComponent<CapsuleCollider2D>();
            if (capsuleCollider2D != null)
            {
                capsuleCollider2D.transform.rotation = Quaternion.identity;
                capsuleCollider2D.offset = new Vector2(EventObj.ColliderOffset.z, EventObj.ColliderOffset.y);
                capsuleCollider2D.size = new Vector2(EventObj.ColliderSize.z, EventObj.ColliderSize.y);
            }
            
            base.Actor.CMelee.ToggleDamage(EventObj.m_WeaponDamageSetting, true);
        }

        protected override void EndEffect()
        {
            CurrentHitBox?.Hide();

            base.EndEffect();
            
            base.Actor.CMelee.ToggleDamage(EventObj.m_WeaponDamageSetting, false);
        }
    }

    public partial class AbilityEventEffect_CreateHitBox : AbilityEventEffect
    {
        public AbilityEventObj_CreateHitBox EventObj => (AbilityEventObj_CreateHitBox)m_EventObj;

        public AbilityEventEffect_CreateHitBox(AbilityEventObj initObj) : base(initObj)
        {
            m_EventObj = initObj;
        }
    }
}