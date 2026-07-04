using Cysharp.Threading.Tasks;
using QFramework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UG20260527
{
    public class TrafficSceneController : SceneControllerBase
    {
        // HUD
        private TrafficSceneHUDPanel _hudPanel;

        // Model数据
        ITrafficGameModel _gameModel;

        // 
        private List<IUnRegister> _unRegisterList = new List<IUnRegister>();


        public override async UniTask OnInit(SceneConfigData sceneConfig)
        {
            await base.OnInit(sceneConfig);

            // 初始化 Model数据
            _gameModel = this.GetModel<ITrafficGameModel>();
            var handle = this.SendCommand(new InitTrafficGameModelCommand(2));
            await handle.Task;
        }

        public override async UniTask OnPreEnter(SceneInstance sceneInstance)
        {
            await base.OnPreEnter(sceneInstance);

            // 切换输入映射
            this.GetSystem<IInputSystem>().SwitchActionMap("TrafficGameMap");
            var unRegister = this.RegisterEvent<InputActionEvent>(e =>
            {
                if (!string.IsNullOrEmpty(e.mapName) && e.mapName == "TrafficGameMap")
                {
                    switch (e.actionName)
                    {
                        case "ECS":
                            break;
                        case "MouseLeftButton":
                            if (e.context.started)
                            {
                                // 射线检测：点击场景中3D物体
                                //Debug.Log("TrafficGame 鼠标点击");
                                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                RaycastHit hit;
                                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                                {
                                    var comp = hit.transform.gameObject.GetComponent<VehicleController>();
                                    if (comp == null) return;
                                    if (comp.isMoving) comp.isMoving = false;
                                    else comp.isMoving = true;
                                    Debug.Log("鼠标点击 " + hit.transform.gameObject.name);
                                }
                            }
                            break;
                    }
                }
            });
            _unRegisterList.Add(unRegister);

            // 打开UHD
            _hudPanel = await this.GetSystem<IUISystem>().OpenSinglePanel<TrafficSceneHUDPanel>(null, null, new OpenPanelSetting { isPushStack = false });

            await UniTask.WaitForSeconds(1);
        }

        public override async UniTask OnEnter()
        {
            await base.OnEnter();

            // 寻找 场景内的控制器
            List<GameObject> gameObjectsList = new List<GameObject>();
            sceneInstance.Scene.GetRootGameObjects(gameObjectsList);
            foreach (GameObject gameObject in gameObjectsList)
            {
                if (gameObject.name == "GameRoot")
                {
                    // 车辆生成控制器 开始游戏（不需要等待，消除警告）
                    _ = gameObject.GetComponentInChildren<VehicleSpawnController>().BeginGame();
                }
            }
        }

        public override async void OnPreExit()
        {
            base.OnPreExit();

            // 隐藏 HUD
            await this.GetSystem<IUISystem>().CloseSinglePanel(_hudPanel.panelConfig.panelLayer, new ClosePanelSetting { panelShouldClose = _hudPanel });

            foreach(var unR in _unRegisterList)
            {
                unR.UnRegister();
            }
            _unRegisterList.Clear();
        }

    }
}
