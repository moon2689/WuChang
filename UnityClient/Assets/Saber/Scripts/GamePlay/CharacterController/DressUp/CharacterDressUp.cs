using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MagicaCloth2;
using Saber.Config;
using Saber.Frame;

using UnityEngine;

namespace Saber.CharacterController
{
    public class CharacterDressUp
    {
        [Serializable]
        public class CharacterClothInfo
        {
            public bool m_Enable;
            public Transform m_RootBone;
            public int[] m_DefaultClothes; //一般指内衣，和默认头发
            public int[] m_StartClothes; //初始服饰
            public SkinnedMeshRenderer m_SMRBody;
            public SkinnedMeshRenderer m_SMRLeg;
            public SkinnedMeshRenderer m_SMRArm;
        }

        private SActor m_Actor;
        private CharacterClothInfo m_ClothInfo;
        private List<ClothItemInfo> m_DressingClothes = new();
        private List<Transform> m_ExtraBones = new();
        private Dictionary<EClothType, GameObject> m_DicDefaultClothes = new();
        private Quaternion m_LeftFootRot, m_LeftToeRot, m_RightFootRot, m_RightToeRot;
        private float m_ShoesHeight;


        public bool Enable => m_ClothInfo.m_Enable;
        public Quaternion LeftFootRot => m_LeftFootRot;
        public Quaternion RightFootRot => m_RightFootRot;

        public bool UpdateFootRotationForHighHeels { get; set; }


        public float ShoesHeight
        {
            get => m_ShoesHeight;
            private set
            {
                m_ShoesHeight = value;
                m_Actor.SetGroundOffset(value);
            }
        }


        public CharacterDressUp(SActor actor, CharacterClothInfo info)
        {
            m_Actor = actor;
            m_ClothInfo = info;
            if (Enable)
            {
                LoadDefaultClothes();
                //DressStartClothes();
            }
        }

        private void LoadDefaultClothes()
        {
            if (m_ClothInfo.m_DefaultClothes != null)
            {
                foreach (var id in m_ClothInfo.m_DefaultClothes)
                {
                    var clothInfo = GameApp.Entry.Config.ClothInfo.GetClothByID(id);
                    GameObject obj = LoadClothGameObject(clothInfo);
                    m_DicDefaultClothes[clothInfo.m_ClothType] = obj;
                }
            }
        }

        /// <summary>刷新默认衣服</summary>
        private void ResetDefaultClothes()
        {
            foreach (var pair in m_DicDefaultClothes)
            {
                bool isDressing;
                if (pair.Key == EClothType.Hair)
                {
                    isDressing = m_DressingClothes.Any(a => a.m_ClothType == EClothType.Hair ||
                                                            a.m_ClothType == EClothType.Full ||
                                                            a.m_ClothType == EClothType.FullNoShoes);
                }
                else if (pair.Key == EClothType.TopDown)
                {
                    isDressing = m_DressingClothes.Any(a => a.m_ClothType == EClothType.FullNoHair ||
                                                            a.m_ClothType == EClothType.TopDown ||
                                                            a.m_ClothType == EClothType.FullNoShoes ||
                                                            a.m_ClothType == EClothType.Full);
                }
                else if (pair.Key == EClothType.FullNoHair)
                {
                    isDressing = m_DressingClothes.Any(a => a.m_ClothType == EClothType.FullNoHair ||
                                                            a.m_ClothType == EClothType.TopDown ||
                                                            a.m_ClothType == EClothType.FullNoShoes ||
                                                            a.m_ClothType == EClothType.Full ||
                                                            a.m_ClothType == EClothType.Shoes);
                }
                else if (pair.Key == EClothType.Shoes)
                {
                    isDressing = m_DressingClothes.Any(a => a.m_ClothType == EClothType.Shoes ||
                                                            a.m_ClothType == EClothType.FullNoHair ||
                                                            a.m_ClothType == EClothType.Full);
                }
                else if (pair.Key == EClothType.Full)
                {
                    isDressing = m_DressingClothes.Any(a => a.m_ClothType == EClothType.Shoes ||
                                                            a.m_ClothType == EClothType.Hair ||
                                                            a.m_ClothType == EClothType.FullNoHair ||
                                                            a.m_ClothType == EClothType.FullNoShoes ||
                                                            a.m_ClothType == EClothType.TopDown ||
                                                            a.m_ClothType == EClothType.Full);
                }
                else if (pair.Key == EClothType.FullNoShoes)
                {
                    isDressing = m_DressingClothes.Any(a => a.m_ClothType == EClothType.Hair ||
                                                            a.m_ClothType == EClothType.FullNoHair ||
                                                            a.m_ClothType == EClothType.FullNoShoes ||
                                                            a.m_ClothType == EClothType.TopDown ||
                                                            a.m_ClothType == EClothType.Full);
                }
                else
                {
                    throw new InvalidOperationException("Unknown cloth:" + pair.Key);
                }

                pair.Value.SetActive(!isDressing);
            }
        }

