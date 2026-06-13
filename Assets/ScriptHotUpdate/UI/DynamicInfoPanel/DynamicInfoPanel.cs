using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace UG20260527
{
    /// <summary>
    /// 动态信息面板
    /// </summary>
    public class DynamicInfoPanel : PanelBase
    {
        /// <summary>
        /// 显示的Item类型
        /// </summary>
        public Type itemType;

        /// <summary>
        /// Item数据列表
        /// </summary>
        public List<object> dataList;

        /// <summary>
        /// 动态信息面板视口
        /// </summary>
        public RectTransform viewport;

        private RectTransform _content;
        private IUISystem _system;
        private List<PanelBase> _itemList;


        public override UniTask OnInit<T>(Action<T> onInit = null, object userData = null)
        {
            _content = GetComponentInChildren<RectTransform>("Content");
            viewport = GetComponentInChildren<RectTransform>("Viewport");
            _system = this.GetSystem<IUISystem>();
            _itemList = new List<PanelBase>();

            return base.OnInit(onInit, userData);
        }

        public override async void OnOpen()
        {
            base.OnOpen();

            if (itemType == null || dataList == null || dataList.Count <= 0) return;

            // 监听
            GetComponentInChildren<Button>("Button").onClick.AddListener(() =>
            {
                // 关闭层级Panel
                _system.CloseSinglePanel(panelConfig.panelLayer);
            });

            // 创建Item
            for (int i = 0; i < dataList.Count; i++)
            {
                var item = await _system.OpenSinglePanel(itemType, null, dataList[i], new OpenPanelSetting { isPushStack = false});
                item.transform.SetParent(_content);
                _itemList.Add(item);
            }
            
        }

        public override void OnClose()
        {
            // 移除所有监听
            GetComponentInChildren<Button>("Button").onClick.RemoveAllListeners();

            // 回收时，清空内容Item
            foreach (var item in _itemList)
            {
                _system.CloseSinglePanel(item.panelConfig.panelLayer, new ClosePanelSetting { panelShouldClose = item });
            }
            _itemList.Clear();

            base.OnClose();
        }
    }
}

