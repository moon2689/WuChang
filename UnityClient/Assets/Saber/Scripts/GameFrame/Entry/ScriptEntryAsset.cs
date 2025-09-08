using System;
using Saber.CharacterController;
using UnityEngine;
using YooAsset;
using UObject = UnityEngine.Object;

namespace Saber.Frame
{
    public class ScriptEntryAsset
    {
        public AssetHandle LoadGameObject(string path, Action<GameObject> onLoaded)
        {
            return LoadAsset<GameObject>(path, prefab =>
            {
                GameObject go = GameObject.Instantiate((GameObject)prefab);
                onLoaded?.Invoke(go);
            });
        }

        public AssetHandle LoadAsset<T>(string path, Action<T> onLoaded) where T : UnityEngine.Object
        {
            return YooAssetManager.Instance.LoadAsset(path, onLoaded);
        }

        public AssetHandle LoadPhysicMaterial(EPhysicMaterialType physicMaterialType, Action<PhysicMaterial> onLoaded)
        {
            string path = $"Config/PhysicMaterial/{physicMaterialType}";
            return LoadAsset(path, onLoaded);
        }

        public SceneHandle LoadScene(string sceneName, Action onLoaded)
        {
            return YooAssetManager.Instance.LoadScene(sceneName, onLoaded);
        }
    }
}