        public void DressStartClothes()
        {
            if (!Enable)
            {
                return;
            }

            foreach (var id in m_ClothInfo.m_StartClothes)
            {
                DressCloth(id);
            }
        }

        public void DressClothes(int[] clothes)
        {
            if (!Enable)
            {
                return;
            }

            foreach (var id in clothes)
            {
                DressCloth(id);
            }
        }

        public bool IsDressing(int id)
        {
            return m_DressingClothes.Any(a => a.m_ID == id);
        }

        /// <summary>穿上某衣服之前，先脱下对应部位衣服</summary>
        private void UndressClothBeforeDress(EClothType toDressClotyType)
        {
            switch (toDressClotyType)
            {
                case EClothType.Hair:
                    UndressCloth(EClothType.Full);
                    UndressCloth(EClothType.FullNoShoes);
                    UndressCloth(EClothType.Hair);
                    break;

                case EClothType.Full:
                    UndressCloth(EClothType.Full);
                    UndressCloth(EClothType.Hair);
                    UndressCloth(EClothType.FullNoHair);
                    UndressCloth(EClothType.FullNoShoes);
                    UndressCloth(EClothType.TopDown);
                    UndressCloth(EClothType.Shoes);
                    break;

                case EClothType.FullNoHair:
                    UndressCloth(EClothType.Full);
                    UndressCloth(EClothType.FullNoHair);
                    UndressCloth(EClothType.FullNoShoes);
                    UndressCloth(EClothType.TopDown);
                    UndressCloth(EClothType.Shoes);
                    break;

                case EClothType.FullNoShoes:
                    UndressCloth(EClothType.Full);
                    UndressCloth(EClothType.Hair);
                    UndressCloth(EClothType.FullNoHair);
                    UndressCloth(EClothType.FullNoShoes);
                    UndressCloth(EClothType.TopDown);
                    break;

                case EClothType.TopDown:
                    UndressCloth(EClothType.Full);
                    UndressCloth(EClothType.FullNoHair);
                    UndressCloth(EClothType.FullNoShoes);
                    UndressCloth(EClothType.TopDown);
                    break;

                case EClothType.Shoes:
                    UndressCloth(EClothType.Full);
                    UndressCloth(EClothType.Shoes);
                    UndressCloth(EClothType.FullNoHair);
                    break;

                case EClothType.Chain:
                    UndressCloth(EClothType.Chain);
                    break;

                case EClothType.Earrings:
                    UndressCloth(EClothType.Earrings);
                    break;

                default:
                    throw new InvalidOperationException("Unknown cloth type:" + toDressClotyType);
            }

            ResetDefaultClothes();
        }

        /// <summary>穿衣服</summary>
        public void DressCloth(int id)
        {
            DressCloth(id, null);
        }

        public void DressCloth(int id, Action onDressed)
        {
            if (Enable)
                GameApp.Entry.Unity.StartCoroutineQueued(DressClothItor(id, onDressed));
        }

