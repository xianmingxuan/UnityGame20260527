using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UG20260527
{
    public class TrafficSceneHUDPanel : PanelBase
    {
        // 局内游戏数据
        private ITrafficGameModel _gameModel;

        // 监听句柄 数组
        private List<IUnRegister> unRegisters = new List<IUnRegister>();

        public override void OnOpen()
        {
            base.OnOpen();

            // 获取游戏数据
            _gameModel = this.GetModel<ITrafficGameModel>();

            // Level
            unRegisters.Add(_gameModel.Level.RegisterWithInitValue(value =>
            {
                var text_Level = GetComponentInChildren<Text>("Text_Level");
                if(text_Level != null) text_Level.text = value.ToString();
            }));

            // HP
            unRegisters.Add(_gameModel.HP.RegisterWithInitValue(value =>
            {
                var text_HP = GetComponentInChildren<Text>("Text_HP");
                if(text_HP != null) text_HP.text = value.ToString();
            }));

            unRegisters.Add(_gameModel.numberOfVehicles.RegisterWithInitValue(value =>
            {
                var text_NumberOfVehicles = GetComponentInChildren<Text>("Text_NumberOfVehicles");
                if (text_NumberOfVehicles != null) text_NumberOfVehicles.text = value.ToString();
            }));

            unRegisters.Add(_gameModel.numberOfRecycledVehicles.RegisterWithInitValue(value =>
            {
                var text_NumberOfRecycledVehicles = GetComponentInChildren<Text>("Text_NumberOfRecycledVehicles");
                if (text_NumberOfRecycledVehicles != null) text_NumberOfRecycledVehicles.text = value.ToString();
            }));

            // 绑定监听
            GetComponentInChildren<Button>("Btn_Setting").onClick.AddListener(() =>
            {
                // 退出场景
                this.SendCommand<ExitLatestSceneCommand>();
            });
        }

        public override void OnClose()
        {
            base.OnClose();

            foreach(var unRegister in unRegisters)
            {
                unRegister.UnRegister();
            }
            unRegisters.Clear();

            GetComponentInChildren<Button>("Btn_Setting").onClick.RemoveAllListeners();
        }
    }
}