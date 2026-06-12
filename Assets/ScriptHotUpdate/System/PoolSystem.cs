using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

namespace UG20260527
{
    public interface IPoolSystem : ISystem
    {
        public void RegisterPrefabPool(GameObject prefab, int defaultCount = 0, int maxCount = 20);
        public GameObject Get(GameObject prefab, Action<GameObject> actionOnGet = null);
        public bool Recycle(GameObject obj, Action<GameObject> actionOnRecycle = null);
        public UniTask<bool> Recycle(GameObject obj, float delaySecond, Action<GameObject> actionOnRecycle = null);
        public void ClearAll();
    }

    /// <summary>
    /// 对象池系统
    /// </summary>
    public class PoolSystem : AbstractSystem, IPoolSystem
    {
        // 对象池： Prefab -> gameObject对象池
        private Dictionary<GameObject, IObjectPool<GameObject>> _prefabsPool = new Dictionary<GameObject, IObjectPool<GameObject>>();
        // 活跃中的对象池：活跃中的obj实例 -> 该obj所属的对象池
        private Dictionary<GameObject, IObjectPool<GameObject>> _activePool = new Dictionary<GameObject, IObjectPool<GameObject>>();

        // 预加载对象存储Root
        private string _preloadRootName = "preloadRoot";
        private Transform _preloadRoot;

        protected override void OnInit()
        {
            InitPreloadRoot();
        }


        // 初始化 预加载Root
        private void InitPreloadRoot()
        {
            if (_preloadRoot == null)
            {
                var obj = GameObject.Find(_preloadRootName);
                if (obj == null) obj = new GameObject(_preloadRootName);
                _preloadRoot = obj.transform;
            }
        }


        // 注册新的对象池
        void IPoolSystem.RegisterPrefabPool(GameObject prefab, int defaultCount, int maxCount)
        {
            _RegisterPrefabPool(prefab, defaultCount, maxCount); 
        }

        private void _RegisterPrefabPool(GameObject prefab, int defaultCount = 0, int maxCount = 20)
        {
            // 检查
            if (prefab == null) return;
            if (_prefabsPool.ContainsKey(prefab))
            {
                Debug.Log($"{prefab.name} 对象池已存在，不可重复创建");
                return;
            }

            // 初始化 预加载Root
            InitPreloadRoot();

            // 创建一个对象池
            IObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                createFunc: () => 
                {
                    GameObject obj = GameObject.Instantiate(prefab);
                    obj.SetActive(false);
                    return obj;
                },
                actionOnGet: obj =>
                {
                    obj.SetActive(true);
                },
                actionOnRelease: obj =>
                {
                    obj.SetActive(false);
                },
                actionOnDestroy: obj =>
                {
                    GameObject.Destroy(obj);
                },
                collectionCheck: true,
                defaultCapacity: defaultCount,
                maxSize: maxCount
            );

            // 添加到对象池字典管理
            _prefabsPool.Add(prefab, pool);

            // 预加载初始对象
            if(defaultCount > 0)
            {
                GameObject tempObj = null;
                List<GameObject> tempList = new List<GameObject>();
                for(int i = 0; i < defaultCount; i++)
                {
                    tempObj = pool.Get();
                    tempObj.transform.SetParent(_preloadRoot);
                    tempObj.transform.localPosition = Vector3.zero;
                    tempList.Add(tempObj);
                }
                foreach(var obj in tempList)
                {
                    pool.Release(obj);
                }
                tempList.Clear();
            }
        }


        // 获取对象
        GameObject IPoolSystem.Get(GameObject prefab, Action<GameObject> actionOnGet)
        {
            if(prefab == null) return null;

            // 注册新的对象池
            if(!_prefabsPool.ContainsKey(prefab))
            {
                _RegisterPrefabPool(prefab);
            }

            // Get
            IObjectPool<GameObject> pool = _prefabsPool[prefab];
            var obj = pool.Get();
            actionOnGet?.Invoke(obj);

            // 记录 该obj所属的对象池
            if(!_activePool.ContainsKey(obj)) _activePool.Add(obj, pool);
            else _activePool[obj] = pool;
            
            return obj;
        }

        // 回收对象
        bool IPoolSystem.Recycle(GameObject obj, Action<GameObject> actionOnRecycle)
        {
            return _Recycle(obj, actionOnRecycle);
        }

        private bool _Recycle(GameObject obj, Action<GameObject> actionOnRecycle = null)
        {
            // 查找 所属的对象池
            if(obj == null) return false;
            if(!_activePool.ContainsKey(obj))
            {
                Debug.Log($"{obj.name} 不受对象池管理，无法释放，直接销毁");
                GameObject.Destroy(obj);
                return false;
            }

            // 回收
            var pool = _activePool[obj];
            actionOnRecycle?.Invoke(obj);
            pool.Release(obj);

            // 移除 活跃中对象池
            _activePool.Remove(obj);

            return true;
        }

        // 回收对象Delay
        async UniTask<bool> IPoolSystem.Recycle(GameObject obj, float delaySecond, Action<GameObject> actionOnRecycle)
        {
            if (obj == null) return false;
            if (delaySecond <= 0)
            {
                return _Recycle(obj, actionOnRecycle);
            }
            else
            {
                await UniTask.WaitForSeconds(delaySecond);
                return _Recycle(obj, actionOnRecycle);
            }
        }

        // 清空所有对象池
        void IPoolSystem.ClearAll()
        {
            // 清空所有对象池
            foreach(var pool in _prefabsPool.Values)
            {
                pool.Clear();
            }
            // 清空映射
            _prefabsPool.Clear();
            _activePool.Clear();
        }

    }
}
