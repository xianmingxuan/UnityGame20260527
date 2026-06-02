using UnityEngine;
using QFramework;
using System.Threading.Tasks;

namespace UG20260527
{
    public class GameRoot_UIScene : MonoBehaviour, IController
    {
        

        private async void Start()
        {
            // MainPanel
            var mainPanelSC = await this.GetSystem<IUISystem>().PushPanel<MainPanel>();
            mainPanelSC.SetToggleGroup(1);
        }



        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}

