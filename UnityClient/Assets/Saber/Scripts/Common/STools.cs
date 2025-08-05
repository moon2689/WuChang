using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saber
{
    public static class STools
    {
        #region Math

        /// <summary>Check if x Seconds have elapsed since the Started Time </summary>
        public static bool ElapsedTime(float StartTime, float intervalTime) => (Time.time - StartTime) >= intervalTime;

        /// <summary> Takes a number and stores the digits on an array. E.g: 6542 = [6,5,4,2] </summary>

        public static bool DoSpheresIntersect(Vector3 center1, float radius1, Vector3 center2, float radius2)
        {
            float squaredDistance = (center1 - center2).sqrMagnitude;
            float squaredRadii = Mathf.Pow(radius1 + radius2, 2);

            return squaredDistance <= squaredRadii;
        }
        
        #endregion

        #region Layers

        /// <summary>True if the colliders layer is on the layer mask</summary>
        public static bool CollidersLayer(Collider collider, LayerMask layerMask) => layerMask == (layerMask | (1 << collider.gameObject.layer));

        #endregion
    }
}