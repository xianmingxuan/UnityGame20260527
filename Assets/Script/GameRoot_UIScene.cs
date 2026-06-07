using UnityEngine;
using QFramework;
using System.Threading.Tasks;

namespace UG20260527
{
    public class GameRoot_UIScene : MonoBehaviour, IController
    {
        

        private async void Start()
        {
            // 输入系统：切换为UIMap
            this.GetSystem<IInputSystem>().SwitchActionMap("UIMap");
            this.RegisterEvent<InputActionEvent>(e =>
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
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

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
        }



        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}

