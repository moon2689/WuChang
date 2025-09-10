using System;
using System.Collections;
using JetBrains.Annotations;
using Saber.CharacterController;
using Saber.Config;
using Saber.Frame;
using YooAsset;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Saber
{
    public class YooAssetManager
    {
        private ResourcePackage m_Package;
        private string m_PackageVersion;

        private static YooAssetManager s_Instance;
        public static YooAssetManager Instance => s_Instance ??= new();
        public float Percent { get; private set; }
        public bool IsDone { get; private set; }


        private YooAssetManager()
        {
        }

        public Coroutine Init()
        {
            return InitItor().StartCoroutine();
        }

        IEnumerator InitItor()
        {
            Percent = 0;
            IsDone = false;

            YooAssets.Initialize();
            m_Package = YooAssets.CreatePackage("DefaultPackage");
            YooAssets.SetDefaultPackage(m_Package);

#if UNITY_EDITOR
            if (GameBaseSetting.Instance.EditorUseBundleAsset)
            {
                yield return InitPackageOfflineMode().StartCoroutine();
            }
            else
            {
                yield return InitPackageEditorMode().StartCoroutine();
            }
#else
            yield return InitPackageOfflineMode().StartCoroutine();
#endif

            yield return RequestPackageVersion().StartCoroutine();

            yield return UpdatePackageManifest().StartCoroutine();

            IsDone = true;
            Percent = 100;
        }

        private IEnumerator InitPackageEditorMode()
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
            var packageRoot = buildResult.PackageRootDirectory;
            var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            var initParameters = new EditorSimulateModeParameters();
            initParameters.EditorFileSystemParameters = editorFileSystemParams;
            var initOperation = m_Package.InitializeAsync(initParameters);
            yield return initOperation;

            if (initOperation.Status == EOperationStatus.Succeed)
                Debug.Log("资源包初始化成功！");
            else
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
        }

        private IEnumerator InitPackageOfflineMode()
        {
            var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            var initParameters = new OfflinePlayModeParameters();
            initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
            var initOperation = m_Package.InitializeAsync(initParameters);
            yield return initOperation;

            while (!initOperation.IsDone)
            {
                Percent = initOperation.Progress * 10;
                yield return null;
            }

            if (initOperation.Status == EOperationStatus.Succeed)
                Debug.Log("资源包初始化成功！");
            else
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
        }

        private IEnumerator RequestPackageVersion()
        {
            var operation = m_Package.RequestPackageVersionAsync();

            while (!operation.IsDone)
            {
                Percent = 10 + operation.Progress * 40;
                yield return null;
            }

            if (operation.Status == EOperationStatus.Succeed)
            {
                //更新成功
                m_PackageVersion = operation.PackageVersion;
                Debug.Log($"Request package Version : {m_PackageVersion}");
            }
            else
            {
                //更新失败
                Debug.LogError(operation.Error);
            }
        }

        private IEnumerator UpdatePackageManifest()
        {
            var operation = m_Package.UpdatePackageManifestAsync(m_PackageVersion);
            while (!operation.IsDone)
            {
                Percent = 50 + operation.Progress * 50;
                yield return null;
            }

            if (operation.Status == EOperationStatus.Succeed)
            {
                //更新成功
            }
            else
            {
                //更新失败
                Debug.LogError(operation.Error);
            }
        }

        // 加载资源
        public AssetHandle LoadAsset<T>(string path, Action<T> onLoaded) where T : UnityEngine.Object
        {
            string location = path.Replace('/', '@').ToLower();
            AssetHandle handle = m_Package.LoadAssetAsync<T>(location);
            handle.Completed += h => onLoaded?.Invoke(h.AssetObject as T);
            return handle;
        }

        public SceneHandle LoadScene(string sceneName, Action onLoaded)
        {
            string location = $"Scenes@{sceneName}".ToLower();
            var sceneMode = LoadSceneMode.Single;
            var physicsMode = LocalPhysicsMode.None;
            bool suspendLoad = false;
            SceneHandle handle = m_Package.LoadSceneAsync(location, sceneMode, physicsMode, suspendLoad);
            handle.Completed += h => onLoaded?.Invoke();
            return handle;
        }
    }
}