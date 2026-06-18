using Cysharp.Threading.Tasks;
using QFramework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace UG20260527
{
    /// <summary>
    /// 加载场景控制器 载荷
    /// </summary>
    /// <typeparam name="T">SceneControllerBase子类</typeparam>
    public class LoadSceneControllerPayLoad<T> where T : SceneControllerBase
    {

        public enum LoadSceneControllerState
        {
            /// <summary>
            /// 成功，返回句柄和场景控制器
            /// </summary>
            Succeed,

            /// <summary>
            /// 重复创建，仅返回场景控制器（返回的句柄无效，仅做占位）
            /// </summary>
            Repeated,
        }

        /// <summary>
        /// 加载创建场景控制器 状态
        /// </summary>
        public LoadSceneControllerState state;

        /// <summary>
        /// 异步加载场景 句柄
        /// </summary>
        public AsyncOperationHandle<SceneInstance> handle;

        /// <summary>
        /// 场景控制器
        /// </summary>
        public T sceneController;

        public LoadSceneControllerPayLoad(AsyncOperationHandle<SceneInstance> handle, T sceneController, LoadSceneControllerState state = LoadSceneControllerState.Succeed)
        {
            this.handle = handle;
            this.sceneController = sceneController;
            this.state = state;
        }
    }

    public interface ISceneSystem : ISystem
    {
        public UniTask<T> EnterScenceAsync<T>(bool activeOnLoad = true) where T : SceneControllerBase, new();
        public UniTask<LoadSceneControllerPayLoad<T>> EnterScencePayLoadAsync<T>(bool activeOnLoad = true) where T : SceneControllerBase, new();
        public UniTask<bool> ExitScenceAsync<T>() where T : SceneControllerBase;
    }

    /// <summary>
    /// 场景系统（管理 场景控制器）
    /// </summary>
    public class SceneSystem : AbstractSystem, ISceneSystem
    {

        // Scene 配置表
        private string sceneConfigPath = "Assets/AddressablesAsset/Config/SceneConfig.asset";
        private SceneConfig sceneConfig;


        /// <summary>
        /// 场景控制器 字典（场景控制器名 -> 场景控制器实例）
        /// </summary>
        private Dictionary<string, SceneControllerBase> _sceneControllerDict = new Dictionary<string, SceneControllerBase>();


        protected override void OnInit()
        {

        }


        // 加载 Scene配置表
        private async UniTask LoadSceneConfig()
        {
            // 加载 Panel配置表
            object obj = await this.GetSystem<IResourceSystem>().LoadAssetsAsync<object>(sceneConfigPath);
            sceneConfig = obj as SceneConfig;
            sceneConfig.InitConfig();
        }


        // 增删查改


        /* -------------------------------------------------- 接口函数 -------------------------------------------------- */

        /// <summary>
        /// 加载并进入场景，根据场景管理器类型
        /// </summary>
        /// <typeparam name="T">SceneController类型</typeparam>
        /// <param name="activeOnLoad">是否直接激活场景</param>
        /// <returns></returns>
        async UniTask<T> ISceneSystem.EnterScenceAsync<T>(bool activeOnLoad)
        {
            // Scene配置
            if(sceneConfig == null) await LoadSceneConfig();
            if(!sceneConfig.sceneConfigDict.ContainsKey(typeof(T).Name))
            {
                Debug.LogWarning($"{sceneConfig.name} 配置表中没有 {typeof(T).Name} 资源");
                return null;
            }

            // 检查是否重复创建
            if(_sceneControllerDict.ContainsKey(typeof(T).Name))
            {
                Debug.LogWarning($"{typeof(T).Name} 场景资源和控制器 已被加载并创建，不能重复创建");
                return _sceneControllerDict[typeof(T).Name] as T;
            }

            // 创建 场景管理器
            T sceneController = new T();
            _sceneControllerDict.Add(typeof(T).Name, sceneController);
            await sceneController.OnInit();
            await sceneController.OnPreEnter();

            // 加载并进入场景
            var sceneInstance = await this.GetSystem<IResourceSystem>().LoadScenceAsync(
                sceneConfig.sceneConfigDict[typeof(T).Name].sceneAssetPath, 
                LoadSceneMode.Additive, 
                activeOnLoad);
            await sceneController.OnEnter(sceneInstance);


            return sceneController;
        }

        /// <summary>
        /// 加载并进入场景，返回 加载场景控制器载荷
        /// </summary>
        /// <typeparam name="T">SceneController类型</typeparam>
        /// <param name="activeOnLoad">是否直接激活场景</param>
        /// <returns>加载场景控制器 载荷</returns>
        async UniTask<LoadSceneControllerPayLoad<T>> ISceneSystem.EnterScencePayLoadAsync<T>(bool activeOnLoad)
        {
            // Scene配置
            if (sceneConfig == null) await LoadSceneConfig();
            if (!sceneConfig.sceneConfigDict.ContainsKey(typeof(T).Name))
            {
                Debug.LogWarning($"{sceneConfig.name} 配置表中没有 {typeof(T).Name} 资源");
                return null;
            }

            // 检查是否重复创建
            if (_sceneControllerDict.ContainsKey(typeof(T).Name))
            {
                Debug.LogWarning($"{typeof(T).Name} 场景资源和控制器 已被加载并创建，不能重复创建");
                return new LoadSceneControllerPayLoad<T>(
                    new AsyncOperationHandle<SceneInstance>(), 
                    _sceneControllerDict[typeof(T).Name] as T,
                    LoadSceneControllerPayLoad<T>.LoadSceneControllerState.Repeated);
            }

            // 创建 场景管理器
            T sceneController = new T();
            _sceneControllerDict.Add(typeof(T).Name, sceneController);
            await sceneController.OnInit();
            await sceneController.OnPreEnter();

            var handle = this.GetSystem<IResourceSystem>().LoadScenceHandleAsync(
                sceneConfig.sceneConfigDict[typeof(T).Name].sceneAssetPath, 
                LoadSceneMode.Additive, 
                activeOnLoad);
            handle.Completed += async h =>
            {
                await sceneController.OnEnter(h.Result);
            };

            return new LoadSceneControllerPayLoad<T>(handle, sceneController);

        }

        /// <summary>
        /// 退出场景
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        async UniTask<bool> ISceneSystem.ExitScenceAsync<T>()
        {
            // 检查是否受 场景系统控制
            if(!_sceneControllerDict.ContainsKey(typeof(T).Name))
            {
                Debug.LogWarning($"{typeof(T).Name} 场景资源和控制器 不受场景系统管理，无法卸载");
                return false;
            }

            // 移除 字典
            SceneControllerBase sceneController = _sceneControllerDict[typeof(T).Name];
            _sceneControllerDict.Remove(typeof(T).Name);

            // 调用 生命周期函数，并卸载
            sceneController.OnPreExit();
            await this.GetSystem<IResourceSystem>().UnLoadScenceAsync(sceneController.sceneInstance);
            sceneController.OnExit();

            return true;
        }

    }



    /// <summary>
    /// 场景控制器基类（关卡蓝图基类）
    /// </summary>
    public abstract class SceneControllerBase : IController
    {
        /// <summary>
        /// 场景实例
        /// </summary>
        public SceneInstance sceneInstance { get; private set; }


        /// <summary>
        /// 自身初始化
        /// </summary>
        public virtual UniTask OnInit() 
        {
            // 异步初始化

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 准备 进入场景（预加载相关资源 到 内存中）
        /// </summary>
        public virtual UniTask OnPreEnter() 
        {
            // 异步加载资源

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 已经 进入场景（打开HUD，生成玩家，等）
        /// </summary>
        public virtual UniTask OnEnter(SceneInstance sceneInstance)
        {
            // 异步加载并实例化场景中的对象
            this.sceneInstance = sceneInstance;

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 准备 退出场景（序列化局内玩家数据，等）
        /// </summary>
        public virtual void OnPreExit() { }

        /// <summary>
        /// 已经 退出场景（卸载内存资源）
        /// </summary>
        public virtual void OnExit() { }



        public IArchitecture GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}