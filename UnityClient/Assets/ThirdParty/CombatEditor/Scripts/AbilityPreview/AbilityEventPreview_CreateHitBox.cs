using Saber.CharacterController;
using UnityEngine;

namespace CombatEditor
{
#if UNITY_EDITOR
    public class AbilityEventPreview_CreateHitBox : AbilityEventPreview_CreateObjWithHandle, IColliderPreviewTarget
    {
        PreviewTransformHandle TransformHandle;
        ColliderPreviewHandle ColliderHandle;

        public AbilityEventObj_CreateHitBox Obj => (AbilityEventObj_CreateHitBox)m_EventObj;

        public AbilityEventPreview_CreateHitBox(AbilityEventObj Obj) : base(Obj)
        {
            m_EventObj = Obj;
        }

        public bool PreviewActive()
        {
            return eve.Previewable;
        }

        public override void InitPreview()
        {
            base.InitPreview();

            if (Obj.ObjData.TargetObj == null)
            {
                return;
            }

            //AddControlScript.
            CreateHitBoxHandles();
        }


        public void CreateHitBoxHandles()
        {
            ColliderHandle = InstantiatedObj.AddComponent<ColliderPreviewHandle>();

            ColliderHandle._combatController = _combatController;
            ColliderHandle._preview = this;
            ColliderHandle.colliderPreview = this;
            ColliderHandle.Init();
        }

        //SetCurrentParticleTime;
        public override void PreviewRunning(float CurrentTime)
        {
            //Set Preview Position and Rotation
            base.PreviewRunning(CurrentTime);
        }

        //Destroy Particles.
        public override void DestroyPreview()
        {
            if (InstantiatedObj != null)
            {
                Object.DestroyImmediate(InstantiatedObj);
            }

            base.DestroyPreview();
        }

        Vector3 IColliderPreviewTarget.ColliderSize
        {
            get => Obj.ColliderSize;
            set => Obj.ColliderSize = value;
        }

        Vector3 IColliderPreviewTarget.ColliderOffset
        {
            get => Obj.ColliderOffset;
            set => Obj.ColliderOffset = value;
        }

        float IColliderPreviewTarget.Radius
        {
            get => Obj.Radius;
            set => Obj.Radius = value;
        }

        float IColliderPreviewTarget.Height
        {
            get => Obj.Height;
            set => Obj.Height = value;
        }

        Object IColliderPreviewTarget.TheObject => Obj;
    }
#endif
}