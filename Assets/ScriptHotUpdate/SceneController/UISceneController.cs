using Cysharp.Threading.Tasks;
using QFramework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UG20260527
{
    public class UISceneController : SceneControllerBase
    {
        /* -------------------------------------------------- 属性 -------------------------------------------------- */

        // 临时注册句柄List（随着场景切换，重复注册、注销）
        private List<IUnRegister> _unRegisterTempList = new List<IUnRegister>();
        
        // 生命周期注册句柄List（本对象创建时注册，销毁时注销）
        private List<IUnRegister> _unRegisterLifeList = new List<IUnRegister>();

        // Main界面
        private MainPanel _mainPanel;

        

        /* -------------------------------------------------- 生命周期 -------------------------------------------------- */

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override async UniTask OnInit(SceneConfigData sceneConfig)
        {
            await base.OnInit(sceneConfig);
            
            // 监听：OnInit Scene
            var unRegisterHandle1 = this.RegisterEvent<LoadSceneEvent>(async e =>
            {
                // 隐藏MainPanel，显示LoadingPanel
                //Debug.Log($"UI场景控制器 - OnInit 场景：{e.payload.sceneControllerType.Name}");
                await this.GetSystem<IUISystem>().OpenSinglePanel<LoadingPanel>(null, new LoadingPanelData(e.payload.handle));
                await this.GetSystem<IUISystem>().CloseSinglePanel(_mainPanel.panelConfig.panelLayer, new ClosePanelSetting { panelShouldClose = _mainPanel });
            });

            // 监听：PreEnter Scene
            var unRegisterHandle2 = this.RegisterEvent<PreEnterSceneEvent>(async e =>
            {
                // 隐藏LoadingPanel，显示HUD
                //Debug.Log($"UI场景控制器 - PreEnter 场景：{e.payload.sceneControllerType.Name}");
                await this.GetSystem<IUISystem>().CloseSinglePanel(PanelLayer.LoadingLayer);
            });

            // 监听：PreExit PlayScene
            var unRegisterHandle3 = this.RegisterEvent<PreExitSceneEvent>(async e =>
            {
                //Debug.Log($"退出的场景： {e.exitSceneControllerType.Name}");
                // 进入UI场景
                await EnterUIScene();
            });

            _unRegisterLifeList.Add(unRegisterHandle1);
            _unRegisterLifeList.Add(unRegisterHandle2);
            _unRegisterLifeList.Add(unRegisterHandle3);
        }

        /// <summary>
        /// 打开UI场景，打开UI
        /// </summary>
        /// <param name="sceneInstance"></param>
        /// <returns></returns>
        public override async UniTask OnPreEnter(SceneInstance sceneInstance)
        {
            await base.OnPreEnter(sceneInstance);

            // 进入 UI场景
            await EnterUIScene();
        }

        /// <summary>
        /// 销毁UI场景
        /// </summary>
        public override void OnPreExit()
        {
            base.OnPreExit();

            // 注销 临时监听
            UnRegisterTempList();

            // 注销 生命周期监听
            UnRegisterLifeList();
        }


        /* -------------------------------------------------- API函数 -------------------------------------------------- */

        // 进入 UI场景
        private async UniTask EnterUIScene()
        {
            // 注销 临时监听
            UnRegisterTempList();

            // 输入系统：切换为UIMap
            this.GetSystem<IInputSystem>().SwitchActionMap("UIMap");
            var unRegisterHandle = this.RegisterEvent<InputActionEvent>(e =>
            {
                if (!string.IsNullOrEmpty(e.mapName) && e.mapName == "UIMap")
                {
                    switch (e.actionName)
                    {
                        case "ESC":
                            if (e.context.started)
                            {
                                this.GetSystem<IUISystem>().CloseSinglePanel();
                                Debug.Log("按下ESC");
                            }
                            break;
                    }
                }
            });
            _unRegisterTempList.Add(unRegisterHandle);

            // UI系统：创建 MainPanel
            _mainPanel = await this.GetSystem<IUISystem>().OpenSinglePanel<MainPanel>(panelSC =>
            {
                // 逻辑注入
                panelSC.InitPageActive("Toggle_Activity", false);
                //panelSC.InitPageActive("Toggle_Bag", false);
                //panelSC.InitPageActive("Toggle_BeginPlay", false);
                panelSC.InitToggleGroup(1);
            }, null, new OpenPanelSetting { isPushStack = false });

        }

        // 注销 临时监听
        private void UnRegisterTempList()
        {
            // 注销监听
            if (_unRegisterTempList != null && _unRegisterTempList.Count > 0)
            {
                foreach (var unRegister in _unRegisterTempList)
                {
                    unRegister.UnRegister();
                }
                _unRegisterTempList.Clear();
            }
        }

        // 注销 生命周期监听
        private void UnRegisterLifeList()
        {
            // 注销 生命周期监听
            if (_unRegisterLifeList != null && _unRegisterLifeList.Count > 0)
            {
                foreach (var unRegister in _unRegisterLifeList)
                {
                    unRegister.UnRegister();
                }
                _unRegisterLifeList.Clear();
            }
        }



    }
}