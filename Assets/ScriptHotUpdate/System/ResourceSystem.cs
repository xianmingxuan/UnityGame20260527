using Cysharp.Threading.Tasks;
using QFramework;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UG20260527
{
    public interface IResourceSystem : ISystem
    {
        public UniTask<TObject> LoadAssetsAsync<TObject>(string path);
        public GameObject Instantiate(GameObject prefab, Transform parent, bool worldPositionStays, Action<GameObject> actionOnGet = null);
        public UniTask<GameObject> LoadAndInstantiateAsync<TObject>(string path, Transform parent = null, bool worldPositionStays = true, Action<GameObject> actionOnGet = null);
        public bool Recycle(GameObject obj, Action<GameObject> actionOnRecycle = null);
        public UniTask<bool> Recycle(GameObject obj, float delaySecond, Action<GameObject> actionOnRecycle = null);

    }
    public class ResourceSystem : AbstractSystem, IResourceSystem
    {
        /// <summary>
        /// 对象池系统
        /// </summary>
        private IPoolSystem poolSystem;


        protected override void OnInit()
        {
            if(poolSystem == null)
            {
                poolSystem = this.GetSystem<IPoolSystem>();
            }
        }


        // 加载资源
        async UniTask<TObject> IResourceSystem.LoadAssetsAsync<TObject>(string path)
        {
            return await Addressables.LoadAssetAsync<TObject>(path).Task;
        }

        // 实例化资源
        GameObject IResourceSystem.Instantiate(GameObject prefab, Transform parent, bool worldPositionStays, Action<GameObject> actionOnGet)
        {
            GameObject obj = poolSystem.Get(prefab, actionOnGet);
            obj.transform.SetParent(parent, worldPositionStays);
            return obj;
        }

        // 加载并实例化资源
        async UniTask<GameObject> IResourceSystem.LoadAndInstantiateAsync<TObject>(string path, Transform parent, bool worldPositionStays, Action<GameObject> actionOnGet)
        {
            var prefab = await Addressables.LoadAssetAsync<TObject>(path).Task as GameObject;
            GameObject obj = poolSystem.Get(prefab, actionOnGet);
            obj.transform.SetParent(parent, worldPositionStays);
            return obj;
        }

        // 回收资源
        bool IResourceSystem.Recycle(GameObject obj, Action<GameObject> actionOnRecycle)
        {
            return poolSystem.Recycle(obj, actionOnRecycle);
        }

        // 回收资源Delay
        UniTask<bool> IResourceSystem.Recycle(GameObject obj, float delaySecond, Action<GameObject> actionOnRecycle)
        {
            return poolSystem.Recycle(obj, delaySecond, actionOnRecycle);
        }

    }
}