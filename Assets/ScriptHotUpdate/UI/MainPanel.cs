using Cysharp.Threading.Tasks;
using QFramework;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UG20260527
{
    public class MainPanel : PanelBase
    {
        /* -------------------------------------------------- 属性 -------------------------------------------------- */

        // 子面板（Main 管理 生命周期）
        private BeginGamePanel panel_BeginPlay = null;
        private BagPanel panel_Bag = null;

        /* -------------------------------------------------- 生命周期 -------------------------------------------------- */

        public override async UniTask OnInit<T>(Action<T> onInit = null, object userData = null)
        {
            // 回调 初始化
            await base.OnInit<T>(null, userData);

            // 可以 异步加载，获取，添加 资源

            // 执行回调
            onInit?.Invoke(this as T);
        }

        public override void OnOpen()
        {
            base.OnOpen();

            // 切换页面 BeginPlay
            Toggle toggle_BeginPlay;
            GetComponentInChildren<Toggle>("Toggle_BeginPlay", out toggle_BeginPlay)?.onValueChanged.AddListener(async IsOn =>
            {
                if (IsOn)
                {
                    //Debug.Log($"切换到页面 {GetComponentInChildren<Text>(toggle_BeginPlay.gameObject, "Label").text}");
                    toggle_BeginPlay.interactable = false;
                    if (!panel_BeginPlay)
                    {
                        panel_BeginPlay = await this.GetSystem<IUISystem>().OpenSinglePanel<BeginGamePanel>(panelSC =>
                        {
                            panelSC.transform.SetParent(GetComponentInChildren<Transform>("PageContext"));
                        }, new BeginGamePanelData(this), new OpenPanelSetting { isPushStack = false});
                    }
                    panel_BeginPlay.gameObject.SetActive(true);
                }
                else
                {
                    //Debug.Log($"取消页面 {GetComponentInChildren<Text>(toggle_BeginPlay.gameObject, "Label").text}");
                    toggle_BeginPlay.interactable = true;
                    if (panel_BeginPlay && panel_BeginPlay.gameObject.activeSelf)
                    {
                        panel_BeginPlay.gameObject.SetActive(false);
                    }
                }
            });
            toggle_BeginPlay?.onValueChanged.Invoke(toggle_BeginPlay.isOn);  // 手动触发一次，用于初始化

            // 切换页面 Bag
            Toggle toggle_Bag;
            GetComponentInChildren<Toggle>("Toggle_Bag", out toggle_Bag)?.onValueChanged.AddListener(async IsOn =>
            {
                if (IsOn)
                {
                    //Debug.Log($"切换到页面 {GetComponentInChildren<Text>(toggle_Bag.gameObject, "Label").text}");
                    toggle_Bag.interactable = false;
                    if (!panel_Bag)
                    {
                        panel_Bag = await this.GetSystem<IUISystem>().OpenSinglePanel<BagPanel>(panelSC =>
                        {
                            panelSC.transform.SetParent(GetComponentInChildren<Transform>("PageContext"));
                        }, null, new OpenPanelSetting { isPushStack = false });
                    }
                    panel_Bag.gameObject.SetActive(true);
                }
                else
                {
                    //Debug.Log($"取消页面 {GetComponentInChildren<Text>(toggle_Bag.gameObject, "Label").text}");
                    toggle_Bag.interactable = true;
                    if (panel_Bag && panel_Bag.gameObject.activeSelf)
                    {
                        panel_Bag.gameObject.SetActive(false);
                    }
                }
                
            });
            toggle_Bag?.onValueChanged.Invoke(toggle_Bag.isOn);

            // 点击头像
            Button btn_Portrait;
            GetComponentInChildren<Button>("Btn_Portrait", out btn_Portrait).onClick.AddListener(async () =>
            {
                Debug.Log($"点击 {GetComponentInChildren<Text>(btn_Portrait.gameObject, "Text").text}");
                await this.GetSystem<IUISystem>().OpenSinglePanel<TestPanel>();
            });

            // 点击设置
            Button btn_Setting;
            GetComponentInChildren<Button>("Btn_Setting", out btn_Setting).onClick.AddListener(() =>
            {
                Debug.Log($"点击 {GetComponentInChildren<Text>(btn_Setting.gameObject, "Text").text}");
            });

        }

        public override void OnPause()
        {
            base.OnPause();

            // 子面板 冻结
            //if (panel_BeginPlay) panel_BeginPlay.OnPause();
            //if (panel_Bag) panel_Bag.OnPause();
        }

        public override void OnResume()
        {
            base.OnResume();

            // 子面板 恢复
            //if(panel_BeginPlay) panel_BeginPlay.OnResume();
            //if (panel_Bag) panel_Bag.OnResume();
        }

        public override void OnClose()
        {
            // 子面板 销毁（回收）
            var sys = this.GetSystem<IUISystem>();
            if (panel_BeginPlay) sys.CloseSinglePanel(PanelLayer.NormalLayer, new ClosePanelSetting { panelShouldClose = panel_BeginPlay });
            panel_BeginPlay = null;
            if (panel_Bag) sys.CloseSinglePanel(PanelLayer.NormalLayer, new ClosePanelSetting { panelShouldClose = panel_Bag });
            panel_Bag = null;

            base.OnClose();
        }


        /* -------------------------------------------------- 初始化（多态） -------------------------------------------------- */

        // 设置 初始页面
        public MainPanel InitToggleGroup(int index)
        {
            // 所有页面
            var toggleGroup = GetComponentInChildren<ToggleGroup>("ToggleGroup");
            Toggle[] arr = toggleGroup.GetComponentsInChildren<Toggle>();

            // 单独打开index页面
            if (arr.Length <= index) return this;
            arr[index].isOn = true;
            arr[index].Select();
            arr[index].interactable = false;

            return this;
        }

        // 设置  页面切换按钮显示
        public MainPanel InitPageActive(string pageName, bool isActive)
        {
            var page = GetGameObjectInChildren(pageName);
            if(page) page.SetActive(isActive);

            return this;
        }
    }
}
