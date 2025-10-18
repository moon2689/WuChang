using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Saber;
using Saber.CharacterController;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditorInternal;
using UnityEngine;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using Object = UnityEngine.Object;

namespace CombatEditor
{
    public class CombatInspector : EditorWindow
    {
        private float m_HeightTop = 40;
        private CombatEditor m_CombatEditor;
        private Editor m_InspectedEditor;
        private Vector2 m_InspectorScrollPos;

        private int m_SelectedClipIndex;

        private Vector2 m_Scroll;

        private int m_CurrentGroupIndex = -1;
        private int m_CurrentAbilityIndex = -1;
        private string m_ConfigFileName;


        //ReorderableList NodeList;

        [MenuItem("Tools/CombatInspector")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            CombatInspector.CreateWindow();
            //window.InitWindow();
        }

        public static CombatInspector CreateWindow()
        {
            CombatInspector window = (CombatInspector)EditorWindow.GetWindow(typeof(CombatInspector));
            window.Show();
            return window;
        }

        private void OnEnable()
        {
        }

        public void ResetInspector()
        {
            CombatControllerSO = null;
            //NodeList = null;
        }


        private void OnGUI()
        {
            PaintInspector();
        }


        public void PaintInspector()
        {
            if (!CombatEditorUtility.EditorExist())
            {
                return;
            }

            m_CombatEditor = CombatEditorUtility.GetCurrentEditor();
            if (m_CombatEditor.SelectedController == null)
            {
                return;
            }

            #region Init

            var HeaderStyle = m_CombatEditor.HeaderStyle;
            var inspectedType = m_CombatEditor.CurrentInspectedType;

            var CurrentAbilityObj = m_CombatEditor.SelectedAbilityObj;

            m_CurrentGroupIndex = m_CombatEditor.CurrentGroupIndex;
            m_CurrentAbilityIndex = m_CombatEditor.CurrentAbilityIndexInGroup;

            var SelectedTrackIndex = m_CombatEditor.SelectedTrackIndex;
            var TrackHeight = CombatEditor.LineHeight;

            #endregion

            GUILayout.Box("Inspector", HeaderStyle, GUILayout.Height(m_HeightTop));
            Rect InspectorRect = new Rect(new Rect(0, m_HeightTop, position.width, position.height));
            if (inspectedType == CombatEditor.InspectedType.Null)
            {
                return;
            }

            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            if (inspectedType == CombatEditor.InspectedType.PreviewConfig)
            {
                m_CombatEditor.PlayTimeMultiplier =
                    EditorGUILayout.FloatField(new GUIContent("PlaySpeed"), m_CombatEditor.PlayTimeMultiplier);
                m_CombatEditor.LoopWaitTime =
                    EditorGUILayout.FloatField(new GUIContent("LoopInterval"), m_CombatEditor.LoopWaitTime);
            }

            if (inspectedType == CombatEditor.InspectedType.AnimationConfig)
            {
                float DefaultWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 80;
                SerializedObject so = new SerializedObject(m_CombatEditor.SelectedController);
                SerializedProperty combatDatas = so.FindProperty("CombatDatas");
                so.Update();
                if (m_CombatEditor.CurrentGroupIndex < combatDatas.arraySize && m_CombatEditor.CurrentGroupIndex >= 0)
                {
                    SerializedProperty ObjsProperty = combatDatas.GetArrayElementAtIndex(m_CombatEditor.CurrentGroupIndex).FindPropertyRelative("CombatObjs");
                    if (m_CombatEditor.CurrentAbilityIndexInGroup < ObjsProperty.arraySize &&
                        m_CombatEditor.CurrentAbilityIndexInGroup >= 0)
                    {
                        SerializedProperty TargetObj = ObjsProperty.GetArrayElementAtIndex(m_CombatEditor.CurrentAbilityIndexInGroup);
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(TargetObj, new GUIContent("ConfigFile"));
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_CombatEditor.SelectedAbilityObj = (AbilityScriptableObject)TargetObj.objectReferenceValue;
                            m_CombatEditor.LoadL3();
                            m_CombatEditor.Repaint();
                            m_CombatEditor.FlushAndInsPreviewToFrame0();
                            Repaint();
                        }

                        if (TargetObj.objectReferenceValue == null)
                        {
                            if (string.IsNullOrEmpty(m_ConfigFileName))
                            {
                                m_ConfigFileName = "NewConfig";
                            }

                            m_ConfigFileName = EditorGUILayout.TextField("Name", m_ConfigFileName);
                            if (GUILayout.Button("CreatConfig"))
                            {
                                TargetObj.objectReferenceValue = CreateAbilityScriptableObject();
                                m_CombatEditor.SelectedAbilityObj = (AbilityScriptableObject)TargetObj.objectReferenceValue;
                                m_CombatEditor.LoadL3();
                                m_CombatEditor.Repaint();
                                m_CombatEditor.FlushAndInsPreviewToFrame0();
                            }
                        }

                        if (TargetObj.objectReferenceValue != null)
                        {
                            DrawAnimationClipSelector((AbilityScriptableObject)TargetObj.objectReferenceValue);
                            DrawSelectSkillAnim((AbilityScriptableObject)TargetObj.objectReferenceValue);
                            DrawAbilityEventSelector((AbilityScriptableObject)TargetObj.objectReferenceValue);
                        }
                    }
                }

                so.ApplyModifiedProperties();
                EditorGUIUtility.labelWidth = DefaultWidth;
            }