        private IEnumerator DressClothItor(int id, Action onDressed)
        {
            if (IsDressing(id))
            {
                yield break;
            }

            var clothInfo = GameApp.Entry.Config.ClothInfo.GetClothByID(id);
            if (clothInfo == null)
            {
                yield break;
            }

            // 先脱下对应部位的衣服
            UndressClothBeforeDress(clothInfo.m_ClothType);
            yield return null;

            // 穿上衣服
            GameObject obj = LoadClothGameObject(clothInfo);
            m_DressingClothes.Add(clothInfo);

            // 默认衣服
            ResetDefaultClothes();
            yield return null;

            OnDressClothFinished(clothInfo, obj);
            onDressed?.Invoke();
        }

        /// <summary>当穿衣服完成</summary>
        private void OnDressClothFinished(ClothItemInfo clothInfo, GameObject obj)
        {
            if (clothInfo.m_ClothType == EClothType.Shoes || clothInfo.m_ClothType == EClothType.FullNoHair)
            {
                string leftFootName = m_Actor.GetNodeTransform(ENodeType.LeftFoot).name;
                string leftToeName = m_Actor.GetNodeTransform(ENodeType.LeftToes).name;
                string rightFootName = m_Actor.GetNodeTransform(ENodeType.RightFoot).name;
                string rightToeName = m_Actor.GetNodeTransform(ENodeType.RightToes).name;
                Transform leftFoot = null;
                Transform leftToe = null;
                Transform rightFoot = null;
                Transform rightToe = null;
                obj.transform.FindChildRecursive(leftFootName, ref leftFoot);
                obj.transform.FindChildRecursive(leftToeName, ref leftToe);
                obj.transform.FindChildRecursive(rightFootName, ref rightFoot);
                obj.transform.FindChildRecursive(rightToeName, ref rightToe);
                m_LeftFootRot = leftFoot.localRotation;
                m_LeftToeRot = leftToe.localRotation;
                m_RightFootRot = rightFoot.localRotation;
                m_RightToeRot = rightToe.localRotation;

                ShoesHeight = clothInfo.m_ShoesHeight;
            }

            TryClipSkin(clothInfo, obj);
        }

        /// <summary>裁剪身体某一部分，避免穿模</summary>
        void TryClipSkin(ClothItemInfo clothInfo, GameObject obj)
        {
            if (clothInfo.m_ClothType != EClothType.FullNoHair && clothInfo.m_ClothType != EClothType.TopDown)
                return;

            ClearSkinClip();

            ClothBodyClipConfig clipConfig = obj.GetComponent<ClothBodyClipConfig>();
            if (clipConfig == null)
                return;

            foreach (var item in clipConfig.m_ClipItems)
            {
                SkinnedMeshRenderer tarSMR = item.m_ClipArea switch
                {
                    ClothBodyClipConfig.EClipArea.Arm => m_ClothInfo.m_SMRArm,
                    ClothBodyClipConfig.EClipArea.Leg => m_ClothInfo.m_SMRLeg,
                    ClothBodyClipConfig.EClipArea.Body => m_ClothInfo.m_SMRBody,
                    _ => throw new InvalidOperationException("Unknown:" + item.m_ClipArea),
                };

                if (item.m_ClipType == ClothBodyClipConfig.EClipType.Hide)
                {
                    tarSMR.gameObject.SetActive(false);
                }
                else if (item.m_ClipType == ClothBodyClipConfig.EClipType.ShaderClip)
                {
                    tarSMR.gameObject.SetActive(true);
                    tarSMR.material.EnableKeyword("_CLIPBODY_ON");
                    tarSMR.material.SetFloat("_ClipBody", 1);
                    tarSMR.material.SetTexture("_ClipBodyMaskMap", item.m_ClipMaskMap);
                }
                else
                {
                    throw new InvalidOperationException("Unknown:" + item.m_ClipType);
                }
            }
        }

