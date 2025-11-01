using System;
using System.Collections.Generic;
using CombatEditor;
using UnityEngine;

namespace Saber.CharacterController
{
    public class ProjectileStraightLine : Projectile
    {
        private Vector3 m_Direction;

        public float OffsetAngle { get; set; }

        public override void Throw(SActor owner, SActor target)
        {
            base.Throw(owner, target);
            if (target)
            {
                m_Direction = target.transform.position + Vector3.up * target.CPhysic.CenterHeight - transform.position;
            }
            else
            {
                m_Direction = owner.transform.forward - Vector3.up * 0.1f;
            }

            if (OffsetAngle != 0)
            {
                m_Direction = Quaternion.AngleAxis(OffsetAngle, Vector3.up) * m_Direction;
            }

            m_Direction.Normalize();
            transform.rotation = Quaternion.LookRotation(m_Direction);
        }

        protected override void Fly()
        {
            transform.position += m_Direction * m_Speed * Time.deltaTime;
        }
    }
}