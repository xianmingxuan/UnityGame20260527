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
            return base.OnInit(onInit, userData);
        }

        public override void OnOpen()
        {
            base.OnOpen();

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
        }

        public override void OnClose()
        {
            base.OnClose();

            _img.color = _color;
            gameObject.transform.localPosition = Vector3.zero;
            GetComponentInChildren<Button>("Button").onClick.RemoveAllListeners();
        }

        public override void UpdataItem(List<object> datas, int dataIndex)
        {
            _dataIndex = dataIndex;
            _itemData = datas[dataIndex] as TestItemData;
            _text.text = dataIndex.ToString();
            if (_itemData.select) _img.color = Color.red;
            else _img.color = _color;
        }
    }
}

