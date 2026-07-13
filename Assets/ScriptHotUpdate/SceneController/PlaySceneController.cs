using Cysharp.Threading.Tasks;
using QFramework;
using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UG20260527
{
    public class PlaySceneController : SceneControllerBase
    {

        // HUD界面
        private HUDPanel _hudPanel;

        public override async UniTask OnPreEnter(SceneInstance sceneInstance)
        {
            await base.OnPreEnter(sceneInstance);
            
            // 显示 HUD
            _hudPanel = await this.GetSystem<IUISystem>().OpenSinglePanel<HUDPanel>(null, null, new OpenPanelSetting { isPushStack = false });

            await UniTask.WaitForSeconds(3);

            // 输入系统
            this.GetSystem<IInputSystem>().SwitchActionMap("PlayerMap");
        }

        public override async UniTask OnPreExit()
        {
            await base.OnPreExit();

            // 隐藏 HUD
            await this.GetSystem<IUISystem>().CloseSinglePanel(_hudPanel.panelConfig.panelLayer, new ClosePanelSetting { panelShouldClose = _hudPanel });
        }
    }
}