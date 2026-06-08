using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UG20260527
{
    public class TestItemData
    {
        public bool select = false;
    }

    public class TestItem : LoopScrollItemBase
    {
        private int _dataIndex;
        private Text _text;
        private Image _img;
        private Color _color;
        private TestItemData _itemData;
        private IUISystem _system;


        public override UniTask OnInit<T>(Action<T> onInit = null, object userData = null)
        {
            _system = this.GetSystem<IUISystem>();
            _text = GetComponentInChildren<Text>("Text");
            _img = GetComponentInChildren<Image>("Image");
            _color = _img.color;

            GetComponentInChildren<Button>("Button").onClick.AddListener(() =>
            {
                if (_itemData == null) return;

                // 显示 动态信息面板
                _system.OpenSinglePanel<DynamicInfoPanel>(panel =>
                {
                    // 面板位置
                    panel.viewport.localPosition = Mouse.current.position.ReadValue() / _system.parentCanvas.Value.GetComponent<Canvas>().scaleFactor;
                    panel.itemType = typeof(DynamicInfoItem);
                    panel.dataList = new List<object> 
                    { 
                        new DynamicInfoItemData(_dataIndex.ToString()),
                        new DynamicInfoItemData(_itemData.select.ToString()) 
                    };
                });
            });

            return base.OnInit(onInit, userData);
        }

        public override void UpdataItem(object data, int dataIndex)
        {
            _dataIndex = dataIndex;
            _itemData = data as TestItemData;
            _text.text = dataIndex.ToString();
            if (_itemData.select) _img.color = Color.red;
            else _img.color = _color;
        }
    }
}

