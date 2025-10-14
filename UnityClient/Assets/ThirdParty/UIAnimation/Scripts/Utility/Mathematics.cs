using UnityEngine;
using System.Collections;

namespace SakashoUISystem
{
    public static class Mathematics
    {
        public static float LerpFloat(float from, float to, float t)
        {
            return from + t * (to - from);
        }
        
        public static Vector3 LerpVec3(Vector3 from, Vector3 to, float t)
        {
            return new Vector3(
                Mathematics.LerpFloat(from.x, to.x, t),
                Mathematics.LerpFloat(from.y, to.y, t),
                Mathematics.LerpFloat(from.z, to.z, t)
            );
        }
        
        public static Vector2 QuadraticBezier2(Vector2 start, Vector2 end, Vector2 control, float t)
        {
            return (1f - t) * (1f - t) * start + 2 * (1f - t) * t * control + t * t * end;
        }
        
        public static Vector2 QuadraticBezier3(Vector3 start, Vector3 end, Vector3 control, float t)
        {
            return (1f - t) * (1f - t) * start + 2 * (1f - t) * t * control + t * t * end;
        }
        
        public static Vector2 CubicBezier2(Vector2 start, Vector2 end, Vector2 control1, Vector2 control2, float t)
        {
            return (1f - t) * (1f - t) * (1f - t) * start + 3 * (1f - t) * (1f - t) * t * control1 + 3 * (1f - t) * t * t * control2 + t * t * t * end;
        }
        
        public static Vector3 CubicBezier3(Vector3 start, Vector3 end, Vector3 control1, Vector3 control2, float t)
        {
            return (1f - t) * (1f - t) * (1f - t) * start + 3 * (1f - t) * (1f - t) * t * control1 + 3 * (1f - t) * t * t * control2 + t * t * t * end;
        }
        
        public static float EaseInQuad(float start, float end, float value)
        {
            end -= start;
            return end * value * value + start;
        }
        
        public static float EaseOutQuad(float start, float end, float value)
        {
            end -= start;
            return -end * value * (value - 2) + start;
        }
        
        public static float EaseInCubic(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value + start;
        }
        
        public static float EaseOutCubic(float start, float end, float value)
        {
            value--;
            end -= start;
            return end * (value * value * value + 1) + start;
        }
        
        public static float EaseInQuart(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value * value + start;
        }
        
        public static float EaseOutQuart(float start, float end, float value)
        {
            value--;
            end -= start;
            return -end * (value * value * value * value - 1) + start;
        }

        public static void ChangePivot(RectTransform rectTrans, Vector2 targetPivot)
        {            
            var deltaPivot = targetPivot - rectTrans.pivot;
            rectTrans.pivot = targetPivot;
            rectTrans.anchoredPosition += new Vector2(deltaPivot.x * rectTrans.rect.width, deltaPivot.y * rectTrans.rect.height);         
        }

        public static float Frac(float value)
        {
            return value - Mathf.FloorToInt(value);
        }

        public static System.Text.RegularExpressions.Regex GenerateRegex(string pattern)
        {
            return new System.Text.RegularExpressions.Regex(pattern);
        }

        public static string ExtractTags(string originalString)
        {
            var pattern = @"<(""[^""]*""|'[^']*'|[^'"">])*>";
            var regex = new System.Text.RegularExpressions.Regex(pattern);
            return regex.Replace(originalString, "");
        }
    }
}