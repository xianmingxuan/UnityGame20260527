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
        public Task<GameObject> PushPanel(PanelBase panelScript);
        public void PopPanel();
    }

    public class UISystem : AbstractSystem, IUISystem
    {
        /* -------------------------------------------------- 属性 -------------------------------------------------- */

        // Panel 配置表
        private string panelConfigPath = "Assets/Config/PanelConfig.asset";
        public PanelConfig panelConfig;

        // UI栈
        public Stack<PanelBase> panelStack = new Stack<PanelBase>();

        // 画布面板
        public Transform parentCanvas;


        protected override void OnInit()
        {
            // 获取 画布面板
            var obj = GameObject.Find("Canvas");
            if(obj == null) Debug.LogWarning("场景中没有 Canvas画布");
            else parentCanvas = obj.transform;
        }

        /* -------------------------------------------------- 内部API函数 -------------------------------------------------- */

        private async Task<AssetReference> GetPanelAssetRef(string panelName)
        {
            if(panelConfig == null)
            {
                await LoadPanelConfig();
            }

            if(!panelConfig.panelAssetRefDic.ContainsKey(panelName))
            {
                Debug.LogWarning($"{panelConfig.name} 配置表中没有 {panelName} 资源");
                return null;
            }

            return panelConfig.panelAssetRefDic[panelName];
        }

        private async Task LoadPanelConfig()
        {
            // 加载 Panel配置表
            panelConfig = await Addressables.LoadAssetAsync<PanelConfig>(panelConfigPath).Task;
        }

        // 加载并显示Panel
        private async Task<GameObject> GetPanel(PanelBase panelScript)
        {
            // 加载 资源引用
            var panelRef = await GetPanelAssetRef(panelScript.GetType().Name);
            if (panelRef == null) return null;

            // 加载 panelGameObject
            var panel = await panelRef.InstantiateAsync(parentCanvas, false).Task;

            // 初始化脚本变量
            panelScript.panelRef = panelRef;
            panelScript.panelGameObject = panel;
            panelScript.parentCanvas = parentCanvas;

            return panel;
        }

        /* -------------------------------------------------- 接口函数 -------------------------------------------------- */

        public async Task<GameObject> PushPanel(PanelBase panelScript)
        {
            // 创建Panel
            GameObject panel = await GetPanel(panelScript);
            if(panel == null) return null;

            // 冻结栈顶
            if(panelStack.Count > 0) panelStack.Peek().OnPause();

            // 入栈 并 启用
            panelStack.Push(panelScript);
            panelScript.OnEnter();

            return panel;
        }

        void IUISystem.PopPanel()
        {
            if(panelStack.Count <= 0) return;

            panelStack.Peek().OnExit();
            panelStack.Pop();

            if(panelStack.Count > 0) panelStack.Peek().OnResume();
        }
    }



    /* -------------------------------------------------------------- UIPanel基类 -------------------------------------------------------- */

    /// <summary>
    /// Panel基类
    /// </summary>
    public abstract class PanelBase : IController
    {
        /// <summary>
        /// Panel资源引用
        /// </summary>
        public AssetReference panelRef;

        /// <summary>
        /// 面板自身GameObject引用
        /// </summary>
        public GameObject panelGameObject;

        /// <summary>
        /// 画布引用
        /// </summary>
        public Transform parentCanvas;

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
            canvasGroup = GetOrAddComponent<CanvasGroup>();
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
            GameObject.Destroy(panelGameObject);
        }


        /* ----------------------------------------- API -------------------------------------------- */

        public T GetComponent<T>() where T : Component
        {
            return panelGameObject.GetComponent<T>();
        }

        public T AddComponent<T>() where T : Component
        {
            return panelGameObject.AddComponent<T>();
        }

        public T GetOrAddComponent<T>() where T : Component
        {
            T comp = panelGameObject.GetComponent<T>();
            if (comp) return comp;

            panelGameObject.AddComponent<T>();
            return panelGameObject.GetComponent<T>();
        }

        public GameObject GetGameObjectInChildren(string name)
        {
            var trans = panelGameObject.GetComponentsInChildren<Transform>();
            foreach(var item in trans)
            {
                if (item.name == name) return item.gameObject;
            }
            return null;
        }

        public T GetComponentInChildren<T>(string name) where T : Component
        {
            GameObject obj = GetGameObjectInChildren(name);
            if (obj == null) return null;

            return obj.GetComponent<T>();
        }


        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }



}