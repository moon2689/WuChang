using System;
using System.Collections;
using System.Collections.Generic;
using MagicaCloth2;

using UnityEngine;

namespace Saber.CharacterController
{
    [RequireComponent(typeof(MagicaCloth))]
    public class MagicaClothColliderConfig : MonoBehaviour
    {
        public bool m_Head;
        public bool m_UpperChest;
        public bool m_UpperBody;

        public bool m_UpperArm;
        public bool m_LowerArm;

        public bool m_Hip;
        public bool m_ThighLeg;
        public bool m_CalfLeg;
        public bool m_Foot;

        public List<ColliderComponent> GetMagicClothColliders(SActor actor)
        {
            List<ColliderComponent> list = new();

            if (m_Head)
            {
                Transform bone = actor.GetNodeTransform(ENodeType.Neck);
                ColliderComponent[] colliderComponents = bone.GetComponentsInChildren<ColliderComponent>();
                list.AddRange(colliderComponents);
            }

            if (m_UpperChest)
            {
                Transform bone = actor.GetNodeTransform(ENodeType.UpperChest);
                var coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);

                bone = actor.GetNodeTransform(ENodeType.LeftShoulder);
                coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);

                bone = actor.GetNodeTransform(ENodeType.RightShoulder);
                coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);
            }
            else if (m_UpperBody)
            {
                Transform bone = actor.GetNodeTransform(ENodeType.UpperChest);
                var coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);

                bone = actor.GetNodeTransform(ENodeType.LeftShoulder);
                coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);

                bone = actor.GetNodeTransform(ENodeType.RightShoulder);
                coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);

                bone = actor.GetNodeTransform(ENodeType.Chest);
                coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);

                bone = actor.GetNodeTransform(ENodeType.Spine);
                coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);
            }

            if (m_UpperArm)
            {
                Transform bone = actor.GetNodeTransform(ENodeType.LeftUpperArm);
                var coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);

                bone = actor.GetNodeTransform(ENodeType.RightUpperArm);
                coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);
            }

            if (m_LowerArm)
            {
                Transform bone = actor.GetNodeTransform(ENodeType.LeftLowerArm);
                var coms = bone.GetComponentsInChildren<ColliderComponent>();
                list.AddRange(coms);

                bone = actor.GetNodeTransform(ENodeType.RightLowerArm);
                coms = bone.GetComponentsInChildren<ColliderComponent>();
                list.AddRange(coms);
            }

            if (m_Hip)
            {
                Transform bone = actor.GetNodeTransform(ENodeType.Hips);
                var coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);
            }

            if (m_ThighLeg)
            {
                Transform bone = actor.GetNodeTransform(ENodeType.LeftUpperLeg);
                var coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);

                bone = actor.GetNodeTransform(ENodeType.RightUpperLeg);
                coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);
            }

            if (m_CalfLeg)
            {
                Transform bone = actor.GetNodeTransform(ENodeType.LeftLowerLeg);
                var coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);

                bone = actor.GetNodeTransform(ENodeType.RightLowerLeg);
                coms = bone.GetComponentsInTopChildren<ColliderComponent>();
                list.AddRange(coms);
            }

            if (m_Foot)
            {
                Transform bone = actor.GetNodeTransform(ENodeType.LeftFoot);
                var coms = bone.GetComponentsInChildren<ColliderComponent>();
                list.AddRange(coms);

                bone = actor.GetNodeTransform(ENodeType.RightFoot);
                coms = bone.GetComponentsInChildren<ColliderComponent>();
                list.AddRange(coms);
            }

            return list;
        }
    }
}