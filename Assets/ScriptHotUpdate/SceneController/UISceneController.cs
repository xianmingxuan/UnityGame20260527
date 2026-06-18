using Cysharp.Threading.Tasks;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UG20260527
{
    public class UISceneController : SceneControllerBase
    {
        private List<IUnRegister> _unRegisterList = new List<IUnRegister>();


        public override async UniTask OnEnter(SceneInstance sceneInstance)
        {
            await base.OnEnter(sceneInstance);

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
            _unRegisterList.Add(unRegisterHandle);

            // UI系统：创建 MainPanel
            var mainPanel = await this.GetSystem<IUISystem>().OpenSinglePanel<MainPanel>(panelSC =>
            {
                // 逻辑注入
                panelSC.InitPageActive("Toggle_Activity", false);
                //panelSC.InitPageActive("Toggle_Bag", false);
                //panelSC.InitPageActive("Toggle_BeginPlay", false);
                panelSC.InitToggleGroup(1);
            }, null, new OpenPanelSetting { isPushStack = false });
            mainPanel.transform.SetParent(GameObject.Find("NormalLayer").transform);

            return;
        }

        public override void OnPreExit()
        {
            base.OnPreExit();

            // 注销监听
            if (_unRegisterList != null && _unRegisterList.Count > 0)
            {
                foreach (var unRegister in _unRegisterList)
                {
                    unRegister.UnRegister();
                }
            }
        }







    }
}