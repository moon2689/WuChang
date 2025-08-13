using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class RayWeapon : WeaponBase
    {
        const float k_RayDamageTimeInterval = 0.3f; // 同一个技能内，每一个角色每隔0.3s受一次伤害

        [SerializeField] float m_RayMinDistanceInterval = 0.5f;

        // 射线检测伤害
        Vector3[] m_LastFrameRayPoint;
        protected RaycastHit[] m_RaycastHit = new RaycastHit[20];
        List<SActor> m_ListRayHurtedActors = new();
        List<float> m_ListRayHurtActorTime = new();
        private bool m_EnableRayDamage;
        private float m_ShowHitGroundEffectTime;
        
        protected float RayInterval { get; private set; }


        public override void ToggleDamage(WeaponDamageSetting damage, bool enable)
        {
            base.ToggleDamage(damage, enable);
            m_EnableRayDamage = enable;

            m_ListRayHurtedActors.Clear();
            m_ListRayHurtActorTime.Clear();

            if (enable)
            {
                for (int i = 0; i < m_LastFrameRayPoint.Length; i++)
                {
                    m_LastFrameRayPoint[i] = GetRayPointPos(i);
                }
            }
        }

        void Awake()
        {
            float length = Vector3.Distance(m_PosEnd.position, m_PosStart.position);
            int rayPointCount = Mathf.CeilToInt(length / m_RayMinDistanceInterval) + 1;
            RayInterval = length / (rayPointCount - 1);
            m_LastFrameRayPoint = new Vector3[rayPointCount];
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (m_EnableRayDamage)
            {
                RayCast();
            }
        }

        void RayCast()
        {
            for (int i = 0; i < m_LastFrameRayPoint.Length; i++)
            {
                Vector3 lastFramePos = m_LastFrameRayPoint[i];
                Vector3 nowFramePos = GetRayPointPos(i);
                if (nowFramePos == lastFramePos)
                {
                    return;
                }

                m_LastFrameRayPoint[i] = nowFramePos;
                int hits = Raycast(nowFramePos, lastFramePos);
                Vector3 dir = nowFramePos - lastFramePos;

                for (int j = 0; j < hits; j++)
                {
                    OnRaycast(m_RaycastHit[j], dir);
                }

                if (GameApp.Entry.Config.GameSetting.DebugFight)
                {
                    Debug.DrawRay(lastFramePos, dir, Color.green, 3);
                }
            }

            if (GameApp.Entry.Config.GameSetting.DebugFight)
            {
                Debug.DrawLine(m_PosStart.position, m_PosEnd.position, Color.green, 3);
            }
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
            float weight = (float)index / (m_LastFrameRayPoint.Length - 1);
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
            if (CanDoDamage(target))
            {
                base.DoDamage(hit.point, dir, hb);
                OnDamageDone(target);
                if (GameApp.Entry.Config.GameSetting.DebugFight)
                {
                    SDebug.DrawWireSphere(hit.point, 0.1f, Color.red, 3);
                }

                return true;
            }

            return false;
        }

        void CreateEffectHitGround(Vector3 hitPos)
        {
            if (Time.time - m_ShowHitGroundEffectTime < 0.2f)
            {
                return;
            }

            m_ShowHitGroundEffectTime = Time.time;
            GameObject prefabHit = GameApp.Entry.Config.GameSetting.GetRandomEffectPrefab_WeaponHitGround();
            GameApp.Entry.Game.Effect.CreateEffect(prefabHit, null, hitPos, Quaternion.identity, 3f);

            AudioClip sound = GameApp.Entry.Config.GameSetting.GetRandomSound_WeaponHitGround();
            GameApp.Entry.Game.Audio.Play3DSound(sound, hitPos);
        }


        bool CanDoDamage(SActor target)
        {
            if (target != null && target != Actor)
            {
                int index = m_ListRayHurtedActors.FindIndex(a => a == target);
                return index < 0 || Time.time > m_ListRayHurtActorTime[index] + k_RayDamageTimeInterval;
            }

            return false;
        }

        void OnDamageDone(SActor target)
        {
            int index = m_ListRayHurtedActors.FindIndex(a => a == target);
            if (index < 0)
            {
                m_ListRayHurtedActors.Add(target);
                m_ListRayHurtActorTime.Add(Time.time);
            }
            else
            {
                m_ListRayHurtActorTime[index] = Time.time;
            }

            Actor.CurrentSkill.OnDamageDone();
        }
    }
}