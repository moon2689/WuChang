#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using UIAnimation;
using System;
using System.Collections.Generic;

namespace UIAnimation.Editor 
{
    [CustomEditor(typeof(Tween.TweenTransformMoveBy))]
    public class TweenTransformMoveByInspector : UnityEditor.Editor 
    {
        private Tween.TweenTransformMoveBy tw;
        
        void OnEnable()
        {
            tw = target as Tween.TweenTransformMoveBy;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            TweenActionCommonInspector.DrawTweenActionBase(tw);
            
            DrawTweenMoveBy();
            
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
        
        private void DrawTweenMoveBy()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(274f));
            {
                EditorGUIUtility.labelWidth = 90f;
                tw.DeltaVec3 = EditorGUILayout.Vector3Field("Delta Vec3", tw.DeltaVec3);
            }
            EditorGUILayout.EndVertical();
        }
        
    }
    
}
#endif