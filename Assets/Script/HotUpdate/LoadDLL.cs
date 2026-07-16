using Cysharp.Threading.Tasks;
using HybridCLR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UG20260527
{
    /// <summary>
    /// 加载 热更新程序集
    /// </summary>
    public class LoadDLL : MonoBehaviour
    {
        //public enum HotUpdateState
        //{
        //    /// <summary>
        //    /// 更新完成
        //    /// </summary>
        //    UpdateCompleted,

        //    /// <summary>
        //    /// 正在下载更新中
        //    /// </summary>
        //    Updating
        //}

        ///// <summary>
        ///// 资源热更新状态
        ///// </summary>
        //private HotUpdateState _hotUpdateState = HotUpdateState.UpdateCompleted;


        private bool _canOpenScene = false;

        async void Start()
        {
            Debug.Log("进入热更新场景");

            // 1. 初始化 Addressables
            await Addressables.InitializeAsync();

            // 2. 检查 远程Catalog更新
            var catalogHandle =  Addressables.CheckForCatalogUpdates(false);
            await catalogHandle.Task;
            if(catalogHandle.Status == AsyncOperationStatus.Succeeded)
            {
                List<string> catalogs = catalogHandle.Result;
                Debug.Log($"检查 更新目录 catalogs.Count = {catalogs.Count}");
                if (catalogs != null && catalogs.Count > 0)
                {
                    // 3. 手动更新 本地Catalog
                    var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
                    await updateHandle.Task;
                    List<string> _updateKeys = new List<string>();
                    foreach (var locator in updateHandle.Result)
                    {
                        // 记录所有热更新资源的key
                        foreach(var key in locator.Keys)
                        {
                            if (key is string strKey) _updateKeys.Add(strKey);
                        }
                        Debug.Log($"热更新资源key数 = {_updateKeys.Count}");
                    }
                    Addressables.Release(updateHandle);

                    // 4. 获取 更新包体总和size
                    var sizeHandle = Addressables.GetDownloadSizeAsync(_updateKeys);
                    await sizeHandle.Task;
                    Debug.Log($"获取 热更新包体Size = {sizeHandle.Result}");
                    if(sizeHandle.Result > 0)
                    {
                        // 自动重试（没法验证是否起作用）
                        int maxRetries = 3;
                        int retryCount = 0;
                        while (retryCount < maxRetries)
                        {
                            // 5. 下载 热更新资源
                            Debug.Log("下载 热更新资源 开始");
                            var downloadHandle = Addressables.DownloadDependenciesAsync(_updateKeys, Addressables.MergeMode.Union);

                            DownloadStatus downloadState;
                            while (!downloadHandle.IsDone)
                            {
                                // 获取 下载状态
                                downloadState = downloadHandle.GetDownloadStatus();
                                // 已经下载的资源字节数
                                long downloadBytes = downloadState.DownloadedBytes;
                                // 总的资源字节数
                                long totalBytes = downloadState.TotalBytes;
                                Debug.Log($"  下载资源数/总资源数 = {downloadBytes} / {totalBytes}");

                                // 下载百分比
                                float percent = downloadState.Percent;
                                //Debug.Log($"  下载百分比 = {percent * 100}%");

                                await UniTask.Yield();
                            }
                            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
                            {
                                Debug.Log("下载 热更新资源包 成功");
                                break;
                            }
                            else
                            {
                                Debug.LogWarning("下载 热更新资源包 失败，2秒后重试");
                                retryCount++;
                                await UniTask.WaitForSeconds(2);
                            }
                            Addressables.Release(downloadHandle);
                        }
                        if (retryCount >= maxRetries) Debug.LogWarning("热更新失败，请检查网络");
                    }
                    Addressables.Release(sizeHandle);
                }
            }
            else
            {
                Debug.LogWarning("网络未连接，无法检查更新");
            }
            Addressables.Release(catalogHandle);

            // 补充AOT
            LoadAOT();

            // 加载程序集
            TextAsset assemblyBytes = await Addressables.LoadAssetAsync<TextAsset>("Assets/Res_HotUpdate/Assembly/StandaloneWindows64/HotUpdate.dll.bytes");
            Assembly.Load(assemblyBytes.bytes);

            Debug.Log("按下 Space键 进入游戏");
            _canOpenScene = true;
        }

        private void Update()
        {
            if(_canOpenScene)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    OpenScene();
                }
            }
        }

        // 进入场景
        private async void OpenScene()
        {
            // 进入新场景
            var sceneHandle = Addressables.LoadSceneAsync("Assets/Res_HotUpdate/Scenes/InitScene.unity", UnityEngine.SceneManagement.LoadSceneMode.Single, false);
            await sceneHandle.Task;
            if (sceneHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"3秒后进入场景：{sceneHandle.Result.Scene.name}");
                await UniTask.WaitForSeconds(3f);
                await sceneHandle.Result.ActivateAsync();
            }
            else if (sceneHandle.Status == AsyncOperationStatus.Failed)
            {
                Debug.Log($"加载场景：{sceneHandle.Result.Scene.name} 失败！！！");
            }
        }

        /// <summary>
        /// 补充AOT
        /// </summary>
        private void LoadAOT()
        {
            // 补充AOT元数据
            List<string> aotDllList = new List<string>
            {
                "System.Core.dll",
                "System.dll",
                "UniTask.dll",
                "Unity.Addressables.dll",
                "Unity.InputSystem.dll",
                "Unity.ResourceManager.dll",
                "UnityEngine.CoreModule.dll",
                "mscorlib.dll",
            };
            foreach (string aotDll in aotDllList)
            {
                Byte[] aotDllBytes = File.ReadAllBytes($"{Application.streamingAssetsPath}/MetaDataDlls/{aotDll}.bytes");
                RuntimeApi.LoadMetadataForAOTAssembly(aotDllBytes, HomologousImageMode.SuperSet);
            }
        }

    }
}