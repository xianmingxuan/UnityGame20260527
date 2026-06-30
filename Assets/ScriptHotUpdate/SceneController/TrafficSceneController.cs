using Cysharp.Threading.Tasks;
using QFramework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UG20260527
{
    public class TrafficSceneController : SceneControllerBase
    {
        private TrafficSceneHUDPanel _hudPanel;

        public override async UniTask OnPreEnter(SceneInstance sceneInstance)
        {
            await base.OnPreEnter(sceneInstance);

            // 打开UHD
            _hudPanel = await this.GetSystem<IUISystem>().OpenSinglePanel<TrafficSceneHUDPanel>(null, null, new OpenPanelSetting { isPushStack = false });

            List<GameObject> gameObjectsList = new List<GameObject>();
            sceneInstance.Scene.GetRootGameObjects(gameObjectsList);
            foreach(GameObject gameObject in gameObjectsList)
            {
                if(gameObject.name == "GameRoot")
                {
                    //gameObject.GetComponentInChildren<VehicleSpawnController>().Init();
                }
            }
        }

        public override async void OnPreExit()
        {
            base.OnPreExit();

            // 隐藏 HUD
            await this.GetSystem<IUISystem>().CloseSinglePanel(_hudPanel.panelConfig.panelLayer, new ClosePanelSetting { panelShouldClose = _hudPanel });
        }

    }
}
