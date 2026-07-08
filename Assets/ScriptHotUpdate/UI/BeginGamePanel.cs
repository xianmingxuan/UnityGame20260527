using Cysharp.Threading.Tasks;
using QFramework;
using System;
using UnityEngine.UI;

namespace UG20260527
{
    public class BeginGamePanelData
    {
        /// <summary>
        /// 面板的所有者
        /// </summary>
        public PanelBase owner;

        public BeginGamePanelData(PanelBase owner)
        {
            this.owner = owner;
        }
    }

    public class BeginGamePanel : PanelBase
    {
        /// <summary>
        /// 拥有者
        /// </summary>
        private PanelBase owner;

        public override UniTask OnInit<T>(Action<T> onInit = null, object userData = null)
        {
            if(userData != null)
            {
                var data = userData as BeginGamePanelData;
                if(data != null)
                {
                    owner = data.owner;
                }
            }

            return base.OnInit(onInit, userData);
        }

        public override void OnOpen()
        {
            base.OnOpen();

            GetComponentInChildren<Button>("Btn_Fight")?.onClick.AddListener( () =>
            {
                // 发送命令：进入 PlayScene
                this.SendCommand<EnterPlaySceneCommand>();
            });

            GetComponentInChildren<Button>("Btn_Qualifying")?.onClick.AddListener(() =>
            {
                //this.SendCommand<EnterTrafficSceneCommand>();
                this.GetSystem<IUISystem>().OpenSinglePanel<SaveGamePanel>();
            });
        }

        public override void OnClose()
        {
            base.OnClose();

            GetComponentInChildren<Button>("Btn_Fight")?.onClick.RemoveAllListeners();
            GetComponentInChildren<Button>("Btn_Qualifying")?.onClick.RemoveAllListeners();
        }
    }
}

