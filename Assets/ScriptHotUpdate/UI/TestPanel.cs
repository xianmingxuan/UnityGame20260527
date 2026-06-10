using QFramework;
using UnityEngine.UI;
using UnityEngine;

namespace UG20260527
{
    public class TestPanel : PanelBase
    {
        public override void OnOpen()
        {
            base.OnOpen();

            GetComponentInChildren<Button>("BtnTest")?.onClick.AddListener(() =>
            {
                Debug.Log("点击测试面板");
                //this.GetSystem<IUISystem>().CloseSinglePanel(panelConfig.panelLayer);
                this.GetSystem<IUISystem>().CloseSinglePanel(PanelLayer.BackgroundLayer, new ClosePanelSetting { panelShouldClose = this});
            });
        }
    }
}
