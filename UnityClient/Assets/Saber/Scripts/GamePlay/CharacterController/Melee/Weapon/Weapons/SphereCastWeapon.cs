using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class SphereCastWeapon : RayWeapon
    {
        protected override int Raycast(Vector3 nowFramePos, Vector3 lastFramePos)
        {
            Vector3 dir = nowFramePos - lastFramePos;
            float maxDis = dir.magnitude;
            dir.Normalize();
            QueryTriggerInteraction queryType = QueryTriggerInteraction.Collide;
            int layer = EStaticLayers.Collider.GetLayerMask() | EStaticLayers.Default.GetLayerMask();
            int hits = Physics.SphereCastNonAlloc(lastFramePos, RayInterval / 2f, dir,
                m_RaycastHit, maxDis, layer, queryType);
            //SDebug.DrawCapsule(lastFramePos, nowFramePos, Color.green, RayInterval / 2f, 10);
            return hits;
        }
    }
}