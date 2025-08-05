using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Saber.CharacterController;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CombatEditor
{
    //If you need to create object with handle, you can just inherit the AbilityEventPreview_CreateObjWithHandle
#if UNITY_EDITOR
    public partial class AbilityEventPreview_DynamicTrail : AbilityEventPreview
    {
        /*
        public GameObject _trailMeshObj;
        
        Vector3[] _vertices;
        Queue<int> trianglesQueue;


        Queue<Vector3> BaseQueue;
        Queue<Vector3> TipQueue;

        int[] _triangles;
        int _frameCount;
        Vector2[] _uvs;
        Mesh _mesh;

        //Vector3 _previousTipPosition;
        //Vector3 _previousBasePotision;
        int FrameCount;
        Transform _base;
        Transform _tip;

        int FrameIndex;
        // Triggers on startframe.
        
        DynamicTrailGenerator trail;
        */
        private WeaponTrail[] m_Trails;

        //Init Preview, for e.g, you can create your preview object here.
        public override void InitPreview()
        {
            base.InitPreview();

            Transform transBone =_combatController.Actor.GetNodeTransform(EventObj.m_WeaponBone);
            m_Trails = transBone.GetComponentsInChildren<WeaponTrail>();
            foreach (var item in m_Trails)
            {
                item.InitTrailMesh();
                item.TrailObj.transform.SetParent(GameObject.Find(CombatGlobalEditorValue.PreviewGroupName).transform);
            }

            /*
            _base = _combatController.GetNodeTranform(EventObj.BaseNode);
            _tip = _combatController.GetNodeTranform(EventObj.TipNode);
            if (_base == null || _tip == null)
            {
                return;
            }

            trail = new DynamicTrailGenerator(_base, _tip, EventObj.MaxFrame, EventObj.TrailSubs, EventObj.StopMultiplier, EventObj.TrailMat, DynamicTrailGenerator.TrailBehavior.FlowUV);
            _trailMeshObj = trail.InitTrailMesh();
            _trailMeshObj.transform.SetParent(GameObject.Find(CombatGlobalEditorValue.PreviewGroupName).transform);
            */
        }


        public override void PreviewUpdateFrame(float CurrentTimePercentage)
        {
            if (m_Trails != null)
            {
                foreach (var item in m_Trails)
                {
                    var trail = item.TrailGenerator;
                    if (trail == null)
                    {
                        return;
                    }

                    if (PreviewInRange(CurrentTimePercentage))
                    {
                        trail.UpdateTrailOnCurrentFrame();
                    }
                    else
                    {
                        trail.StopTrailSmoothly();
                    }
                }
            }
        }

        public override void BackToStart()
        {
            if (m_Trails != null)
            {
                foreach (var item in m_Trails)
                {
                    var trail = item.TrailGenerator;
                    if (trail != null)
                    {
                        trail.StopTrailHard();
                    }
                }
            }

            base.BackToStart();
        }

        public override void DestroyPreview()
        {
            if (m_Trails != null)
            {
                foreach (var item in m_Trails)
                {
                    if (item.TrailObj != null)
                    {
                        Object.DestroyImmediate(item.TrailObj);
                    }
                }
            }

            base.DestroyPreview();
        }
    }

    public partial class AbilityEventPreview_DynamicTrail : AbilityEventPreview
    {
        public AbilityEventObj_DynamicTrail EventObj => (AbilityEventObj_DynamicTrail)m_EventObj;

        public AbilityEventPreview_DynamicTrail(AbilityEventObj Obj) : base(Obj)
        {
            m_EventObj = Obj;
        }
    }
#endif
}