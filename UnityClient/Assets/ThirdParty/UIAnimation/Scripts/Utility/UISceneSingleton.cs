using UnityEngine;

namespace SakashoUISystem
{
    public abstract class UISceneSingleton<T> : MonoBehaviour where T : UISceneSingleton<T>
    {
        private static T instance;
        public static T Instance {
            get {
                if (instance == null) {
                    var go = new GameObject(typeof(T).Name);
                    instance = go.AddComponent<T>();
                    instance.OnInstantiated();
                }
                return instance;
            }
        }

        protected virtual void OnInstantiated() {}

        protected virtual void OnDestroyed() {}

        private void OnDestroy()
        {
            if (instance == null) {
                return;
            }
            instance.OnDestroyed();
                        
            Destroy(instance);
            instance = null;
        }
    }
}