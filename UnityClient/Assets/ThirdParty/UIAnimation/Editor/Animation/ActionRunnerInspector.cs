#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using UIAnimation.Actions;
using System;
using System.Collections.Generic;

namespace UGUIAnimation.Editor
{
    [CustomEditor(typeof(ActionRunner))]
    public class ActionRunnerInspector : UnityEditor.Editor
    {
        private ActionRunner actionRunner;
        private ActionEntryContainer container;
        private Vector2 m_ScrollPosition;

        void OnEnable()
        {
            actionRunner = target as ActionRunner;
            container = actionRunner.EntryContainer;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (Application.isEditor && Application.isPlaying)
            {
                EditorGUILayout.LabelField("Status: " + actionRunner.Status.ToString());
                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(160f));
                {
                    EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(180f));
                    {
                        if (GUILayout.Button("Run", EditorStyles.miniButtonLeft))
                        {
                            actionRunner.Run();
                        }

                        if (GUILayout.Button("Pause", EditorStyles.miniButtonMid))
                        {
                            actionRunner.Pause();
                        }

                        if (GUILayout.Button("Stop", EditorStyles.miniButtonRight))
                        {
                            actionRunner.Stop();
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (GUILayout.Button("FastForward", EditorStyles.miniButton))
                    {
                        actionRunner.FastForward();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            actionRunner.FastforwardOnDisable = EditorGUILayout.Toggle("Fastforward Before Inactive", actionRunner.FastforwardOnDisable);
            actionRunner.Description = EditorGUILayout.TextField("Description", actionRunner.Description);
            EditorGUIUtility.labelWidth = 60f;
            if (container.RootEntry != null)
            {
                m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
                DrawEntry(container.RootEntry);
                GUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(140f));
                {
                    if (GUILayout.Button("Add Root Entry", EditorStyles.miniButtonLeft))
                    {
                        AddRootEntry(ActionEntryType.Executable);
                    }

                    if (GUILayout.Button("Add Root Block", EditorStyles.miniButtonRight))
                    {
                        AddRootEntry(ActionEntryType.ParallelBlock);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
            
            if (GUI.changed && !Application.isPlaying)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawEntry(ActionEntry entry)
        {
            if (!entry.HasPrev)
            {
                EditorGUILayout.BeginVertical(GUI.skin.label, GUILayout.MinWidth(140f));
            }

            if (entry.EntryType == ActionEntryType.Executable)
            {
                DrawSerialEntry(entry);
            }
            else
            {
                DrawParallelEntry(entry);
            }

            if (entry.HasNext)
            {
                DrawEntry(container.GetEntry(entry.NextId));
            }
            else
            {
                DrawAppendButtonsFor(entry);
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawSerialEntry(ActionEntry entry)
        {
            GUI.color = Color.green;
            if (entry.PrimaryAction != null && entry.PrimaryAction.IsRunning)
            {
                GUI.color = Color.red;
            }

            EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.MinWidth(140f));
            {
                GUI.color = Color.white;
                DrawRemoveButtonFor(entry);

                if (entry.PrimaryAction != null)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.button);
                    {
                        EditorGUILayout.LabelField(entry.PrimaryAction.Description);
                        entry.PrimaryAction = EditorGUILayout.ObjectField(entry.PrimaryAction, typeof(IAction), true) as IAction;

                        // Draw Action's Custom Editor 
                        CreateEditor(entry.PrimaryAction).OnInspectorGUI();
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    entry.PrimaryAction = EditorGUILayout.ObjectField(entry.PrimaryAction, typeof(IAction), true) as IAction;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawParallelEntry(ActionEntry entry)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                GUI.color = Color.yellow;
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                {
                    GUI.color = Color.white;
                    DrawRemoveButtonFor(entry);
                    EditorGUILayout.LabelField("Parallel Block");
                    DrawInsertButtonsFor(entry);
                }

                EditorGUILayout.EndHorizontal();
                if (!entry.HasChild)
                {
                    GUILayout.Space(40f);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        foreach (var child in entry.GetChildEntries(container))
                        {
                            DrawEntry(child);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }


        /// <summary>
        /// Button Actions
        /// </summary>
        private void AddRootEntry(ActionEntryType type)
        {
            var entry = new ActionEntry();
            entry.EntryType = type;
            container.AddEntry(entry);
            UnityEngine.Debug.Log("count: " + container.EntryDict.Count);
            UnityEngine.Debug.Log("Add root to: " + entry.Id);
        }

        private void AppendTo(ActionEntry thisEntry, ActionEntryType type)
        {
            UnityEngine.Debug.Log("append to: " + thisEntry.Id);
            var nextEntry = new ActionEntry();
            nextEntry.EntryType = type;
            container.AppendEntryTo(nextEntry, thisEntry);
        }

        private void InsertTo(ActionEntry parentEntry, ActionEntryType type)
        {
            UnityEngine.Debug.Log("insert to: " + parentEntry.Id);
            var childEntry = new ActionEntry();
            childEntry.EntryType = type;
            container.InsertChildEntryTo(childEntry, parentEntry);
        }

        private void Remove(ActionEntry entry)
        {
            UnityEngine.Debug.Log("Remove: " + entry.Id);
            container.RemoveEntry(entry);
            UnityEngine.Debug.Log("size after removing entry: " + entry.Id + ", is " + container.EntryDict.Count);
        }

        private void DrawRemoveButtonFor(ActionEntry entry)
        {
            if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.MaxWidth(18f)))
            {
                Remove(entry);
            }
        }

        private void DrawAppendButtonsFor(ActionEntry entry)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(140f), GUILayout.MaxWidth(160f));
            {
                if (GUILayout.Button("Append Entry", EditorStyles.miniButtonLeft))
                {
                    AppendTo(entry, ActionEntryType.Executable);
                }

                if (GUILayout.Button("Append Block", EditorStyles.miniButtonRight))
                {
                    AppendTo(entry, ActionEntryType.ParallelBlock);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawInsertButtonsFor(ActionEntry entry)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(160f));
            {
                if (GUILayout.Button("+ Entry", EditorStyles.miniButtonLeft))
                {
                    InsertTo(entry, ActionEntryType.Executable);
                }

                if (GUILayout.Button("+ Block", EditorStyles.miniButtonRight))
                {
                    InsertTo(entry, ActionEntryType.ParallelBlock);
                }

                entry.EntryWrapMode = (ActionEntry.ActionEntryWrapMode)EditorGUILayout.EnumPopup(entry.EntryWrapMode);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif