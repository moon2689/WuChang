#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using UIAnimation;
using System;
using System.Collections.Generic;

namespace UIAnimation.Editor 
{
    [CustomEditor(typeof(Actions.CallbackAction), true)]
    public class CallbackActionInspector : UnityEditor.Editor
    {
        private Actions.CallbackAction callback;
        
        void OnEnable()
        {
            callback = target as Actions.CallbackAction;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(250f));
            {
                EditorGUIUtility.labelWidth = 80f;
                EditorGUIUtility.fieldWidth = 46f;
                EditorGUILayout.BeginHorizontal();
                {
                    callback.Description = EditorGUILayout.TextField("Description", callback.Description);
                    callback.DelaySeconds = EditorGUILayout.FloatField("Delay Seconds", callback.DelaySeconds);        
                }
                EditorGUILayout.EndHorizontal();

                DrawCallbacks();
            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
        
        protected virtual void DrawCallbacks()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("callback"));
        }
    }
}
#endif