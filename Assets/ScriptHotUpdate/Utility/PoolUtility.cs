using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace UG20260527
{
    public interface IPoolUtility : IUtility
    {
        public void RegisterPrefabPool(GameObject prefab, Scene scene, int defaultCount = 0, int maxCount = 20);
        public GameObject Get(GameObject prefab, Scene scene, Action<GameObject> actionOnGet = null);
        public bool Recycle(GameObject obj, Action<GameObject> actionOnRecycle = null);
        public UniTask<bool> Recycle(GameObject obj, float delaySecond, Action<GameObject> actionOnRecycle = null);
        public void ClearAll();
        public void ClearScenePool(Scene scene);
    }

    /// <summary>
    /// 对象池系统
    /// </summary>
    public class PoolUtility : IPoolUtility
    {

        // 场景-对象池：Scene -> Prefab -> gameObject对象池
        private Dictionary<Scene, Dictionary<GameObject, IObjectPool<GameObject>>> _scenePool = new Dictionary<Scene, Dictionary<GameObject, IObjectPool<GameObject>>>();
        // 活跃中的对象池：活跃中的obj实例 -> 该obj所属的对象池
        private Dictionary<GameObject, IObjectPool<GameObject>> _activePool = new Dictionary<GameObject, IObjectPool<GameObject>>();

        // 预加载对象存储Root
        private string _preloadPoolName = "preloadPoolRoot";
        private Transform _preloadPoolRoot;

        public PoolUtility()
        {
            SceneManager.sceneUnloaded += scene =>
            {
                ClearScenePool(scene);
            };
        }


        // 初始化 预加载Root
        private void InitPreloadRoot()
        {
            if (_preloadPoolRoot == null)
            {
                var obj = GameObject.Find(_preloadPoolName);
                if (obj == null) obj = new GameObject(_preloadPoolName);
                _preloadPoolRoot = obj.transform;
            }
        }


        // 注册新的对象池
        public void RegisterPrefabPool(GameObject prefab, Scene scene, int defaultCount = 0, int maxCount = 20)
        {
            // 检查
            if (prefab == null) return;
            // 场景池是否存在
            if (_scenePool.ContainsKey(scene))
            {
                var prefabsPool = _scenePool[scene];
                // 场景-对象池是否存在
                if (prefabsPool.ContainsKey(prefab))
                {
                    Debug.Log($"{scene.name}-{prefab.name} 场景-对象池已存在，不可重复创建");
                    return;
                }
            }
            else
            {
                // 创建 场景池
                _scenePool.Add(scene, new Dictionary<GameObject, IObjectPool<GameObject>>());
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

            // 添加到 场景-对象池 字典管理
            _scenePool[scene].Add(prefab, pool);

            // 预加载初始对象
            if (defaultCount > 0)
            {
                GameObject tempObj = null;
                List<GameObject> tempList = new List<GameObject>();
                for (int i = 0; i < defaultCount; i++)
                {
                    tempObj = pool.Get();
                    tempObj.transform.SetParent(_preloadPoolRoot);
                    tempObj.transform.localPosition = Vector3.zero;
                    tempList.Add(tempObj);
                }
                foreach (var obj in tempList)
                {
                    pool.Release(obj);
                }
                tempList.Clear();
            }
        }

        // 获取对象
        public GameObject Get(GameObject prefab, Scene scene, Action<GameObject> actionOnGet)
        {
            if(prefab == null) return null;

            // 注册新的对象池
            if(!_scenePool.ContainsKey(scene) || !_scenePool[scene].ContainsKey(prefab))
            {
                RegisterPrefabPool(prefab, scene);
            }

            // Get
            IObjectPool<GameObject> pool = _scenePool[scene][prefab];
            var obj = pool.Get();
            if (obj.scene != scene) SceneManager.MoveGameObjectToScene(obj, scene);
            actionOnGet?.Invoke(obj);

            // 记录 该obj所属的对象池
            if(!_activePool.ContainsKey(obj)) _activePool.Add(obj, pool);
            else _activePool[obj] = pool;
            
            return obj;
        }

        // 回收对象
        public bool Recycle(GameObject obj, Action<GameObject> actionOnRecycle)
        {
            // 查找 所属的对象池
            if (obj == null) return false;
            if (!_activePool.ContainsKey(obj))
            {
                if (obj.activeSelf == true)
                {
                    Debug.Log($"{obj.name} 不受对象池管理，无法释放，直接销毁");
                    GameObject.Destroy(obj);
                    return false;
                }
                else
                {
                    // 重复回收
                    return false;
                }
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
        public async UniTask<bool> Recycle(GameObject obj, float delaySecond, Action<GameObject> actionOnRecycle)
        {
            if (obj == null) return false;
            if (delaySecond <= 0)
            {
                return Recycle(obj, actionOnRecycle);
            }
            else
            {
                await UniTask.WaitForSeconds(delaySecond);
                return Recycle(obj, actionOnRecycle);
            }
        }

        // 清空所有对象池
        public void ClearAll()
        {
            // 清空所有对象池
            foreach(var prefabsPool in _scenePool.Values)
            {
                foreach(var pool in prefabsPool.Values)
                {
                    pool.Clear();
                }
            }
            // 清空映射
            _scenePool.Clear();
            _activePool.Clear();
        }

        // 清空场景池
        public void ClearScenePool(Scene scene)
        {
            foreach(var pool in _scenePool[scene].Values)
            {
                pool.Clear();
            }
            // 移除 该场景池映射
            _scenePool.Remove(scene);
            // 移除 被销毁的对象引用
            List<GameObject> list = new List<GameObject>();
            foreach(var obj in _activePool.Keys)
            {
                if(obj == null) list.Add(obj);
            }
            foreach(var obj in list)
            {
                _activePool.Remove(obj);
            }
        }

    }
}