        void ClearSkinClip()
        {
            ClearSkinClip(m_ClothInfo.m_SMRBody);
            ClearSkinClip(m_ClothInfo.m_SMRLeg);
            ClearSkinClip(m_ClothInfo.m_SMRArm);
        }

        void ClearSkinClip(SkinnedMeshRenderer smr)
        {
            smr.gameObject.SetActive(true);
            smr.material.DisableKeyword("_CLIPBODY_ON");
            smr.material.SetFloat("_ClipBody", 0);
        }

        IEnumerator TryClearClip()
        {
            yield return null;
            if (!m_Actor.GetComponentInChildren<ClothBodyClipConfig>())
            {
                ClearSkinClip();
            }
        }

        /// <summary>加载衣服gameObject</summary>
        private GameObject LoadClothGameObject(ClothItemInfo clothInfo)
        {
            // load obj
            GameObject obj = clothInfo.LoadGameObject();
            obj.name = clothInfo.m_PrefabName;
            obj.transform.parent = m_Actor.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            obj.SetLayerRecursive(EStaticLayers.Actor);

            // init magica cloth
            var mcClothList = obj.GetComponentsInChildren<MagicaCloth>();
            foreach (var mcCloth in mcClothList)
            {
                mcCloth.Initialize();
                mcCloth.DisableAutoBuild();
            }

            // set bones
            SkinnedMeshRenderer[] smrArray = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var smr in smrArray)
            {
                smr.rootBone = GetOrCreateNewBone(smr.rootBone);
                Transform[] bones = new Transform[smr.bones.Length];
                for (int i = 0; i < smr.bones.Length; i++)
                    bones[i] = GetOrCreateNewBone(smr.bones[i]);
                smr.bones = bones;
            }

            // build magica cloth
            Transform[] allBones = m_ClothInfo.m_RootBone.GetComponentsInChildren<Transform>();
            Dictionary<string, Transform> dicAllBones = new();
            foreach (var b in allBones)
            {
                dicAllBones[b.name] = b;
            }

            foreach (var mcCloth in mcClothList)
            {
                mcCloth.ReplaceTransform(dicAllBones);

                MagicaClothColliderConfig config = mcCloth.GetComponent<MagicaClothColliderConfig>();
                if (config)
                {
                    var colliders = config.GetMagicClothColliders(m_Actor);
                    foreach (var c in colliders)
                        mcCloth.SerializeData.colliderCollisionConstraint.colliderList.Add(c);
                }


                mcCloth.BuildAndRun();
            }

            return obj;
        }

        /// <summary>获取动骨</summary>
        private Transform GetOrCreateNewBone(Transform oldBone)
        {
            Transform bone = null;
            m_ClothInfo.m_RootBone.FindChildRecursive(oldBone.name, ref bone);
            if (bone)
            {
                return bone;
            }

            // 把额外骨骼，从衣服移到人物骨骼中
            Transform oldCurBone = oldBone;
            Transform oldBoneParent = oldBone.parent;
            Transform actorBoneParent = null;
            while (true)
            {
                m_ClothInfo.m_RootBone.FindChildRecursive(oldBoneParent.name, ref actorBoneParent);

                if (actorBoneParent)
                {
                    break;
                }

                if (actorBoneParent == null)
                {
                    oldCurBone = oldBoneParent;
                    oldBoneParent = oldBoneParent.parent;
                }
            }

            Vector3 localPosition = oldCurBone.localPosition;
            Quaternion localRot = oldCurBone.localRotation;
            Vector3 localScale = oldCurBone.localScale;
            oldCurBone.parent = actorBoneParent;
            oldCurBone.localPosition = localPosition;
            oldCurBone.localRotation = localRot;
            oldCurBone.localScale = localScale;
            m_ClothInfo.m_RootBone.FindChildRecursive(oldBone.name, ref bone);

            Transform[] extraBones = oldCurBone.GetComponentsInChildren<Transform>();
            m_ExtraBones.AddRange(extraBones);

            return bone;
        }

