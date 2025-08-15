using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class CharacterRender
    {
        class OriginMaterialData
        {
            public Shader m_OriginShader;
            public int m_Queue;
            public Shader m_OpaqueShader;
        }


        private SCharacter m_Character;
        private Shader m_ShaderShadow; //透明边缘泛光Shader
        private List<Material> m_ShadowMaterials = new();
        private bool m_IsInWater;
        private GameObject[] m_FlyTrailEffects;
        private GameObject m_WaterWaveParticle;
        private Coroutine m_CoroutineHideWaterWaveParticle;
        private Dictionary<Material, OriginMaterialData> m_DicMatOriginData = new();


        /// <summary>是否在水里，在水中行走，或者游泳</summary>
        public bool IsInWater
        {
            get => m_IsInWater;
            private set
            {
                m_IsInWater = value;
                ActiveWaterWave(value);
                AdjustMaterialByWater(value);
                GetWet(value);
            }
        }


        public CharacterRender(SCharacter cha)
        {
            m_Character = cha;
            CreateWaterWaveParticle();

            cha.EventOnTriggerEnter += OnTriggerEnter;
            cha.EventOnTriggerExit += OnTriggerExit;

            // m_LookTargetController = m_Character.GetComponent<LookTargetController>();
            // m_EyeAndHeadAnimator = m_Character.GetComponent<EyeAndHeadAnimator>();
        }

        public void Release()
        {
        }

        public void OnGodStatueRest()
        {
            IsInWater = false;
        }


        #region 残影

        public void ShowOneChaShadow(float holdTime)
        {
            ShowChaShadowItor(holdTime).StartCoroutine();
        }

        public void ShowManyChaShadow(int count, float interval, float holdTime)
        {
            ShowManyChaShadowItor(count, interval, holdTime).StartCoroutine();
        }

        IEnumerator ShowManyChaShadowItor(int count, float interval, float holdTime)
        {
            for (int i = 0; i < count; i++)
            {
                ShowOneChaShadow(holdTime);
                yield return new WaitForSeconds(interval);
            }
        }

        IEnumerator ShowChaShadowItor(float holdSeconds)
        {
            if (m_ShaderShadow == null)
            {
                m_ShaderShadow = Shader.Find("Saber/Unlit/CharacterShadowRim");
            }

            m_ShadowMaterials.Clear();

            GameObject shadowGO = new GameObject($"Shadow_{m_Character.name}_{Time.time}");
            SkinnedMeshRenderer[] smrArray = m_Character.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < smrArray.Length; i++)
            {
                var smr = smrArray[i];
                GameObject go = new GameObject(smr.name);
                go.transform.SetParent(shadowGO.transform);
                go.transform.position = smr.transform.position;
                go.transform.rotation = smr.transform.rotation;
                go.transform.localScale = smr.transform.localScale;

                var meshRenderer = go.AddComponent<MeshRenderer>();
                var meshFilter = go.AddComponent<MeshFilter>();

                var mesh = new Mesh() { name = go.name };
                smr.BakeMesh(mesh); //烘焙残影
                meshFilter.sharedMesh = mesh;
                var mat = new Material(m_ShaderShadow);
                //mat.CopyPropertiesFromMaterial(smr.sharedMaterial);

                meshRenderer.sharedMaterial = mat;
                m_ShadowMaterials.Add(mat);
            }

            MeshRenderer[] mrArray = m_Character.transform.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < mrArray.Length; i++)
            {
                var mr = mrArray[i];
                GameObject go = new GameObject(mr.name);
                go.transform.SetParent(shadowGO.transform);
                go.transform.position = mr.transform.position;
                go.transform.rotation = mr.transform.rotation;
                go.transform.localScale = mr.transform.localScale;

                var meshRenderer = go.AddComponent<MeshRenderer>();
                var meshFilter = go.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mr.GetComponent<MeshFilter>().sharedMesh;

                var mat = new Material(m_ShaderShadow);
                meshRenderer.sharedMaterial = mat;

                m_ShadowMaterials.Add(mat);
            }

            yield return null;

            //残影消隐插值
            float timer = 0;
            float duration = holdSeconds;
            while (true)
            {
                timer += Time.deltaTime;
                float weight = Mathf.Clamp01(timer / duration);
                if (weight > 1)
                {
                    weight = 1;
                }

                Color newColor = new Color(0, 1, 1, 1 - weight);
                for (int i = 0; i < m_ShadowMaterials.Count; i++)
                    m_ShadowMaterials[i].color = newColor;

                yield return null;

                if (weight == 1)
                {
                    break;
                }
            }

            //销毁残影
            GameObject.Destroy(shadowGO);
        }

        #endregion


        #region 水

        public static bool IsWater(GameObject gameObject)
        {
            return gameObject.layer == EStaticLayers.Water.GetLayer();
        }

        void CreateWaterWaveParticle()
        {
            GameObject go = GameApp.Entry.Asset.LoadGameObject("Game/WaterWaveParticle");
            go.transform.SetParent(m_Character.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            go.SetActive(false);
            WaterWaveRenderer.SetActiveCamera(false);
            m_WaterWaveParticle = go;
        }

        public void ActiveWaterWave(bool active)
        {
            if (m_CoroutineHideWaterWaveParticle != null)
            {
                m_CoroutineHideWaterWaveParticle.StopCoroutine();
            }

            if (active)
            {
                m_WaterWaveParticle.SetActive(true);
                WaterWaveRenderer.SetActiveCamera(true);
            }
            else
            {
                m_CoroutineHideWaterWaveParticle =
                    GameApp.Entry.Unity.DoDelayAction(5, () =>
                    {
                        m_WaterWaveParticle.SetActive(false);
                        WaterWaveRenderer.SetActiveCamera(false);
                        m_CoroutineHideWaterWaveParticle = null;
                    });
            }
        }

        // 解决从水上看水下时，半透明材质被裁掉的问题
        void AdjustMaterialByWater(bool toOpaque)
        {
            SkinnedMeshRenderer[] smrArray = m_Character.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < smrArray.Length; i++)
            {
                var smr = smrArray[i];
                for (int j = 0; j < smr.materials.Length; j++)
                {
                    var mat = smr.materials[j];
                    if (!m_DicMatOriginData.ContainsKey(mat) && mat.renderQueue >= 3000)
                    {
                        Shader opaqueShader = GetOpaqueShader(mat.shader);
                        if (opaqueShader)
                        {
                            OriginMaterialData matData = new()
                            {
                                m_OriginShader = mat.shader,
                                m_Queue = mat.renderQueue,
                                m_OpaqueShader = opaqueShader,
                            };

                            m_DicMatOriginData.Add(mat, matData);
                        }
                    }

                    if (m_DicMatOriginData.TryGetValue(mat, out var data))
                    {
                        mat.shader = toOpaque ? data.m_OpaqueShader : data.m_OriginShader;
                        mat.renderQueue = toOpaque ? 2000 : data.m_Queue;
                    }
                }
            }
        }

        Shader GetOpaqueShader(Shader shader)
        {
            if (shader.name == "Saber/Human/Cloth Silk Lit Transparent")
            {
                return Shader.Find("Saber/Human/Cloth Silk Lit");
            }
            else if (shader.name == "Saber/Human/Hair Lit")
            {
                return Shader.Find("Saber/Human/Hair Lit Clip");
            }

            return null;
        }

        void OnTriggerEnter(Collider other)
        {
            //Debug.Log($"OnTriggerEnter {other.name}  {other.gameObject.layer}  {EStaticLayers.Water.GetLayer()}", other);
            if (IsWater(other.gameObject))
            {
                IsInWater = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (IsWater(other.gameObject))
            {
                IsInWater = false;
            }
        }

        public void GetWet(bool wet)
        {
            SkinnedMeshRenderer[] smrArray = m_Character.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < smrArray.Length; i++)
            {
                var smr = smrArray[i];
                for (int j = 0; j < smr.materials.Length; j++)
                {
                    var mat = smr.materials[j];
                    if (mat.HasProperty("_Wet"))
                    {
                        if (wet)
                        {
                            mat.EnableKeyword("_WET_ON");
                            mat.SetFloat("_Wet", 1);
                        }
                        else
                        {
                            mat.DisableKeyword("_WET_ON");
                            mat.SetFloat("_Wet", 0);
                        }
                    }
                }
            }
        }

        #endregion


        #region 飞行

        /// <summary>切换飞行时的拖尾效果</summary>
        public void ToggleFlyTrailEffect(bool active)
        {
            if (m_FlyTrailEffects == null)
            {
                m_FlyTrailEffects = new GameObject[3];
                m_FlyTrailEffects[0] = GameApp.Entry.Game.Effect.GetOrCreateEffect("Particles/TrailWhenGlide",
                    m_Character.GetNodeTransform(ENodeType.LeftHand));
                m_FlyTrailEffects[1] = GameApp.Entry.Game.Effect.GetOrCreateEffect("Particles/TrailWhenGlide",
                    m_Character.GetNodeTransform(ENodeType.RightHand));
                m_FlyTrailEffects[2] =
                    GameApp.Entry.Game.Effect.GetOrCreateEffect("Particles/WindEffect", m_Character.transform);
            }

            for (int i = 0; i < m_FlyTrailEffects.Length; i++)
            {
                m_FlyTrailEffects[i].SetActive(active);
            }
        }

        #endregion


        /*
         private LookTargetController m_LookTargetController;
         private EyeAndHeadAnimator m_EyeAndHeadAnimator;
        private List<TrailEffect> m_TrailEffects;


        public void EyeLockAt(SActor actor)
        {
            EyeLockAt(actor != null ? actor.HeadBone : null);
        }
        
        public void EyeLockAt(Transform target)
        {
            if (m_LookTargetController)
            {
                m_LookTargetController.thirdPersonPlayerEyeCenter = target;
                m_LookTargetController.noticePlayerDistance = target ? 10 : 1;
                m_LookTargetController.lookAtPlayerRatio = target ? 0.9f : 0.5f;
            }
        }

        /// <summary>切换眼睛和头动画功能</summary>
        public void ToggleEyeLock(bool active)
        {
            if (m_LookTargetController)
            {
                m_EyeAndHeadAnimator.mainWeight = active ? 1 : 0;
            }
        }
        
        */
    }
}