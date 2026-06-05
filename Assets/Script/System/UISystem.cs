using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UG20260527
{

    /* ---------------------------------------------------------------- UI系统 -------------------------------------------------------- */

    public interface IUISystem : ISystem
    {
        public UniTask<T> OpenSinglePanel<T>(Action<T> onInit = null, bool isPushStack = true) where T : PanelBase;
        public UniTask<PanelBase> OpenSinglePanel(Type type, Action<PanelBase> onInit, bool isPushStack);
        public UniTask CloseSinglePanel(PanelBase panelSC = null);
    }

    public class UISystem : AbstractSystem, IUISystem
    {
        /* -------------------------------------------------- 属性 -------------------------------------------------- */

        // Panel 配置表
        private string uiConfigPath = "Assets/Config/UIConfig.asset";
        public UIConfig uiConfig;

        // UI栈
        public Stack<PanelBase> panelStack;
        // 正在加载 准备入栈的Panel数量
        public BindableProperty<int> pushingPanelCount { get; } = new BindableProperty<int>(0);

        // 画布面板
        public Transform parentCanvas;


        protected override void OnInit()
        {
            panelStack = new Stack<PanelBase>();
            // 获取 画布面板
            var obj = GameObject.Find("Canvas");
            if (obj == null) Debug.LogWarning("场景中没有 Canvas画布");
            else parentCanvas = obj.transform;

            // 加载中Panel数量
            pushingPanelCount.Register(value =>
            {
                if (value < 0) pushingPanelCount.Value = 0;
            });
        }

        /* -------------------------------------------------- API函数 -------------------------------------------------- */

        // 加载 UI配置表
        private async UniTask LoadUIConfig()
        {
            // 加载 Panel配置表
            uiConfig = await Addressables.LoadAssetAsync<UIConfig>(uiConfigPath).Task;
        }

        // 根据Type 获取Panel预制体AssetRef
        private async UniTask<AssetReference> GetPanelAssetRef<T>() where T : PanelBase
        {
            if (uiConfig == null)
            {
                await LoadUIConfig();
            }

            if (!uiConfig.panelConfigDic.ContainsKey(typeof(T)))
            {
                Debug.LogWarning($"{uiConfig.name} 配置表中没有 {typeof(T).Name} 资源");
                return null;
            }

            return uiConfig.panelConfigDic[typeof(T)].panelAssetRef;
        }

        private async UniTask<AssetReference> GetPanelAssetRef(Type type)
        {
            if (uiConfig == null)
            {
                await LoadUIConfig();
            }

            if (!uiConfig.panelConfigDic.ContainsKey(type))
            {
                Debug.LogWarning($"{uiConfig.name} 配置表中没有 {type.Name} 资源");
                return null;
            }

            return uiConfig.panelConfigDic[type].panelAssetRef;
        }

        // 根据Type 实例化Panel和脚本
        private async UniTask<T> CreatePanel<T>(Action<T> onInit) where T : PanelBase
        {
            // 获取Panel资源
            AssetReference panelAssetRef = await GetPanelAssetRef<T>();
            if(panelAssetRef == null) return null;

            // 实例化Panel
            var panel = await panelAssetRef.InstantiateAsync(parentCanvas, false).Task;

            // 添加 控制脚本
            T panelScript = panel.GetComponent<T>();
            if (panelScript == null) panelScript = panel.AddComponent<T>();
            await panelScript.OnInit(onInit);

            return panelScript;
        }

        private async UniTask<PanelBase> CreatePanel(Type type, Action<PanelBase> onInit)
        {
            // 获取Panel资源
            AssetReference panelAssetRef = await GetPanelAssetRef(type);
            if (panelAssetRef == null) return null;

            // 实例化Panel
            var panel = await panelAssetRef.InstantiateAsync(parentCanvas, false).Task;

            // 添加 控制脚本
            PanelBase panelScript = panel.GetComponent(type) as PanelBase;
            if (panelScript == null) panelScript = panel.AddComponent(type) as PanelBase;
            await panelScript.OnInit(onInit);

            return panelScript;
        }


        /* -------------------------------------------------- 接口函数 -------------------------------------------------- */

        async UniTask<T> IUISystem.OpenSinglePanel<T>(Action<T> onInit, bool isPushStack)
        {
            // 不入栈（由创建者自己管理，例如：子面板）
            if(!isPushStack)
            {
                T panelSC = await CreatePanel<T>(onInit);
                panelSC.OnOpen();
                return panelSC;
            }

            // 加载中Panel++
            pushingPanelCount.Value++;

            // 创建Panel
            T panelScript = await CreatePanel<T>(onInit);
            if (panelScript == null)
            {
                // 加载中Panel--
                pushingPanelCount.Value--;
                return null;
            }

            // 冻结栈顶
            if (panelStack.Count > 0) panelStack.Peek().OnPause();

            // 入栈 并 启用
            panelStack.Push(panelScript);
            panelScript.OnOpen();

            // 加载中Panel--
            pushingPanelCount.Value--;
            return panelScript;
        }

        async UniTask<PanelBase> IUISystem.OpenSinglePanel(Type type, Action<PanelBase> onInit, bool isPushStack)
        {
            // 不入栈（由创建者自己管理，例如：子面板）
            if (!isPushStack)
            {
                PanelBase panelSC = await CreatePanel(type, onInit);
                panelSC.OnOpen();
                return panelSC;
            }

            // 加载中Panel++
            pushingPanelCount.Value++;

            // 创建Panel
            PanelBase panelScript = await CreatePanel(type, onInit);
            if (panelScript == null)
            {
                // 加载中Panel--
                pushingPanelCount.Value--;
                return null;
            }

            // 冻结栈顶
            if (panelStack.Count > 0) panelStack.Peek().OnPause();

            // 入栈 并 启用
            panelStack.Push(panelScript);
            panelScript.OnOpen();

            // 加载中Panel--
            pushingPanelCount.Value--;
            return panelScript;
        }

        async UniTask IUISystem.CloseSinglePanel(PanelBase panelSC)
        {
            if(panelSC != null)
            {
                panelSC.OnClose();
                return;
            }

            // 先等其它 push任务 完成
            if(pushingPanelCount.Value > 0) await UniTask.WaitUntil(() => pushingPanelCount.Value <= 0);
            if (panelStack.Count <= 0) return;
            // 弹出
            panelStack.Peek().OnClose();
            panelStack.Pop();

            if (panelStack.Count > 0) panelStack.Peek().OnResume();
        }
    }



    /* -------------------------------------------------------------- UIPanel基类 -------------------------------------------------------- */

    /// <summary>
    /// Panel基类
    /// </summary>
    public abstract class PanelBase : MonoBehaviour, IController
    {
        /// <summary>
        /// Panel控制组件
        /// </summary>
        public CanvasGroup canvasGroup;


        /* -------------------------------------------- 生命周期 -------------------------------------------- */

        /// <summary>
        /// Panel初始化时
        /// </summary>
        public virtual UniTask OnInit<T>(Action<T> onInit = null) where T : PanelBase 
        {
            // 可以 异步加载，获取，添加 资源和组件
            // CanvasGroup
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
            // 回调
            onInit?.Invoke(this as T);

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Panel显示时
        /// </summary>
        public virtual void OnOpen()
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.alpha = 1.0f;

            // 子类 监听UI事件
        }

        /// <summary>
        /// Panel冻结时
        /// </summary>
        public virtual void OnPause()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0.7f;
        }

        /// <summary>
        /// Panel恢复时
        /// </summary>
        public virtual void OnResume()
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.alpha = 1.0f;
        }

        /// <summary>
        /// panel销毁时
        /// </summary>
        public virtual void OnClose()
        {
            GameObject.Destroy(gameObject);
        }


        /* ----------------------------------------- API -------------------------------------------- */

        // 获取 子对象
        public GameObject GetGameObjectInChildren(string name)
        {
            var trans = gameObject.GetComponentsInChildren<Transform>();
            foreach (var item in trans)
            {
                if (item.name == name) return item.gameObject;
            }
            return null;
        }

        public GameObject GetGameObjectInChildren(GameObject parentObj, string name)
        {
            var trans = parentObj.GetComponentsInChildren<Transform>();
            foreach (var item in trans)
            {
                if (item.name == name) return item.gameObject;
            }
            return null;
        }

        // 获取 子对象 的 子组件脚本
        public T GetComponentInChildren<T>(string name) where T : Component
        {
            GameObject obj = GetGameObjectInChildren(name);
            if (obj == null) return null;

            return obj.GetComponent<T>();
        }

        public T GetComponentInChildren<T>(string name, out T comp) where T : Component
        {
            GameObject obj = GetGameObjectInChildren(name);
            if (obj == null)
            {
                comp = null;
                return null;
            }
            comp = obj.GetComponent<T>();
            return comp;
        }

        public T GetComponentInChildren<T>(GameObject parentObj, string name) where T : Component
        {
            GameObject obj = GetGameObjectInChildren(parentObj, name);
            if (obj == null) return null;

            return obj.GetComponent<T>();
        }

        public T GetComponentInChildren<T>(GameObject parentObj, string name, out T comp) where T : Component
        {
            GameObject obj = GetGameObjectInChildren(parentObj, name);
            if (obj == null)
            {
                comp = null;
                return null;
            }
            comp = obj.GetComponent<T>();
            return comp;
        }


        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }



}