            if (inspectedType == CombatEditor.InspectedType.Track)
            {
                //myRect.center = Vector2.one * 200;
                float DefaultWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 80;
                if (CurrentAbilityObj != null)
                {
                    if (SelectedTrackIndex - 1 < CurrentAbilityObj.events.Count && SelectedTrackIndex - 1 >= 0 &&
                        CurrentAbilityObj.events.Count > 0)
                    {
                        string name = CurrentAbilityObj.events[SelectedTrackIndex - 1].Obj.name;
                        CurrentAbilityObj.events[SelectedTrackIndex - 1].Obj.name =
                            EditorGUILayout.TextField("Name", name);
                    }

                    if (m_InspectedEditor != null)
                    {
                        if (m_InspectedEditor.target != null)
                        {
                            m_InspectedEditor.OnInspectorGUI();
                        }
                    }

                    EditorGUIUtility.labelWidth = DefaultWidth;
                }
            }

            if (inspectedType == CombatEditor.InspectedType.CombatConfig)
            {
                if (m_CombatEditor.SelectedController != null)
                {
                    CombatController controller = m_CombatEditor.SelectedController;
                    SerializedObject so = new SerializedObject(controller);
                    //CombatControllerSO.Update();
                    /*
                    EditorGUILayout.PropertyField(so.FindProperty("_animator"));
                    if (m_CombatEditor.SelectedController._animator != null)
                    {
                        if (m_CombatEditor.SelectedController._animator.transform ==
                            m_CombatEditor.SelectedController.transform)
                        {
                            EditorGUILayout.HelpBox("Animator transform should be the child transform of Combatcontroller!", MessageType.Error);
                        }
                    }
                    */

                    if ( /*NodeList == null ||*/ CombatControllerSO == null)
                    {
                        InitNodeReorableList();
                    }

                    CombatControllerSO.Update();
                    //NodeList.DoLayoutList();
                    CombatControllerSO.ApplyModifiedProperties();
                    so.ApplyModifiedProperties();
                }

                // [saber]
                if (GUILayout.Button("清理"))
                {
                    m_CombatEditor.ClearControllersInScene();
                }

                if (GUILayout.Button("关联技能配置"))
                {
                    LinkSkillConfig();
                }

                if (GUILayout.Button("保存"))
                {
                    m_CombatEditor.SaveControllersFromSceneToAsset();
                }

                if (GUILayout.Button("离开"))
                {
                    SaberTools.OpenScene_Launcher();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        #region 关联技能配置

        void LinkSkillConfig()
        {
            SActor actor = m_CombatEditor.SelectedController._animator.GetComponent<SActor>();
            SkillConfig tarSkillConfig = actor.SkillConfigs;

            foreach (var combatGroup in m_CombatEditor.SelectedController.CombatDatas)
            {
                foreach (var combatObj in combatGroup.CombatObjs)
                {
                    ChildAnimatorState animatorState = GetSkillAnimStateByClip(combatObj.Clip);
                    foreach (var skillItem in tarSkillConfig.m_SkillItems)
                    {
                        foreach (var skillAnimState in skillItem.m_AnimStates)
                        {
                            if (skillAnimState.m_Name == animatorState.state.name)
                                skillAnimState.m_EventData = combatObj;
                        }
                    }

                    EditorUtility.SetDirty(tarSkillConfig);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Debug.Log($"关联成功！skill:{tarSkillConfig.name},state:{animatorState.state.name},config:{combatObj.name}", tarSkillConfig);
                }
            }

            Debug.Log($"关联成功！{tarSkillConfig.name}", tarSkillConfig);
        }

        private ChildAnimatorState GetSkillAnimStateByClip(AnimationClip clip)
        {
            ChildAnimatorState[] states = GetSkillAnimStates();
            return states.FirstOrDefault(a => a.state.motion != null && a.state.motion.name == clip.name);
        }

        private void DrawSelectSkillAnim(AbilityScriptableObject currentAbilityObj)
        {
            if (EditorGUILayout.DropdownButton(new GUIContent("选择技能动画"), FocusType.Passive))
            {
                ChildAnimatorState[] skillStates = GetSkillAnimStates();
                List<string> clipsNames = new List<string>();
                for (int i = 0; i < skillStates.Length; i++)
                {
                    clipsNames.Add(skillStates[i].state.motion.name);
                }

                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < clipsNames.Count; i++)
                {
                    string menuName = $"{clipsNames[i]}";
                    menu.AddItem(new GUIContent(menuName), false, (object index) =>
                    {
                        int.TryParse(index.ToString(), out int clipIndex);
                        currentAbilityObj.Clip = skillStates[clipIndex].state.motion as AnimationClip;
                        m_CombatEditor.LoadL3();
                    }, i);
                }

                menu.ShowAsContext();
            }
        }

        /*
        AnimatorState GetAnimatorStateByClipRecursive(AnimatorStateMachine animatorStateMachine, AnimationClip clip)
        {
            foreach (var state in animatorStateMachine.states)
            {
                Debug.Log($"{state.state.name} motion:{state.state.motion.name}  ");
                if (state.state.motion is AnimationClip ac && ac == clip)
                {
                    return state.state;
                }
            }

            foreach (var sm in animatorStateMachine.stateMachines)
            {
                return GetAnimatorStateByClipRecursive(sm.stateMachine, clip);
            }

            return null;
        }
        */

        private ChildAnimatorState[] GetSkillAnimStates()
        {
            CombatController controller = m_CombatEditor.SelectedController;
            Animator animator = controller._animator;
            if (animator == null)
                return null;
            AnimatorController ac = animator.runtimeAnimatorController as AnimatorController;
            var sm = ac.layers[0].stateMachine;
            for (int i = 0; i < sm.stateMachines.Length; i++)
            {
                var stateMachine = sm.stateMachines[i].stateMachine;
                if (stateMachine.name.Equals("skill", StringComparison.OrdinalIgnoreCase))
                    return stateMachine.states;
            }

            return sm.states;
        }

        #endregion


        public AbilityScriptableObject CreateAbilityScriptableObject()
        {
            if (!System.IO.Directory.Exists(CombatEditor.SandBoxPath))
            {
                System.IO.Directory.CreateDirectory(CombatEditor.SandBoxPath);
            }

            AbilityScriptableObject InsObj = CreateInstance("AbilityScriptableObject") as AbilityScriptableObject;
            InsObj.name = m_ConfigFileName; //"NewAbilityScriptableObject";
            string path = CombatEditor.SandBoxPath;
            int index = 0;
            string TargetPath = path + InsObj.name + ".asset";
            while (true)
            {
                if (File.Exists(TargetPath))
                {
                    TargetPath = path + InsObj.name + "_" + index + ".asset";
                }
                else
                {
                    break;
                }

                index += 1;
            }

            AssetDatabase.CreateAsset(InsObj, TargetPath);
            return InsObj;
        }


        public void CreateInspectedObj(Object InspectedObj)
        {
            ClearInspectedObj();

            m_InspectedEditor = Editor.CreateEditor(InspectedObj);
            Repaint();
        }

        public void ClearInspectedObj()
        {
            if (m_InspectedEditor != null)
            {
                DestroyImmediate(m_InspectedEditor);
            }
        }

        public static CombatInspector GetInspector()
        {
            return EditorWindow.GetWindow<CombatInspector>(false);
        }

        public void DrawAbilityConfigSelector(AbilityScriptableObject CurrentAbilityObj)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("ConfigObj", CurrentAbilityObj, typeof(AbilityScriptableObject), false);
            EditorGUI.EndDisabledGroup();
        }

