#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using UIAnimation;
using System;
using System.Collections.Generic;

namespace UIAnimation.Editor 
{
    [CustomEditor(typeof(Tween.TweenAlphaBy))]
    public class TweenAlphaByInspector : UnityEditor.Editor 
    {
        private Tween.TweenAlphaBy tw;
        
        void OnEnable()
        {
            tw = target as Tween.TweenAlphaBy;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            TweenActionCommonInspector.DrawTweenActionBase(tw);
            
            DrawTweenAlphaBy();
            
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
        
        private void DrawTweenAlphaBy()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(274f));
            {
                EditorGUIUtility.labelWidth = 90f;
                tw.DeltaAlpha = EditorGUILayout.Slider("Delta Alpha", tw.DeltaAlpha, -1f, 1f);
            }
            EditorGUILayout.EndVertical();
        }
        
    }
    
}
#endif