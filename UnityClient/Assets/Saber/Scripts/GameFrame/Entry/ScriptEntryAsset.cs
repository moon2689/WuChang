using Saber.CharacterController;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Saber.Frame
{
    public class ScriptEntryAsset
    {
        private Texture2D m_TextureUnknown;

        private Texture2D TextureUnknown
        {
            get
            {
                if (m_TextureUnknown == null)
                {
                    m_TextureUnknown = Resources.Load<Texture2D>("Image/Unknown");
                }

                return m_TextureUnknown;
            }
        }

        public GameObject LoadGameObject(string path)
        {
            GameObject asset = Resources.Load<GameObject>(path);
            GameObject go = GameObject.Instantiate(asset);
            return go;
        }

        public Texture2D LoadTexture(string path)
        {
            return Resources.Load<Texture2D>(path) ?? TextureUnknown;
        }

        public AnimationClip LoadClip(string path)
        {
            return Resources.Load<AnimationClip>(path);
        }

        public PhysicMaterial LoadPhysicMaterial(string path)
        {
            return Resources.Load<PhysicMaterial>(path);
        }

        public PhysicMaterial LoadPhysicMaterial(EPhysicMaterialType physicMaterialType)
        {
            string path = $"Config/PhysicMaterial/{physicMaterialType}";
            return LoadPhysicMaterial(path);
        }

        /*
        public BundleNormal<T> CreateBundleNormal<T>(string packageName, string address) where T : UObject
        {
            return new BundleNormal<T>(packageName, address);
        }

        public BundleNormal<T> CreateBundleNormal<T>(string address) where T : UObject
        {
            return CreateBundleNormal<T>(YooAssetPackages.PackageName_Main, address);
        }

        public bool IsAddressValid(string address)
        {
            return YooAssetPackages.PackageMain.CheckLocationValid(address);
        }

        public BundlePrefab CreateBundlePrefab(string packageName, string address)
        {
            return new BundlePrefab(packageName, address);
        }

        public BundlePrefab CreateBundlePrefab(string address)
        {
            return CreateBundlePrefab(YooAssetPackages.PackageName_Main, address);
        }

        public BundleAnimCtrl CreatebundleAnimCtrl(string packageName, string address)
        {
            return new BundleAnimCtrl(packageName, address);
        }

        public BundleAnimCtrl CreatebundleAnimCtrl(string address)
        {
            return CreatebundleAnimCtrl(YooAssetPackages.PackageName_Main, address);
        }

        public BundleScene CreateBundleScene(string packageName, string address)
        {
            return new BundleScene(packageName, address);
        }

        public BundleScene CreateBundleScene(string address)
        {
            return CreateBundleScene(YooAssetPackages.PackageName_Main, address);
        }*/
    }
}