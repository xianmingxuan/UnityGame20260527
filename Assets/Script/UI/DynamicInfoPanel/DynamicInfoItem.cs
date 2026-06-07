using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UG20260527
{
    public class DynamicInfoItemData
    {
        public string content;

        public DynamicInfoItemData(string content)
        {
            this.content = content;
        }
    }

    /// <summary>
    /// 动态信息面板Item
    /// </summary>
    public class DynamicInfoItem : PanelBase
    {
        public override UniTask OnInit<T>(Action<T> onInit = null, object userData = null)
        {
            return base.OnInit(onInit, userData);
        }

        public override void OnOpen()
        {
            base.OnOpen();

            // 初始化 信息
            var data = userData as DynamicInfoItemData;
            GetComponentInChildren<Text>("Text").text = data.content;

            // 监听
            GetComponentInChildren<Button>("Button").onClick.AddListener(() =>
            {
                this.GetSystem<IUISystem>().OpenSinglePanel<DynamicInfoPanel>(panel => 
                {
                    // 面板位置
                    panel.viewport.localPosition = Mouse.current.position.ReadValue();
                    panel.itemType = typeof(DynamicInfoItem);
                    panel.dataList = new List<object>
                    {
                        new DynamicInfoItemData(data.content)
                    };
                });
            });
        }
    }
}