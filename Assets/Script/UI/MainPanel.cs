using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace UG20260527
{
    public class MainPanel : PanelBase
    {
        /* -------------------------------------------------- 属性 -------------------------------------------------- */


        /* -------------------------------------------------- 生命周期 -------------------------------------------------- */

        public override void OnEnter()
        {
            base.OnEnter();


            // 切换到页面BeginPlay
            Toggle toggle_BeginPlay;
            GetComponentInChildren<Toggle>("Toggle_BeginPlay", out toggle_BeginPlay).onValueChanged.AddListener(IsOn =>
            {
                if (!IsOn) toggle_BeginPlay.interactable = true;
                else
                {
                    toggle_BeginPlay.interactable = false;
                    Debug.Log($"切换到页面 {GetComponentInChildren<Text>(toggle_BeginPlay.gameObject, "Label").text}");
                }
            });

            // 切换到页面Bag
            Toggle toggle_Bag;
            GetComponentInChildren<Toggle>("Toggle_Bag", out toggle_Bag).onValueChanged.AddListener(IsOn =>
            {
                if (!IsOn) toggle_Bag.interactable = true;
                else
                {
                    toggle_Bag.interactable = false;
                    Debug.Log($"切换到页面 {GetComponentInChildren<Text>(toggle_Bag.gameObject, "Label").text}");
                }
                
            });

            // 点击头像
            Button btn_Portrait;
            GetComponentInChildren<Button>("Btn_Portrait", out btn_Portrait).onClick.AddListener(() =>
            {

                Debug.Log($"点击 {GetComponentInChildren<Text>(btn_Portrait.gameObject, "Text").text}");
            });

            // 点击设置
            Button btn_Setting;
            GetComponentInChildren<Button>("Btn_Setting", out btn_Setting).onClick.AddListener(() =>
            {
                Debug.Log($"点击 {GetComponentInChildren<Text>(btn_Setting.gameObject, "Text").text}");
            });



            // 初始化 开始页面
            SetToggleGroup(0);

        }


        /* -------------------------------------------------- 初始化（多态） -------------------------------------------------- */

        // 设置初始页面
        public MainPanel SetToggleGroup(int index)
        {
            var toggleGroup = GetComponentInChildren<ToggleGroup>("ToggleGroup");
            toggleGroup.SetAllTogglesOff();
            Toggle[] arr = toggleGroup.GetComponentsInChildren<Toggle>();
            if (arr.Length <= index) return this;
            arr[index].isOn = true;
            arr[index].Select();
            return this;
        }
    }
}
