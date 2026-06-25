using QFramework;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UG20260527
{
    public class TrafficSceneHUDPanel : PanelBase
    {
        public override void OnOpen()
        {
            base.OnOpen();

            GetComponentInChildren<Button>("Btn_Setting").onClick.AddListener(() =>
            {
                // 退出场景
                this.SendCommand<ExitLatestSceneCommand>();
            });
        }

        public override void OnClose()
        {
            base.OnClose();

            GetComponentInChildren<Button>("Btn_Setting").onClick.RemoveAllListeners();
        }
    }
}