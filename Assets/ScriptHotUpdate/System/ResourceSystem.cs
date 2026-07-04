using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace UG20260527
{
    public interface IResourceSystem : ISystem
    {
        /// <summary>
        /// 加载资源，直接返回句柄
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public AsyncOperationHandle<TObject> LoadAssetHandleAsync<TObject>(string path);
        /// <summary>
        /// 加载资源预制体（加载到内存中）
        /// </summary>
        /// <typeparam name="TObject">GameObject预制体</typeparam>
        /// <param name="path">资源路径（Addressables的地址名）</param>
        /// <returns></returns>
        public UniTask<TObject> LoadAssetAsync<TObject>(string path);
        /// <summary>
        /// 实例化资源
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父对象</param>
        /// <param name="worldPositionStays">世界位置保持不变</param>
        /// <param name="actionOnGet">拿到对象时的逻辑注入</param>
        /// <returns></returns>
        public GameObject Instantiate(GameObject prefab, Scene scene, Transform parent = null, bool worldPositionStays = true, Action<GameObject> actionOnGet = null);
        /// <summary>
        /// 加载并实例化资源
        /// </summary>
        /// <typeparam name="TObject">GameObject类</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="parent">父对象</param>
        /// <param name="worldPositionStays">世界位置保持不变</param>
        /// <param name="actionOnGet">拿到对象时的逻辑注入</param>
        /// <returns></returns>
        public UniTask<GameObject> LoadAndInstantiateAsync<TObject>(string path, Scene scene, Transform parent = null, bool worldPositionStays = true, Action<GameObject> actionOnGet = null);
        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="obj">回收对象</param>
        /// <param name="actionOnRecycle">回收时的逻辑注入</param>
        /// <returns>不受对象池管理，直接销毁，返回false</returns>
        public bool Recycle(GameObject obj, Action<GameObject> actionOnRecycle = null);
        /// <summary>
        /// 回收资源Delay
        /// </summary>
        /// <param name="obj">回收对象</param>
        /// <param name="delaySecond">等待时长，秒</param>
        /// <param name="actionOnRecycle">回收时的逻辑注入</param>
        /// <returns>不受对象池管理，直接销毁，返回false</returns>
        public UniTask<bool> Recycle(GameObject obj, float delaySecond, Action<GameObject> actionOnRecycle = null);


        // 预制体路径
        /// <summary>
        /// 获取 制定文件夹目录 中 所有预制体的路径
        /// </summary>
        /// <param name="FileDirectory">文件夹目录，以"/"结尾</param>
        /// <returns></returns>
        public List<string> GetPrefabPathInFile(string FileDirectory)
        {
            return new List<string>();
        }

        // 场景相关
        public AsyncOperationHandle<SceneInstance> LoadScenceHandleAsync(string path, LoadSceneMode loadSceneMode, bool activeOnLoad);
        public UniTask<SceneInstance> LoadScenceAsync(string path, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool activeOnLoad = true);
        public void LoadScenceAsync(string path, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool activeOnLoad = true, Action<SceneInstance> callBack = null);
        public UniTask UnLoadScenceAsync(SceneInstance instance);
        public void UnLoadScenceAsync(SceneInstance instance, Action callBack = null);
    }
    public class ResourceSystem : AbstractSystem, IResourceSystem
    {
        protected override void OnInit() { }

        /* -------------------------------------------------- 预制体资源 -------------------------------------------------- */

        /// <summary>
        /// 加载资源，直接返回句柄
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        AsyncOperationHandle<TObject> IResourceSystem.LoadAssetHandleAsync<TObject>(string path)
        {
            return Addressables.LoadAssetAsync<TObject>(path);
        }


        /// <summary>
        /// 加载资源预制体（加载到内存中）
        /// </summary>
        /// <typeparam name="TObject">GameObject预制体</typeparam>
        /// <param name="path">资源路径（Addressables的地址名）</param>
        /// <returns></returns>
        async UniTask<TObject> IResourceSystem.LoadAssetAsync<TObject>(string path)
        {
            return await Addressables.LoadAssetAsync<TObject>(path).Task;
        }

        /// <summary>
        /// 实例化资源
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父对象</param>
        /// <param name="worldPositionStays">世界位置保持不变</param>
        /// <param name="actionOnGet">拿到对象时的逻辑注入</param>
        /// <returns></returns>
        GameObject IResourceSystem.Instantiate(GameObject prefab, Scene scene, Transform parent, bool worldPositionStays, Action<GameObject> actionOnGet)
        {
            if (parent) scene = parent.gameObject.scene;
            GameObject obj = this.GetUtility<IPoolUtility>().Get(prefab, scene, actionOnGet);
            obj.transform.SetParent(parent, worldPositionStays);
            return obj;
        }

        /// <summary>
        /// 加载并实例化资源
        /// </summary>
        /// <typeparam name="TObject">GameObject类</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="parent">父对象</param>
        /// <param name="worldPositionStays">世界位置保持不变</param>
        /// <param name="actionOnGet">拿到对象时的逻辑注入</param>
        /// <returns></returns>
        async UniTask<GameObject> IResourceSystem.LoadAndInstantiateAsync<TObject>(string path, Scene scene, Transform parent, bool worldPositionStays, Action<GameObject> actionOnGet)
        {
            if (parent) scene = parent.gameObject.scene;
            var prefab = await Addressables.LoadAssetAsync<TObject>(path).Task as GameObject;
            GameObject obj = this.GetUtility<IPoolUtility>().Get(prefab, scene, actionOnGet);
            obj.transform.SetParent(parent, worldPositionStays);
            return obj;
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="obj">回收对象</param>
        /// <param name="actionOnRecycle">回收时的逻辑注入</param>
        /// <returns>不受对象池管理，直接销毁，返回false</returns>
        bool IResourceSystem.Recycle(GameObject obj, Action<GameObject> actionOnRecycle)
        {
            return this.GetUtility<IPoolUtility>().Recycle(obj, actionOnRecycle);
        }

        /// <summary>
        /// 回收资源Delay
        /// </summary>
        /// <param name="obj">回收对象</param>
        /// <param name="delaySecond">等待时长，秒</param>
        /// <param name="actionOnRecycle">回收时的逻辑注入</param>
        /// <returns>不受对象池管理，直接销毁，返回false</returns>
        UniTask<bool> IResourceSystem.Recycle(GameObject obj, float delaySecond, Action<GameObject> actionOnRecycle)
        {
            return this.GetUtility<IPoolUtility>().Recycle(obj, delaySecond, actionOnRecycle);
        }


        /* -------------------------------------------------- 场景资源 -------------------------------------------------- */

        // 加载场景（直接返回句柄，用于实现加载进度条）
        AsyncOperationHandle<SceneInstance> IResourceSystem.LoadScenceHandleAsync(string path, LoadSceneMode loadSceneMode, bool activeOnLoad)
        {
            return Addressables.LoadSceneAsync(path, loadSceneMode, activeOnLoad);
        }

        // 加载场景
        async UniTask<SceneInstance> IResourceSystem.LoadScenceAsync(string path, LoadSceneMode loadSceneMode, bool activeOnLoad)
        {
            var handle = Addressables.LoadSceneAsync(path, loadSceneMode, activeOnLoad);
            await handle.Task;
            return handle.Result;
        }

        // 加载场景
        void IResourceSystem.LoadScenceAsync(string path, LoadSceneMode loadSceneMode, bool activeOnLoad, Action<SceneInstance> callBack)
        {
            var handle = Addressables.LoadSceneAsync(path, loadSceneMode, activeOnLoad);
            handle.Completed += (op) =>
            {
                callBack?.Invoke(op.Result);
            };
        }

        // 卸载场景
        async UniTask IResourceSystem.UnLoadScenceAsync(SceneInstance instance)
        {
            await Addressables.UnloadSceneAsync(instance).Task;
        }

        // 卸载场景
        void IResourceSystem.UnLoadScenceAsync(SceneInstance instance, Action callBack)
        {
            var handle = Addressables.UnloadSceneAsync(instance);
            handle.Completed += (op) =>
            {
                callBack?.Invoke();
            };
        }
    }
}