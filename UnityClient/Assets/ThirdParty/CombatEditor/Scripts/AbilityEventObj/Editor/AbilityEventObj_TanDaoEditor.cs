using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CombatEditor
{
    [CustomEditor(typeof(AbilityEventObj_TanDao))]
    public class AbilityEventObj_TanDaoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Paste from perfect dodge"))
            {
                PasteFromPerfectDodge();
            }
        }

        void PasteFromPerfectDodge()
        {
            var from = AbilityEventObj_PerfectDodgeEditor.CopyObj;
            if (from == null)
            {
                return;
            }

            AbilityEventObj_TanDao to = (AbilityEventObj_TanDao)target;
            to.ObjData.TargetObj = from.ObjData.TargetObj;
            to.ObjData.controlType = from.ObjData.controlType;
            to.ObjData.Offset = from.ObjData.Offset;
            to.ObjData.Rot = from.ObjData.Rot;
            to.ObjData.RotateByNode = from.ObjData.RotateByNode;
            to.ObjData.FollowNode = from.ObjData.FollowNode;
            to.ObjData.TargetNode = from.ObjData.TargetNode;
            to.Height = from.Height;
            to.Radius = from.Radius;
            to.ColliderOffset = from.ColliderOffset;
            to.ColliderSize = from.ColliderSize;
            to.IsActive = true;
        }
    }
}