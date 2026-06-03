using UnityEngine;
using QFramework;

namespace UG20260527
{
    public class GameRoot_UIScene : MonoBehaviour, IController
    {
        

        private async void Start()
        {
            // MainPanel
            await this.GetSystem<IUISystem>().OpenSinglePanel<MainPanel>(panelSC =>
            {
                //panelSC.InitPageActive("Toggle_Activity", false);
                panelSC.InitPageActive("Toggle_Bag", false);
                //panelSC.InitPageActive("Toggle_BeginPlay", false);
                panelSC.InitToggleGroup(1);
            });
        }



        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}

