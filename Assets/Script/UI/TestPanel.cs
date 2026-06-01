using QFramework;
using UnityEngine.UI;
using UnityEngine;

namespace UG20260527
{
    public class TestPanel : PanelBase
    {
        public override void OnEnter()
        {
            base.OnEnter();

            GetComponentInChildren<Button>("BtnTest").onClick.AddListener(() =>
            {
                Debug.Log("点击");
                this.GetSystem<IUISystem>().PopPanel();
            });
        }
    }
}
