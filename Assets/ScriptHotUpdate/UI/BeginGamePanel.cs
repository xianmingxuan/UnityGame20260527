using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace UG20260527
{
    public class BeginGamePanel : PanelBase
    {
        public override void OnOpen()
        {
            base.OnOpen();

            GetComponentInChildren<Button>("Btn_Fight")?.onClick.AddListener(async () =>
            {
                Debug.Log("加载场景");
                await this.GetSystem<IResourceSystem>().LoadScenceAsync("Assets/AddressablesAsset/Scenes/SampleScene.unity", UnityEngine.SceneManagement.LoadSceneMode.Additive, true);
            });
        }

        public override void OnClose()
        {
            base.OnClose();

            GetComponentInChildren<Button>("Btn_Fight")?.onClick.RemoveAllListeners();
        }
    }
}

