using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityHelper
{
	public static bool IsEditor
    {
        get
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }
    }

    public static bool IsAndroid
    {
        get
        {
#if UNITY_ANDROID
            return true;
#else
            return false;
#endif
        }
    }

    public static bool IsIOS
    {
        get
        {
#if UNITY_IPHONE
            return true;
#else
            return false;
#endif
        }
    }
    
    /// <summary>returns the delta position from a rotation.</summary>
    public static Vector3 DeltaPositionFromRotate(this Transform transform, Vector3 platform, Quaternion deltaRotation)
    {
        var pos = transform.position;

        var direction = pos - platform;
        var directionAfterRotation = deltaRotation * direction;

        var NewPoint = platform + directionAfterRotation;


        pos = NewPoint - transform.position;

        return pos;
    }
}
