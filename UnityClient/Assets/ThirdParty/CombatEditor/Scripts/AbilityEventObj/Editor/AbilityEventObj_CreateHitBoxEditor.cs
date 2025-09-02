using System.Collections;
using System.Collections.Generic;
using Saber.CharacterController;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CombatEditor
{
    [CustomEditor(typeof(AbilityEventObj_CreateHitBox))]
    public class AbilityEventObj_CreateHitBoxEditor : Editor
    {
        private static AbilityEventObj_CreateHitBox s_FromObj;


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("复制"))
            {
                s_FromObj = target as AbilityEventObj_CreateHitBox;
            }

            if (GUILayout.Button("粘贴"))
            {
                Paste();
            }
        }

        void Paste()
        {
            if (s_FromObj == null)
            {
                EditorUtility.DisplayDialog("错误", "没有复制...", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog("提示", "确定要粘贴吗？", "确定", "取消"))
            {
                return;
            }

            AbilityEventObj_CreateHitBox from = s_FromObj;
            AbilityEventObj_CreateHitBox to = (AbilityEventObj_CreateHitBox)target;
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

            to.m_WeaponDamageSetting.m_WeaponBone = from.m_WeaponDamageSetting.m_WeaponBone;
            to.m_WeaponDamageSetting.m_DamageValue = from.m_WeaponDamageSetting.m_DamageValue;
            to.m_WeaponDamageSetting.m_ImpactForce = from.m_WeaponDamageSetting.m_ImpactForce;
            to.m_WeaponDamageSetting.m_HitRecover = from.m_WeaponDamageSetting.m_HitRecover;
            to.m_WeaponDamageSetting.m_ForceWhenGround = from.m_WeaponDamageSetting.m_ForceWhenGround;

            to.IsActive = true;
        }
    }
}