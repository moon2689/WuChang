using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class RayWeapon : WeaponBase
    {
        [SerializeField] float m_RayMinDistanceInterval = 0.5f;

        // 射线检测伤害
        Vector3[] m_WeaponRayPoints;
        protected RaycastHit[] m_RaycastHit = new RaycastHit[20];
        List<SActor> m_ListRayHurtedActors = new();
        private bool m_EnableRayDamage;
        private float m_ShowHitGroundEffectTime;
        private bool m_ToCalcFirstFrameRayPoint;

        protected float RayInterval { get; private set; }


        public override void ToggleDamage(WeaponDamageSetting damage, bool enable)
        {
            base.ToggleDamage(damage, enable);
            m_EnableRayDamage = enable;
            m_ListRayHurtedActors.Clear();
            m_ToCalcFirstFrameRayPoint = true;
        }

        void Awake()
        {
            float length = Vector3.Distance(m_PosEnd.position, m_PosStart.position);
            int rayPointCount = Mathf.CeilToInt(length / m_RayMinDistanceInterval) + 1;
            RayInterval = length / (rayPointCount - 1);
            m_WeaponRayPoints = new Vector3[rayPointCount];
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (m_EnableRayDamage)
            {
                if (m_ToCalcFirstFrameRayPoint)
                {
                    m_ToCalcFirstFrameRayPoint = false;
                    for (int i = 0; i < m_WeaponRayPoints.Length; i++)
                    {
                        m_WeaponRayPoints[i] = GetRayPointPos(i);
                    }
                }
                else
                {
                    RayCast();
                }
            }
        }

        void RayCast()
        {
            for (int i = 0; i < m_WeaponRayPoints.Length; i++)
            {
                Vector3 lastFramePos = m_WeaponRayPoints[i];
                Vector3 nowFramePos = GetRayPointPos(i);
                if (nowFramePos == lastFramePos)
                {
                    return;
                }

                m_WeaponRayPoints[i] = nowFramePos;
                int hits = Raycast(nowFramePos, lastFramePos);
                Vector3 dir = nowFramePos - lastFramePos;

                for (int j = 0; j < hits; j++)
                {
                    OnRaycast(m_RaycastHit[j], dir);
                }

                //SDebug.DrawArrow(lastFramePos, dir, Color.white, 3);
            }

            //Debug.DrawLine(m_PosStart.position, m_PosEnd.position, Color.white, 3);
        }

        protected virtual int Raycast(Vector3 nowFramePos, Vector3 lastFramePos)
        {
            Vector3 dir = nowFramePos - lastFramePos;
            float maxDis = dir.magnitude;
            QueryTriggerInteraction queryType = QueryTriggerInteraction.Collide;
            int layer = EStaticLayers.Collider.GetLayerMask() | EStaticLayers.Default.GetLayerMask();
            int hits = Physics.RaycastNonAlloc(lastFramePos, dir, m_RaycastHit, maxDis, layer, queryType);
            return hits;
        }

        Vector3 GetRayPointPos(int index)
        {
            float weight = (float)index / (m_WeaponRayPoints.Length - 1);
            return Vector3.Lerp(m_PosEnd.position, m_PosStart.position, weight);
        }

        bool OnRaycast(RaycastHit hit, Vector3 dir)
        {
            Collider other = hit.collider;
            if (!other)
                return false;
            //Debug.Log($"Trigger enter:{other.name}, layer:{other.gameObject.layer}", other.gameObject);
            if (!other.isTrigger && other.gameObject.layer == (int)EStaticLayers.Default)
            {
                CreateEffectHitGround(hit.point);
            }

            HurtBox hb = other.GetComponent<HurtBox>();
            if (hb == null)
                return false;

            SActor target = hb.Actor;
            bool canDoDmg = target != null && target != Actor && !m_ListRayHurtedActors.Contains(target);
            if (canDoDmg)
            {
                Vector3 pos = FixDamagePos(other, hit);
                base.DoDamage(pos, dir, hb);
                OnDamageDone(target);
                //SDebug.DrawWireSphere(hit.point, 0.1f, Color.red, 3);

                return true;
            }

            return false;
        }

        Vector3 FixDamagePos(Collider c, RaycastHit hit)
        {
            Vector3 pos = hit.point != Vector3.zero ? hit.point : c.transform.position;
            // if (hit.point != Vector3.zero)
            //     return hit.point;

            //Vector3 dir = Actor.transform.position - pos;
            Vector3 dir = GameApp.Entry.Game.PlayerCamera.transform.position - pos;
            dir.y = 0;
            dir.Normalize();
            float offsetDis = 0.1f;
            if (c is SphereCollider sphereCollider)
            {
                offsetDis += sphereCollider.radius;
            }
            else if (c is CapsuleCollider capsuleCollider)
            {
                offsetDis += Mathf.Max(capsuleCollider.radius, capsuleCollider.height / 2f);
            }

            pos += offsetDis * dir;

            return pos;
        }

        void CreateEffectHitGround(Vector3 hitPos)
        {
            if (Time.time - m_ShowHitGroundEffectTime < 0.2f)
            {
                return;
            }

            m_ShowHitGroundEffectTime = Time.time;
            GameObject prefabHit = GameApp.Entry.Config.SkillCommon.GetRandomEffectPrefab_WeaponHitGround();
            GameApp.Entry.Game.Effect.CreateEffect(prefabHit, null, hitPos, Quaternion.identity, 3f);

            AudioClip sound = GameApp.Entry.Config.SkillCommon.GetRandomSound_WeaponHitGround();
            GameApp.Entry.Game.Audio.Play3DSound(sound, hitPos);
        }

        void OnDamageDone(SActor target)
        {
            m_ListRayHurtedActors.Add(target);
            Actor.CurrentSkill.OnDamageDone();
        }
    }
}