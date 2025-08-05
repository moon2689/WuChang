using System;
using System.Collections.Generic;
using System.Linq;
using RootMotion.FinalIK;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class ActorHitReaction
    {
        private SActor m_Actor;

        private HitReaction m_IKHitReaction;

        //private Transform[] m_Bones;
        //private Transform[] m_IKBones;
        //private Dictionary<string, Collider> m_DicColliders;
        private Dictionary<HurtBox, Collider> m_DicColliders;


        public ActorHitReaction(SActor actor)
        {
            m_Actor = actor;
            GameApp.Entry.Unity.DoActionOneFrameLater(InitIKHitReaction);
        }

        Collider GetCollider(string transName)
        {
            var item = m_IKHitReaction.effectorHitPoints.FirstOrDefault(a => a.collider.name == transName);
            return item != null ? item.collider : null;
        }

        void FindParentColliderRecursive(Transform trans, ref Collider collider)
        {
            if (trans == null)
            {
                return;
            }

            collider = GetCollider(trans.name);
            if (collider)
            {
                return;
            }

            FindParentColliderRecursive(trans.parent, ref collider);
        }

        void FindChildColliderRecursive(Transform trans, ref Collider collider)
        {
            if (trans == null)
            {
                return;
            }

            collider = GetCollider(trans.name);
            if (collider)
            {
                return;
            }

            foreach (Transform c in trans.transform)
            {
                FindChildColliderRecursive(c, ref collider);
                if (collider)
                {
                    return;
                }
            }
        }

        void InitIKHitReaction()
        {
            m_IKHitReaction = m_Actor.transform.GetComponentInChildren<HitReaction>();
            if (m_IKHitReaction == null)
            {
                return;
            }

            m_IKHitReaction.gameObject.SetLayerRecursive(EStaticLayers.Collider);
            for (int i = 0; i < m_IKHitReaction.transform.childCount; i++)
            {
                Transform ikBone = m_IKHitReaction.transform.GetChild(i);
                Rigidbody rb = ikBone.GetComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.excludeLayers = EStaticLayers.Actor.GetLayerMask() | EStaticLayers.Collider.GetLayerMask();
            }

            m_DicColliders = new();
            var hurtBoxes = m_Actor.CMelee.HurtBoxes;
            foreach (var hurtBox in hurtBoxes)
            {
                Collider c = null;
                FindParentColliderRecursive(hurtBox.transform.parent, ref c);
                if (c == null)
                {
                    FindChildColliderRecursive(hurtBox.transform.parent, ref c);
                }

                if (c)
                {
                    m_DicColliders.Add(hurtBox, c);
                }
            }

            /*
            foreach (Transform c in m_IKHitReaction.transform)
            {
                c.gameObject.SetActive(false);
            }

            // Transform boneHips = m_Actor.CBones[CharacterBones.EBone.Hips];
            // Transform boneChest = m_Actor.CBones[CharacterBones.EBone.Chest];
            // Debug.Log("hips:" + boneHips.name, boneHips);
            // Debug.Log("chest:" + boneChest.name, boneChest);
            for (int i = 0; i < m_IKHitReaction.effectorHitPoints.Length; i++)
            {
                var hitPoint = m_IKHitReaction.effectorHitPoints[i];
                string colliderName = hitPoint.collider.name;
                Transform bone = m_Actor.CBones[colliderName];
                bone.gameObject.AddComponent<Collider>();
            }
            */

            /*
            m_IKBones = new Transform[m_IKHitReaction.transform.childCount];
            m_Bones = new Transform[m_IKHitReaction.transform.childCount];
            for (int i = 0; i < m_IKHitReaction.transform.childCount; i++)
            {
                Transform ikBone = m_IKHitReaction.transform.GetChild(i);
                Transform bone = null;
                m_Actor.GetNodeTransform(ENodeType.RootBone).FindChildRecursive(ikBone.name, ref bone);
                m_IKBones[i] = ikBone;
                m_Bones[i] = bone;

                m_DicColliders[ikBone.name] = ikBone.GetComponent<Collider>();

                Rigidbody rb = ikBone.GetComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.excludeLayers = EStaticLayers.Actor.GetLayerMask() | EStaticLayers.Collider.GetLayerMask();
            }
            */
        }

        public void OnHit(HurtBox hurtBox, Vector3 force, Vector3 point)
        {
            if (m_IKHitReaction)
            {
                if (m_DicColliders.TryGetValue(hurtBox, out var c))
                {
                    m_IKHitReaction.Hit(c, force, point);
                }
                else
                {
                    Debug.LogError($"Unknown hurt box:{hurtBox}");
                }
            }
        }

        /*
        public void Update()
        {
            for (int i = 0; i < m_IKBones.Length; i++)
            {
                m_IKBones[i].position = m_Bones[i].position;
                m_IKBones[i].rotation = m_Bones[i].rotation;
            }
        
        }*/
    }
}