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

            // HP
            unRegisters.Add(_gameModel.HP.RegisterWithInitValue(value =>
            {
                var text_HP = GetComponentInChildren<Text>("Text_HP");
                if(text_HP != null) text_HP.text = value.ToString();
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

            GetComponentInChildren<Button>("Btn_Setting").onClick.RemoveAllListeners();
        }
    }
}