        /// <summary>脱下某衣服</summary>
        public void UndressCloth(int id)
        {
            if (!Enable)
                return;
            var clothInfo = m_DressingClothes.Find(a => a.m_ID == id);
            if (clothInfo == null)
                return;

            UndressCloth(clothInfo);

            ResetDefaultClothes();

            OnUndressClothFinished(clothInfo);
        }

        private void OnUndressClothFinished(ClothItemInfo clothInfo)
        {
            if (clothInfo.m_ClothType == EClothType.Shoes || clothInfo.m_ClothType == EClothType.FullNoHair)
            {
                m_LeftFootRot = m_LeftToeRot = m_RightFootRot = m_RightToeRot = Quaternion.identity;
                ShoesHeight = 0;
            }

            GameApp.Entry.Unity.StartCoroutine(TryClearClip());
        }

        private void UndressCloth(EClothType clothType)
        {
            var clothInfo = m_DressingClothes.Find(a => a.m_ClothType == clothType);
            if (clothInfo == null)
                return;
            UndressCloth(clothInfo);
            OnUndressClothFinished(clothInfo);
        }

        private void UndressCloth(ClothItemInfo clothInfo)
        {
            if (clothInfo == null)
                return;

            m_DressingClothes.Remove(clothInfo);

            Transform t = m_Actor.transform.Find(clothInfo.m_PrefabName);
            if (t)
                GameObject.Destroy(t.gameObject);

            // 移除废弃不用的额外骨骼
            for (int i = m_ExtraBones.Count - 1; i >= 0; --i)
            {
                Transform bone = m_ExtraBones[i];
                if (bone == null)
                {
                    m_ExtraBones.RemoveAt(i);
                    break;
                }

                bool isUsed = IsTheBoneAndChildrenUsed(bone);
                if (!isUsed)
                {
                    GameObject.Destroy(bone.gameObject);
                    m_ExtraBones.RemoveAt(i);
                }
            }
        }

        private GameObject GetClothObj(ClothItemInfo clothInfo)
        {
            Transform t = m_Actor.transform.Find(clothInfo.m_PrefabName);
            return t ? t.gameObject : null;
        }

        private bool IsTheBoneAndChildrenUsed(Transform bone)
        {
            var children = bone.GetComponentsInChildren<Transform>();
            return children.Any(a => IsTheBoneUsed(a));
        }

        private bool IsTheBoneUsed(Transform bone)
        {
            foreach (var c in m_DressingClothes)
            {
                GameObject obj = GetClothObj(c);
                SkinnedMeshRenderer[] smrArray = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var smr in smrArray)
                {
                    if (smr.rootBone == bone)
                        return true;

                    if (smr.bones.Any(a => a == bone))
                        return true;
                }
            }

            return false;
        }

        public void AfterFixedUpdate()
        {
            if (!Enable)
                return;
            UpdateShoesHeight();
        }

        /// <summary>更新高跟鞋</summary>
        private void UpdateShoesHeight()
        {
            Transform leftToe = m_Actor.GetNodeTransform(ENodeType.LeftToes);
            Transform rightToe = m_Actor.GetNodeTransform(ENodeType.RightToes);
            leftToe.localRotation *= m_LeftToeRot;
            rightToe.localRotation *= m_RightToeRot;

            if (UpdateFootRotationForHighHeels)
            {
                Transform leftFoot = m_Actor.GetNodeTransform(ENodeType.LeftFoot);
                Transform rightFoot = m_Actor.GetNodeTransform(ENodeType.RightFoot);
                leftFoot.localRotation *= m_LeftFootRot;
                rightFoot.localRotation *= m_RightFootRot;
            }
        }

        public int[] GetDressingClothes()
        {
            int[] ids = new int[m_DressingClothes.Count];
            for (int i = 0; i < m_DressingClothes.Count; i++)
            {
                ids[i] = m_DressingClothes[i].m_ID;
            }

            return ids;
        }
    }
}