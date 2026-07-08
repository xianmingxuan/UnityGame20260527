using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UG20260527
{
    public class SaveGameItemData
    {
        // 具体的存档数据
        /// <summary>
        /// 关卡索引
        /// </summary>
        public int levelIndex = 0;


        public SaveGameItemData(int levelIndex)
        {
            this.levelIndex = levelIndex;
        }
    }

    /// <summary>
    /// 游戏存档Item
    /// </summary>
    public class SaveGameItem : LoopScrollItemBase
    {
        // UI
        private Text Text_Level;
        private Button Btn_Select;

        // 数据
        private List<SaveGameItemData> _datas = new List<SaveGameItemData>();
        private SaveGameItemData _data;
        private int _dataIndex;

        private ISaveGameModel _saveGameModel;


        public override async UniTask OnInit<T>(Action<T> onInit = null, object userData = null)
        {
            await base.OnInit(onInit, userData);

            Text_Level = GetComponentInChildren<Text>("Text_Level");
            Btn_Select = GetComponentInChildren<Button>("Btn_Select");
            _saveGameModel = this.GetModel<ISaveGameModel>();

            // 更新数据 --触发-> 更新UI
            _saveGameModel.selectSaveGameIndex.RegisterWithInitValue(value =>
            {
                if (_data == null) return;

                if(value == _dataIndex)
                {
                    Btn_Select.interactable = false;
                }
                else
                {
                    Btn_Select.interactable = true;
                }
            });
        }

        public override void OnOpen()
        {
            base.OnOpen();

            Btn_Select.onClick.AddListener(() =>
            {
                // 手动点击按钮时：更新数据 --触发-> 更新UI
                this.GetModel<ISaveGameModel>().selectSaveGameIndex.Value = _dataIndex;
                Debug.Log(_dataIndex);
            });
        }

        public override void OnClose()
        {
            base.OnClose();

            Btn_Select.onClick.RemoveAllListeners();
            Btn_Select.interactable = true;
        }

        public override void UpdataItem(List<object> datas, int dataIndex)
        {
            base.UpdataItem(datas, dataIndex);

            // 更新数据
            _dataIndex = dataIndex;
            _datas.Clear();
            foreach (var data in datas)
            {
                _datas.Add(data as SaveGameItemData);
            }
            _data = datas[dataIndex] as SaveGameItemData;

            // 更新UI
            Text_Level.text = _data.levelIndex.ToString();
            Btn_Select.interactable = !(_saveGameModel.selectSaveGameIndex.Value == _dataIndex);
        }
    }
}