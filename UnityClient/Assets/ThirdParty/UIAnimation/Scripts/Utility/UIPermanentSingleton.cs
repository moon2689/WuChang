using UnityEngine;
using System;
using System.Collections;

namespace SakashoUISystem
{
    public class UIPermanentSingleton<T> : MonoBehaviour where T : UIPermanentSingleton<T>
    {
        private static T instance;
        public static T Instance
        {
            get {
                if (instance == null) {
                    instance = FindObjectOfType(typeof(T)) as T;
                }
                
                if (instance == null) {
                    instance = CreateInstance();
                }
                
                return instance;
            }
        }
        
        private static T CreateInstance()
        {
            var obj = new GameObject(typeof(T).Name);                
            instance = obj.AddComponent<T>();
            DontDestroyOnLoad(obj);
            
            return instance;
        }
        
        public static bool HasInstance()
        {
            return instance != null;
        }
        
        private void OnDestroy()
        {
            if (instance == null) {
                return;
            }
            if (instance.gameObject == null) {
                return;
            }
            GameObject.Destroy(instance);
            instance = null;
        }
    }
}