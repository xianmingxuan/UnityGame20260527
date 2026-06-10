using Cysharp.Threading.Tasks;
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
using static UG20260527.LoadDLL;

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


        async void Start()
        {
            Debug.Log("进入加载场景");

#if UNITY_EDITOR
            // 编辑器状态下，无需下载，直接获取 热更新程序集 即可
            Assembly hotUpdateAss = null;
            hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First( a => a.GetName().Name == "HotUpdate" );
            // 反射 获取程序集中 Hello脚本，运行 静态函数Run
            if (hotUpdateAss == null)
            {
                Debug.Log("加载 HotUpdate程序集 为空");
                return;
            }
            Type type = hotUpdateAss.GetType("Hello");
            type.GetMethod("Run").Invoke(null, null);
            await UniTask.CompletedTask;
#else
            // 非编辑器下（游戏打包运行时），需要到 流送资源目录 加载 热更新程序集
            // 热更新
            var sizeHandle = Addressables.GetDownloadSizeAsync("default");
            await sizeHandle.Task;
            if (sizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                if (sizeHandle.Result > 0)
                {
                    Debug.Log("需要 热更新");
                    Debug.Log($"size = {sizeHandle.Result}");
                    await Download();
                }
                else
                {
                    Debug.Log("不需要 热更新");
                }
            }

            // 获取程序集
            Assembly hotUpdateAss = null;
            hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First( a => a.GetName().Name == "HotUpdate" );
            // 反射 获取程序集中 Hello脚本，运行 静态函数Run
            if (hotUpdateAss == null)
            {
                Debug.Log("加载 HotUpdate程序集 为空");
                return;
            }
            Type type = hotUpdateAss.GetType("Hello");
            type.GetMethod("Run").Invoke(null, null);
#endif

        }




        /// <summary>
        /// 下载 热更新包
        /// </summary>
        public async UniTask Download()
        {
            // 根据 目录列表 获取更新包的大小
            Debug.Log("检查 热更新资源包 大小");
            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync("default");

            Debug.Log("下载 热更新资源包 开始");
            DownloadStatus downloadState;
            while (!downloadHandle.IsDone)
            {
                // 获取 下载状态
                downloadState = downloadHandle.GetDownloadStatus();
                // 已经下载的资源字节数
                long downloadBytes = downloadState.DownloadedBytes;
                // 总的资源字节数
                long totalBytes = downloadState.TotalBytes;
                Debug.Log($"    下载资源数/总资源数 = {downloadBytes} / {totalBytes}");

                // 下载百分比
                float percent = downloadState.Percent;
                Debug.Log($"    下载百分比 = {percent * 100}%");

                await UniTask.Yield();
            }
            if(downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("下载 热更新资源包 成功");
                // 加载程序集
                TextAsset assemblyBytes = await Addressables.LoadAssetAsync<TextAsset>("Assets/AddressablesAsset/Assembly/StandaloneWindows64/HotUpdate.dll.bytes");
                Assembly.Load(assemblyBytes.bytes);
            }
            else
            {
                Debug.Log("下载 热更新资源包 失败");
            }
        }

    }
}