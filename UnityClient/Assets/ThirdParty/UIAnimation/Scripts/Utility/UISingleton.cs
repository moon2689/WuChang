using UnityEngine;
using System.Collections;

namespace SakashoUISystem
{
    public class UISingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        
        public static T Instance { get { return instance; } }
        
        public static T CreateInstance()
        {
            if (instance == null) {
                instance = (T)FindObjectOfType(typeof(T));
            }
            return instance;
        }

        private void OnDestroy()
        {
            instance = null;
        }
    }
}