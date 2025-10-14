#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using UIAnimation;
using System;
using System.Collections.Generic;

namespace UIAnimation.Editor 
{
    [CustomEditor(typeof(Tween.TweenLegacyAnimation))]
    public class TweenLegacyAnimationInspector : UnityEditor.Editor
    {
        private Tween.TweenLegacyAnimation tw;
        
        void OnEnable()
        {
            tw = target as Tween.TweenLegacyAnimation;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(250f));
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Height(120f));
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(220f));
                    {
                        EditorGUIUtility.labelWidth = 90f;
                        tw.Description = EditorGUILayout.TextField("Description", tw.Description);
                        tw.Clip = EditorGUILayout.ObjectField("Clip", tw.Clip, typeof(AnimationClip), true) as AnimationClip;
                        if (tw.Clip != null) {
                            tw.WrapMode = (Actions.TweenActionBase.AnimationWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", tw.WrapMode);
                            EditorGUILayout.LabelField("Duration: " + string.Format("{0}秒", tw.Clip.length / tw.Speed));
                            EditorGUILayout.LabelField("FrameRate: " + string.Format("{0}fps", tw.Clip.frameRate));
                            tw.DurationSeconds = tw.Clip.length;
                        }
                        tw.DelaySeconds = EditorGUILayout.FloatField("Delay Seconds", tw.DelaySeconds);   
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUIUtility.fieldWidth = 30f;
                tw.Speed = EditorGUILayout.Slider("Speed", tw.Speed, -10f, 10f);
            }
            EditorGUILayout.EndVertical();
            
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
#endif