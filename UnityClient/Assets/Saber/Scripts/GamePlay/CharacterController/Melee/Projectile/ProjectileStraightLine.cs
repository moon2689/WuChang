using System;
using System.Collections.Generic;
using CombatEditor;
using UnityEngine;

namespace Saber.CharacterController
{
    public class ProjectileStraightLine : Projectile
    {
        [SerializeField] private bool m_LockRotX;
        [SerializeField] private bool m_LockRotY;
        [SerializeField] private bool m_LockRotZ;

        private Vector3 m_Direction;

        public float OffsetAngle { get; set; }

        public override void Throw(SActor owner, SActor target)
        {
            base.Throw(owner, target);
            m_Direction = owner.transform.forward;
            if (target)
            {
                Vector3 dirToTarget = target.GetNodeTransform(ENodeType.Chest).position - transform.position;
                m_Direction.y = dirToTarget.normalized.y;
            }
            else
            {
                m_Direction.y = -0.1f;
            }
            /*
            if (target)
            {
                m_Direction = target.GetNodeTransform(ENodeType.Chest).position - transform.position;
            }
            else
            {
                m_Direction = owner.transform.forward - Vector3.up * 0.1f;
            }
            */

            if (OffsetAngle != 0)
            {
                m_Direction = Quaternion.AngleAxis(OffsetAngle, Vector3.up) * m_Direction;
            }

            m_Direction.Normalize();

            //transform.rotation = Quaternion.LookRotation(m_Direction);

            Quaternion tarRot = Quaternion.LookRotation(m_Direction);
            if (m_LockRotX || m_LockRotY || m_LockRotZ)
            {
                Vector3 tarRotEuler = tarRot.eulerAngles;
                if (m_LockRotX)
                    tarRotEuler.x = transform.rotation.eulerAngles.x;
                if (m_LockRotY)
                    tarRotEuler.y = transform.rotation.eulerAngles.y;
                if (m_LockRotZ)
                    tarRotEuler.z = transform.rotation.eulerAngles.z;
                tarRot = Quaternion.Euler(tarRotEuler);
            }

            transform.rotation = tarRot;
        }

        protected override void Fly()
        {
            transform.position += m_Direction * m_Speed * Time.deltaTime;
        }
    }
}