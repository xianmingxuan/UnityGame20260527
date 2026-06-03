using QFramework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UG20260527
{

    /* ---------------------------------------------------------------- UI系统 -------------------------------------------------------- */

    public interface IUISystem : ISystem
    {
        public Task<T> PushPanel<T>() where T : PanelBase;
        public void PopPanel();
    }

    public class UISystem : AbstractSystem, IUISystem
    {
        /* -------------------------------------------------- 属性 -------------------------------------------------- */

        // Panel 配置表
        private string uiConfigPath = "Assets/Config/UIConfig.asset";
        public UIConfig uiConfig;

        // UI栈
        public Stack<PanelBase> panelStack;

        // 画布面板
        public Transform parentCanvas;


        protected override void OnInit()
        {
            panelStack = new Stack<PanelBase>();
            // 获取 画布面板
            var obj = GameObject.Find("Canvas");
            if (obj == null) Debug.LogWarning("场景中没有 Canvas画布");
            else parentCanvas = obj.transform;
        }

        /* -------------------------------------------------- 内部API函数 -------------------------------------------------- */

        // 加载 UI配置表
        private async Task LoadUIConfig()
        {
            // 加载 Panel配置表
            uiConfig = await Addressables.LoadAssetAsync<UIConfig>(uiConfigPath).Task;
        }

        // 根据Type 获取Panel预制体AssetRef
        private async Task<AssetReference> GetPanelAssetRef<T>() where T : PanelBase
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
        
        // 根据Type 实例化Panel和脚本
        private async Task<GameObject> GetPanel<T>() where T : PanelBase
        {
            // 获取Panel资源
            AssetReference panelAssetRef = await GetPanelAssetRef<T>();
            if(panelAssetRef == null) return null;

            // 实例化Panel
            var panel = await panelAssetRef.InstantiateAsync(parentCanvas, false).Task;

            return panel;
        }


        /* -------------------------------------------------- 接口函数 -------------------------------------------------- */

        async Task<T> IUISystem.PushPanel<T>()
        {
            // 创建Panel
            GameObject panel = await GetPanel<T>();
            if (panel == null)
            {
                return null;
            }

            // 添加 控制脚本
            T panelScript = panel.GetComponent<T>();
            if(panelScript == null) panelScript = panel.AddComponent<T>();

            // 冻结栈顶
            if (panelStack.Count > 0) panelStack.Peek().OnPause();

            // 入栈 并 启用
            panelStack.Push(panelScript);
            panelScript.OnEnter();

            return panelScript;
        }

        void IUISystem.PopPanel()
        {
            if (panelStack.Count <= 0) return;

            panelStack.Peek().OnExit();
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
        /// Panel显示时
        /// </summary>
        public virtual void OnEnter()
        {
            // Panel控制
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup  == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.alpha = 1.0f;
        }

        /// <summary>
        /// panel冻结时
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
        public virtual void OnExit()
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