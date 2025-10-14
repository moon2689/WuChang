using UnityEngine;
using System.Collections;

namespace SakashoUISystem
{
    public static class ColorLog
    {
        private static void LogColor(object msg, string color)
        {
            #if UNITY_EDITOR
            UnityEngine.Debug.Log("<color=" + color + ">" + msg + "</color>");
            #endif
        }

        public static void LogRed(object msg)
        {
            LogColor(msg, "red");
        }

        public static void LogGreen(object msg)
        {
            LogColor(msg, "green");
        }

        public static void LogBlue(object msg)
        {
            LogColor(msg, "blue");
        }

        public static void LogYellow(object msg)
        {
            LogColor(msg, "yellow");
        }

        public static void LogWhite(object msg)
        {
            LogColor(msg, "white");
        }

        public static void LogWarning(object msg)
        {
            #if UNITY_EDITOR
            UnityEngine.Debug.LogWarning(msg);
            #endif
        }
    }
}