        public void DrawAbilityEventSelector(AbilityScriptableObject CurrentAbilityObj)
        {
            if (EditorGUILayout.DropdownButton(new GUIContent("Copy Events From Template"), FocusType.Passive))
            {
                if (!System.IO.Directory.Exists(CombatEditor.TemplatesPath))
                {
                    System.IO.Directory.CreateDirectory(CombatEditor.TemplatesPath);
                }

                AbilityScriptableObject[] TemplatesObjs =
                    CombatEditor.GetAtPath<AbilityScriptableObject>(CombatEditor.TemplatesPath);
                List<string> TemplatesObjNames = new List<string>();
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < TemplatesObjs.Length; i++)
                {
                    TemplatesObjNames.Add(TemplatesObjs[i].name);
                    menu.AddItem(new GUIContent(TemplatesObjs[i].name), false, CopyAbilityEvent, TemplatesObjs[i]);
                }

                menu.ShowAsContext();
            }
        }

        public void CopyAbilityEvent(object obj)
        {
            var editor = CombatEditorUtility.GetCurrentEditor();
            AbilityScriptableObject CurrentObj = editor.SelectedAbilityObj;
            for (int i = 0; i < CurrentObj.events.Count; i++)
            {
                string path = AssetDatabase.GetAssetPath(CurrentObj.events[i].Obj);
                var EveObj = CurrentObj.events[i].Obj;
                AssetDatabase.RemoveObjectFromAsset(EveObj);
                DestroyImmediate(EveObj, true);
            }

            CurrentObj.events = new List<AbilityEvent>();


            List<AbilityEvent> TargetEves = new List<AbilityEvent>();
            TargetEves = (obj as AbilityScriptableObject).events;

            for (int i = 0; i < TargetEves.Count; i++)
            {
                if (TargetEves[i].Obj == null) continue;
                AbilityEvent eve = new AbilityEvent();
                var EveObj = Instantiate(TargetEves[i].Obj);
                eve.Obj = EveObj;
                string path = AssetDatabase.GetAssetPath(editor.SelectedAbilityObj);
                eve.Obj.name = eve.Obj.name.Replace("(Clone)", "");
                AssetDatabase.AddObjectToAsset(EveObj, path);
                CurrentObj.events.Add(eve);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            editor.LoadL3();
            //Debug.Log("CopyAbility!");
        }

        public void DrawAnimationClipSelector(AbilityScriptableObject CurrentAbilityObj)
        {
            if (CurrentAbilityObj == null)
            {
                return;
            }

            CurrentAbilityObj.Clip =
                (AnimationClip)EditorGUILayout.ObjectField("Clip", CurrentAbilityObj.Clip, typeof(AnimationClip),
                    false);

            CombatController controller = m_CombatEditor.SelectedController;
            Animator animator = controller._animator;
            if (animator == null)
                return;
            var clips = animator.runtimeAnimatorController.animationClips;
            Array.Sort(clips, (a, b) => a.name.CompareTo(b.name));

            //Animator clips may change, so the index should auto change.
            bool ClipExist = false;
            for (int i = 0; i < clips.Length; i++)
            {
                if (CurrentAbilityObj.Clip == clips[i])
                {
                    m_SelectedClipIndex = i + 1;
                    ClipExist = true;
                }
            }

            if (!ClipExist)
            {
                m_SelectedClipIndex = 0;
            }

            List<string> clipsNames = new List<string>();
            //clipsNames.Add("Null");
            for (int i = 0; i < clips.Length; i++)
            {
                clipsNames.Add(clips[i].name);
            }

            //string[] ClipNamesArray = clipsNames.ToArray();

            GenericMenu menu = new GenericMenu();
            if (EditorGUILayout.DropdownButton(new GUIContent("Select Clip From Animator"), FocusType.Passive))
            {
                for (int i = 0; i < clipsNames.Count; i++)
                {
                    int page = i / 40;
                    string menuName = $"{page}/{clipsNames[i]}";
                    menu.AddItem(new GUIContent(menuName), false, (object index) =>
                    {
                        int ClipIndex = 0;
                        int.TryParse(index.ToString(), out ClipIndex);
                        CurrentAbilityObj.Clip = clips[ClipIndex];

                        m_CombatEditor.LoadL3();
                        //combatEditor.Repaint();
                    }, i);
                    menu.ShowAsContext();
                }
            }

            m_CombatEditor.LoadL3();
        }

        //ReorderableList NodeList;

        SerializedObject CombatControllerSO;

        public void InitNodeReorableList()
        {
            CombatControllerSO = new SerializedObject(m_CombatEditor.SelectedController);
            /*
            NodeList = new ReorderableList(CombatControllerSO, CombatControllerSO.FindProperty("Nodes"), true, true,true, true);
            NodeList.drawHeaderCallback = (Rect rect) => { GUI.Label(rect, "CharacterNodes"); };
            NodeList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 2, rect.width, rect.height),
                    NodeList.serializedProperty.GetArrayElementAtIndex(index));
            };
            */
        }

        public void SelectCombatConfig()
        {
            //CombatControllerSO = new SerializedObject(combatEditor.SelectedController);
            m_CombatEditor.CurrentInspectedType = CombatEditor.InspectedType.CombatConfig;
            Repaint();
            InitNodeReorableList();
        }
    }
}