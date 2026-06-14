using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UG20260527
{
    /// <summary>
    /// UI面板的层级
    /// </summary>
    public enum PanelLayer
    {
        /// <summary>
        /// 背景层：位于画面最底部，作为背景
        /// </summary>
        BackgroundLayer,
        /// <summary>
        /// 正常层（弹窗层）：
        /// 1.常驻界面，如技能栏、背包、任务栏等；
        /// 2.临时可交互界面，如设置面板、背包详情、“确定，取消”等
        /// 3.作为某个Normal面板 的 子面板界面，如 背包子面板，开始游戏子面板等
        /// </summary>
        NormalLayer,
        /// <summary>
        /// 提示层：短暂出现的非交互式信息，例如“道具已获得”、“网络连接失败”等
        /// </summary>
        TipLayer,
        /// <summary>
        /// 新手引导层：新手引导或强制操作指引
        /// </summary>
        GuideLayer,
        /// <summary>
        /// 加载层：场景切换或异步加载资源时显示加载界面和进度条
        /// </summary>
        LoadingLayer,
        /// <summary>
        /// 最上层（杂项层）：显示最高优先级的非阻断信息，如电池电量、网络信号、当前时间等状态栏信息
        /// </summary>
        TopLayer,
    }

    /// <summary>
    /// 打开面板时的特殊设置
    /// </summary>
    public struct OpenPanelSetting
    {
        /// <summary>
        /// 是否入栈（让UI栈控制面板冻结，子面板一般不入栈）
        /// </summary>
        public bool isPushStack;
        // 旧面板是否冻结
        // 旧面板冻结时，透明度设置成多少

        public OpenPanelSetting(bool isPushStack)
        {
            this.isPushStack = isPushStack;
        }

        /// <summary>
        /// 构建 默认值（正常情况下都使用这个默认值来OpenPanel）
        /// </summary>
        /// <returns></returns>
        public static OpenPanelSetting DefaultValue() => new OpenPanelSetting(true);
    }

    /// <summary>
    /// 关闭面板时的特殊设置
    /// </summary>
    public struct ClosePanelSetting
    {
        /// <summary>
        /// 需要关闭的Panel，一般是不受UI栈控制的Panel，如子面板
        /// </summary>
        public PanelBase panelShouldClose;

        public ClosePanelSetting(PanelBase panel)
        {
            this.panelShouldClose = panel;
        }

        /// <summary>
        /// 构建 默认值（正常情况下都使用这个默认值来关闭Panel）
        /// </summary>
        /// <returns></returns>
        public static ClosePanelSetting DefaultValue() => new ClosePanelSetting(null);
    }


    /* -------------------------------------------------------------------------------- UI系统 ---------------------------------------------------------------------------- */

    public interface IUISystem : ISystem
    {
        public UniTask<T> OpenSinglePanel<T>(Action<T> onInit = null, object userData = null, OpenPanelSetting? openPanelSetting = null) where T : PanelBase;
        public UniTask<PanelBase> OpenSinglePanel(Type type, Action<PanelBase> onInit = null, object userData = null, OpenPanelSetting? openPanelSetting = null);
        public UniTask CloseSinglePanel(PanelLayer layer = PanelLayer.NormalLayer, ClosePanelSetting? closePanelSetting = null);
        public BindableProperty<Transform> parentCanvas { get; }
    }

    public class UISystem : AbstractSystem, IUISystem
    {
        /* -------------------------------------------------- 属性 -------------------------------------------------- */

        // Panel 配置表
        private string uiConfigPath = "Assets/AddressablesAsset/Config/UIConfig.asset";
        private UIConfig uiConfig;

        // UI栈
        private Dictionary<PanelLayer, List<PanelBase>> _panelStacks;
        // 层级对象，Panel创建后需要挂载到目标层级gameObject下
        private Dictionary<PanelLayer, RectTransform> _layerGameObjects;
        // 正在加载 准备入栈的Panel数量
        private BindableProperty<int> pushingPanelCount { get; } = new BindableProperty<int>(0);

        // 画布面板
        public BindableProperty<Transform> parentCanvas { get; private set; } = new BindableProperty<Transform>(null);


        protected override void OnInit()
        {
            // 获取 画布面板
            var obj = GameObject.Find("Canvas");
            if (obj == null) Debug.LogWarning("场景中没有 Canvas画布");
            else parentCanvas.Value = obj.transform;

            // 初始化Panel栈 和 层级对象
            _panelStacks = new Dictionary<PanelLayer, List<PanelBase>>();
            _layerGameObjects = new Dictionary<PanelLayer, RectTransform>();
            foreach (PanelLayer layer in Enum.GetValues(typeof(PanelLayer)))
            {
                // 创建 层级对象
                GameObject layerObj = new GameObject(layer.ToString());
                layerObj.AddComponent<RectTransform>();
                layerObj.layer = LayerMask.NameToLayer("UI");
                // 层级对象 设置：父对象为Canvas，全屏拉伸，上下左右间隔为0，本地坐标为0，本地缩放为1
                RectTransform layerTrans = layerObj.transform as RectTransform;
                layerTrans.SetParent(parentCanvas.Value);
                layerTrans.anchorMin = Vector2.zero;
                layerTrans.anchorMax = Vector2.one;
                layerTrans.offsetMin = Vector2.zero;
                layerTrans.offsetMax = Vector2.zero;
                layerTrans.localPosition = Vector3.zero;
                layerTrans.localScale = Vector3.one;
                _layerGameObjects.Add(layer, layerTrans);
            }

            // 加载中Panel数量
            pushingPanelCount.Register(value =>
            {
                if (value < 0) pushingPanelCount.Value = 0;
            });
        }

        /* -------------------------------------------------- Panel栈函数 -------------------------------------------------- */

        // 获取、添加UI栈
        private List<PanelBase> GetOrAddPanelStack(PanelLayer layer)
        {
            if (!_panelStacks.ContainsKey(layer))
            {
                // 添加 目标层级的UI栈
                _panelStacks.Add(layer, new List<PanelBase>());
            }
            if (!_layerGameObjects.ContainsKey(layer))
            {
                // 创建 层级对象
                GameObject layerObj = new GameObject(layer.ToString());
                layerObj.AddComponent<RectTransform>();
                layerObj.layer = LayerMask.NameToLayer("UI");
                // 层级对象 设置：父对象为Canvas，全屏拉伸，上下左右间隔为0，本地坐标为0，本地缩放为1
                RectTransform layerTrans = layerObj.transform as RectTransform;
                layerTrans.SetParent(parentCanvas.Value);
                layerTrans.anchorMin = Vector2.zero;
                layerTrans.anchorMax = Vector2.one;
                layerTrans.offsetMin = Vector2.zero;
                layerTrans.offsetMax = Vector2.zero;
                layerTrans.localPosition = Vector3.zero;
                layerTrans.localScale = Vector3.one;
                _layerGameObjects.Add(layer, layerTrans);
            }
            return _panelStacks[layer];
        }

        // 入栈 目标脚本
        void PushPanelStack(PanelBase panelScript)
        {
            // 获取目标层级UI栈
            List<PanelBase> panelStack = GetOrAddPanelStack(panelScript.panelConfig.panelLayer);
            panelScript.transform.SetParent(_layerGameObjects[panelScript.panelConfig.panelLayer]);
            panelScript.transform.localPosition = Vector3.zero;
            panelScript.transform.localScale = Vector3.one;

            // 冻结栈顶
            if (panelStack.Count > 0) panelStack[panelStack.Count - 1].OnPause();

            // 入栈 并 启用
            panelStack.Add(panelScript);
            panelScript.OnOpen();
        }

        // 弹栈 目标层级
        void PopPanelStack(PanelLayer layer)
        {
            // 检查是否存在UI栈
            if (_panelStacks.Count <= 0) return;
            if (!_panelStacks.ContainsKey(layer)) return;
            if (_panelStacks[layer].Count <= 0) return;

            // 弹出
            _panelStacks[layer][_panelStacks[layer].Count - 1].OnClose();
            _panelStacks[layer].RemoveAt(_panelStacks[layer].Count - 1);

            // 恢复
            if (_panelStacks[layer].Count > 0) _panelStacks[layer][_panelStacks[layer].Count - 1].OnResume();
        }

        // 弹栈 目标Panel（仅弹栈，不执行Panel的OnClose，脱离UI栈的管理）
        void PopPanelStackOnly(PanelBase targetPanel)
        {
            // 遍历所有UI栈，将目标Panel踢出
            if (_panelStacks.Values.Count > 0)
            {
                bool isComplete = false;
                foreach (var stack in _panelStacks.Values)
                {
                    if (isComplete) break;
                    if (stack == null || stack.Count <= 0) continue;
                    foreach (var panel in stack)
                    {
                        if (panel == null) continue;
                        if (panel == targetPanel)
                        {
                            stack.Remove(panel);
                            // 恢复
                            if (stack.Count > 0 && stack[stack.Count - 1].isPause) stack[stack.Count - 1].OnResume();
                            isComplete = true;
                            break;
                        }
                    }
                }
            }
        }


        /* -------------------------------------------------- API函数 -------------------------------------------------- */

        // 加载 UI配置表
        private async UniTask LoadUIConfig()
        {
            // 加载 Panel配置表
            var handle = Addressables.LoadAssetAsync<object>(uiConfigPath);
            await handle.Task;
            uiConfig = handle.Result as UIConfig;
            uiConfig.InitConfig();
        }

        // 根据Type 获取Panel预制体路径
        private async UniTask<string> GetPanelPrefabPath<T>() where T : PanelBase
        {
            if (uiConfig == null)
            {
                await LoadUIConfig();
            }

            if (!uiConfig.panelConfigDic.ContainsKey(typeof(T).Name))
            {
                Debug.LogWarning($"{uiConfig.name} 配置表中没有 {typeof(T).Name} 资源");
                return null;
            }

            return uiConfig.panelConfigDic[typeof(T).Name].panelAssetPath;
        }

        private async UniTask<string> GetPanelPrefabPath(Type type)
        {
            if (uiConfig == null)
            {
                await LoadUIConfig();
            }

            if (!uiConfig.panelConfigDic.ContainsKey(type.Name))
            {
                Debug.LogWarning($"{uiConfig.name} 配置表中没有 {type.Name} 资源");
                return null;
            }

            return uiConfig.panelConfigDic[type.Name].panelAssetPath;
        }

        // 根据Type 实例化Panel和脚本
        private async UniTask<T> CreatePanel<T>(Action<T> onInit, object userData) where T : PanelBase
        {
            // 获取Panel资源
            string panelPrefabPath = await GetPanelPrefabPath<T>();
            if(string.IsNullOrEmpty(panelPrefabPath)) return null;

            // 实例化Panel
            //var panel = await Addressables.InstantiateAsync(panelPrefabPath, parentCanvas.Value, false).Task;
            var panel = await this.GetSystem<IResourceSystem>().LoadAndInstantiateAsync<GameObject>(panelPrefabPath, parentCanvas.Value, false);
            panel.transform.localPosition = Vector3.zero;
            panel.transform.localScale = Vector3.one;

            // 添加 控制脚本
            T panelScript = panel.GetComponent<T>();
            if (panelScript == null) panelScript = panel.AddComponent<T>();

            // 初始化配置脚本
            panelScript.panelConfig = uiConfig.panelConfigDic[typeof(T).Name];  // 面板配置：面板层级等
            await panelScript.OnInit(onInit, userData);

            return panelScript;
        }

        private async UniTask<PanelBase> CreatePanel(Type type, Action<PanelBase> onInit, object userData)
        {
            // 获取Panel资源
            string panelPrefabPath = await GetPanelPrefabPath(type);
            if (string.IsNullOrEmpty(panelPrefabPath)) return null;

            // 实例化Panel
            //var panel = await Addressables.InstantiateAsync(panelPrefabPath, parentCanvas.Value, false).Task;
            var panel = await this.GetSystem<IResourceSystem>().LoadAndInstantiateAsync<GameObject>(panelPrefabPath, parentCanvas.Value, false);
            panel.transform.localPosition = Vector3.zero;
            panel.transform.localScale = Vector3.one;

            // 添加 控制脚本
            PanelBase panelScript = panel.GetComponent(type) as PanelBase;
            if (panelScript == null) panelScript = panel.AddComponent(type) as PanelBase;

            // 初始化配置脚本
            panelScript.panelConfig = uiConfig.panelConfigDic[type.Name];  // 面板配置：面板层级等
            await panelScript.OnInit(onInit, userData);

            return panelScript;
        }


        /* -------------------------------------------------- 接口函数 -------------------------------------------------- */

        async UniTask<T> IUISystem.OpenSinglePanel<T>(Action<T> onInit, object userData, OpenPanelSetting? openPanelSetting)
        {
            // OpenPanel时的特殊配置
            OpenPanelSetting setting = openPanelSetting ?? OpenPanelSetting.DefaultValue();


            // 不入栈（由创建者自己管理，例如：子面板）
            if (!setting.isPushStack)
            {
                T panelSC = await CreatePanel<T>(onInit, userData);
                panelSC.OnOpen();
                return panelSC;
            }

            // 加载中Panel++
            pushingPanelCount.Value++;

            // 创建Panel
            T panelScript = await CreatePanel<T>(onInit, userData);
            if (panelScript == null)
            {
                // 加载中Panel--
                pushingPanelCount.Value--;
                return null;
            }

            // 入栈
            PushPanelStack(panelScript);

            // 加载中Panel--
            pushingPanelCount.Value--;
            return panelScript;
        }

        async UniTask<PanelBase> IUISystem.OpenSinglePanel(Type type, Action<PanelBase> onInit, object userData, OpenPanelSetting? openPanelSetting)
        {
            // OpenPanel时的特殊配置
            OpenPanelSetting setting = openPanelSetting ?? OpenPanelSetting.DefaultValue();

            // 不入栈（由创建者自己管理，例如：子面板）
            if (!setting.isPushStack)
            {
                PanelBase panelSC = await CreatePanel(type, onInit, userData);
                panelSC.OnOpen();
                return panelSC;
            }

            // 加载中Panel++
            pushingPanelCount.Value++;

            // 创建Panel
            PanelBase panelScript = await CreatePanel(type, onInit, userData);
            if (panelScript == null)
            {
                // 加载中Panel--
                pushingPanelCount.Value--;
                return null;
            }

            // 入栈
            PushPanelStack(panelScript);

            // 加载中Panel--
            pushingPanelCount.Value--;
            return panelScript;
        }



        async UniTask IUISystem.CloseSinglePanel(PanelLayer layer, ClosePanelSetting? closePanelSetting)
        {
            // 先等其它 push任务 完成
            if (pushingPanelCount.Value > 0) await UniTask.WaitUntil(() => pushingPanelCount.Value <= 0);

            // 弹出指定Panel
            ClosePanelSetting setting = closePanelSetting ?? ClosePanelSetting.DefaultValue();
            if(setting.panelShouldClose != null)
            {
                // 弹栈 目标Panel
                PopPanelStackOnly(setting.panelShouldClose);

                // 关闭 目标Panel
                setting.panelShouldClose.OnClose();
                return;
            }

            // 弹栈
            PopPanelStack(layer);
        }

        
    }



    /* ----------------------------------------------------------------------------- UIPanel基类 ---------------------------------------------------------------------------- */

    /// <summary>
    /// Panel基类
    /// </summary>
    public abstract class PanelBase : MonoBehaviour, IController
    {
        /// <summary>
        /// 面板配置：包含 面板层级
        /// </summary>
        public PanelConfig panelConfig;

        /// <summary>
        /// Panel控制组件
        /// </summary>
        public CanvasGroup canvasGroup;

        /// <summary>
        /// 初始化数据（数据注入）
        /// </summary>
        public object userData;

        /// <summary>
        /// 是否处于 冻结状态
        /// </summary>
        public bool isPause = false;


        /* -------------------------------------------- 生命周期 -------------------------------------------- */

        /// <summary>
        /// Panel初始化时
        /// </summary>
        public virtual UniTask OnInit<T>(Action<T> onInit = null, object userData = null) where T : PanelBase 
        {
            // 可以 异步加载，获取，添加 资源和组件
            // CanvasGroup
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
            // 数据注入
            this.userData = userData;

            // 逻辑注入：回调
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
            isPause = true;
        }

        /// <summary>
        /// Panel恢复时
        /// </summary>
        public virtual void OnResume()
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.alpha = 1.0f;
            isPause = false;
        }

        /// <summary>
        /// panel销毁时
        /// </summary>
        public virtual void OnClose()
        {
            //GameObject.Destroy(gameObject);
            // 回收
            this.GetSystem<IResourceSystem>().Recycle(gameObject);
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