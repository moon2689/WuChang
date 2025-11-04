using System;
using System.Collections.Generic;
using CombatEditor;
using UnityEngine;

namespace Saber.CharacterController
{
    public class ProjectileBezierCurve : Projectile
    {
        public enum EOffsetPointStyle
        {
            Up,
            Left,
            Right,
        }

        private Vector3 m_Pos1;
        private Transform m_Pos3Trans;
        private Vector3 m_Pos3;
        private Vector3 m_Pos2;
        private float m_TotalTime;
        private float m_Timer;
        private Vector3 m_LastPos;


        public EOffsetPointStyle OffsetPointStyle { get; set; }


        public override void Throw(SActor owner, SActor target)
        {
            base.Throw(owner, target);

            m_Pos1 = transform.position;
            Transform ownerTrans = owner.transform;
            float offsetDis = 3;
            m_Pos2 = OffsetPointStyle switch
            {
                EOffsetPointStyle.Up => m_Pos1 + (ownerTrans.forward + ownerTrans.up) * offsetDis,
                EOffsetPointStyle.Left => m_Pos1 + (ownerTrans.forward - ownerTrans.right) * offsetDis,
                EOffsetPointStyle.Right => m_Pos1 + (ownerTrans.forward + ownerTrans.right) * offsetDis,
                _ => throw new InvalidOperationException(),
            };

            if (target)
            {
                m_Pos3Trans = target.GetNodeTransform(ENodeType.LockUIPos);
                m_Pos3 = target.transform.position;
            }
            else
            {
                m_Pos3 = m_Pos1 + owner.transform.forward * 15 - Vector3.up * 3;
            }

            m_TotalTime = ((m_Pos1 - m_Pos2).magnitude + (m_Pos3 - m_Pos2).magnitude) / m_Speed;

            m_Timer = 0;

            m_LastPos = transform.position;

            transform.rotation = Quaternion.LookRotation(m_Pos2 - m_Pos1);
        }

        protected override void Fly()
        {
            m_Timer += Time.deltaTime;
            float t = m_Timer / m_TotalTime;

            Vector3 p3 = m_Pos3Trans ? m_Pos3Trans.position : m_Pos3;
            Vector3 curPos = GameHelper.CalcBezierCurve(m_Pos1, m_Pos2, p3, t);
            Vector3 dir = curPos - m_LastPos;
            transform.position = curPos;
            transform.rotation = Quaternion.LookRotation(dir);
            m_LastPos = curPos;

            if (t >= 1)
            {
                Impact();
            }
        }
    }
}