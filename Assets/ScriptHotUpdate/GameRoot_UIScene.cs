using QFramework;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UI;

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
                panelSC.InitToggleGroup(0);
            }, null, new OpenPanelSetting { isPushStack = false });
            mainPanel.transform.SetParent(GameObject.Find("NormalLayer").transform);

            // 对象池系统
            this.GetSystem<IPoolSystem>().ClearAll();
        }

        void Update()
        {
            //Debug.Log(Mouse.current.position.ReadValue());

            //var system = this.GetSystem<IUISystem>();
            //if (system != null) Debug.Log(system.parentCanvas.Value.GetComponent<Canvas>().scaleFactor);
        }

        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}

