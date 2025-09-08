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
            public int[] m_DefaultClothes; //一般指内衣，和默认头发
            public int[] m_StartClothes; //初始服饰
            public SkinnedMeshRenderer[] m_DefaultSMR;
        }

        private SActor m_Actor;
        private CharacterClothInfo m_ClothInfo;
        private List<ClothItemInfo> m_DressingClothes = new();
        private List<Transform> m_ExtraBones = new();
        private Dictionary<EClothType, GameObject> m_DicDefaultClothes = new();
        private Quaternion m_LeftFootRot, m_LeftToeRot, m_RightFootRot, m_RightToeRot;
        private float m_ShoesHeight;
        private Transform m_RootBone;


        public bool Enable => m_ClothInfo.m_Enable;


        public CharacterDressUp(SActor actor, CharacterClothInfo info)
        {
            m_Actor = actor;
            m_ClothInfo = info;
            m_RootBone = actor.GetNodeTransform(ENodeType.RootBone);

            if (Enable)
            {
                foreach (var smr in info.m_DefaultSMR)
                {
                    smr.gameObject.SetActive(false);
                }

                LoadDefaultClothes();
                DressStartClothes();
            }
        }

        private void LoadDefaultClothes()
        {
            if (m_ClothInfo.m_DefaultClothes != null)
            {
                foreach (var id in m_ClothInfo.m_DefaultClothes)
                {
                    var clothInfo = GameApp.Entry.Config.ClothInfo.GetClothByID(id);
                    LoadClothGameObject(clothInfo, obj => m_DicDefaultClothes[clothInfo.m_ClothType] = obj);
                }
            }
        }

        /// <summary>刷新默认衣服</summary>
        private void ResetDefaultClothes()
        {
            foreach (var pair in m_DicDefaultClothes)
            {
                bool isDressing = m_DressingClothes.Any(a => a.m_ClothType == pair.Key);
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
            UndressCloth(toDressClotyType);
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
            GameObject clothObj = null;
            LoadClothGameObject(clothInfo, obj => clothObj = obj);
            while (clothObj == null)
            {
                yield return null;
            }

            m_DressingClothes.Add(clothInfo);

            // 默认衣服
            ResetDefaultClothes();
            yield return null;

            OnDressClothFinished(clothInfo, clothObj);
            onDressed?.Invoke();
        }

        /// <summary>当穿衣服完成</summary>
        private void OnDressClothFinished(ClothItemInfo clothInfo, GameObject obj)
        {
        }

        /// <summary>加载衣服gameObject</summary>
        private async void LoadClothGameObject(ClothItemInfo clothInfo, Action<GameObject> onLoaded)
        {
            // load obj
            GameObject clothObj = null;
            await clothInfo.LoadGameObject(go => clothObj = go).Task;

            clothObj.name = clothInfo.PrefabName;
            clothObj.transform.parent = m_Actor.transform;
            clothObj.transform.localPosition = Vector3.zero;
            clothObj.transform.localRotation = Quaternion.identity;
            clothObj.transform.localScale = Vector3.one;
            clothObj.SetLayerRecursive(EStaticLayers.Actor);
            clothObj.SetRenderingLayerRecursive(ERenderingLayers.Actor);

            // init magica cloth
            var mcClothList = clothObj.GetComponentsInChildren<MagicaCloth>();
            foreach (var mcCloth in mcClothList)
            {
                mcCloth.Initialize();
                mcCloth.DisableAutoBuild();
            }

            // set bones
            SkinnedMeshRenderer[] smrArray = clothObj.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var smr in smrArray)
            {
                smr.rootBone = GetOrCreateNewBone(smr.rootBone);
                Transform[] bones = new Transform[smr.bones.Length];
                for (int i = 0; i < smr.bones.Length; i++)
                    bones[i] = GetOrCreateNewBone(smr.bones[i]);
                smr.bones = bones;
            }

            // build magica cloth
            Transform[] allBones = m_RootBone.GetComponentsInChildren<Transform>();
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

            onLoaded?.Invoke(clothObj);
        }

        /// <summary>获取动骨</summary>
        private Transform GetOrCreateNewBone(Transform oldBone)
        {
            Transform bone = null;
            m_RootBone.FindChildRecursive(oldBone.name, ref bone);
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
                m_RootBone.FindChildRecursive(oldBoneParent.name, ref actorBoneParent);

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
            m_RootBone.FindChildRecursive(oldBone.name, ref bone);

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

            Transform t = m_Actor.transform.Find(clothInfo.PrefabName);
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
            Transform t = m_Actor.transform.Find(clothInfo.PrefabName);
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
                GameObject clothObj = GetClothObj(c);
                if (IsTheBoneUsed(bone, clothObj))
                {
                    return true;
                }
            }

            foreach (var pair in m_DicDefaultClothes)
            {
                if (IsTheBoneUsed(bone, pair.Value))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsTheBoneUsed(Transform bone, GameObject clothObj)
        {
            SkinnedMeshRenderer[] smrArray = clothObj.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var smr in smrArray)
            {
                if (smr.rootBone == bone)
                    return true;

                if (smr.bones.Any(a => a == bone))
                    return true;
            }

            return false;
        }
    }
}