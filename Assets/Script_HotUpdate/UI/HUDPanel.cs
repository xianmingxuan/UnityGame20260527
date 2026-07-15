using System.Collections;
using UnityEngine;
using QFramework;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UG20260527
{
    public class HUDPanel : PanelBase
    {
        public Text ScoreText;
        public Button BtnAdd;
        public Button BtnSub;
        public Button BtnExit;
        public Text AchievementDisplayText;

        private IScoreModel mScoreModel;
        private List<IUnRegister> unRegisterList = new List<IUnRegister>();

        public override void OnOpen()
        {
            base.OnOpen();

            // 初始化 子UI组件
            ScoreText = GetComponentInChildren<Text>("ScoreText");
            AchievementDisplayText = GetComponentInChildren<Text>("AchievementDisplayText");
            BtnAdd = GetComponentInChildren<Button>("BtnAdd");
            BtnSub = GetComponentInChildren<Button>("BtnSub");
            BtnExit = GetComponentInChildren<Button>("BtnExit");

            // 获取 分数数据模型
            mScoreModel = this.GetModel<IScoreModel>();

            // 交互逻辑：发送命令，增删改 分数数据模型
            BtnAdd.onClick.AddListener(() =>
            {
                // 发送命令：增加分数
                this.SendCommand<AddScoreCommand>();
            });

            BtnSub.onClick.AddListener(() =>
            {
                // 发送命令：减少分数
                this.SendCommand<SubScoreCommand>();
            });

            BtnExit.onClick.AddListener(() =>
            {
                //this.SendCommand<ExitPlaySceneCommand>();
                this.SendCommand<ExitLatestSceneCommand>();
            });

            // 表现逻辑：监听分数变化，更新UI
            unRegisterList.Add(mScoreModel.Score.RegisterWithInitValue(value => UpdateScoreText(value)));

            // 成就表现
            unRegisterList.Add(this.RegisterEvent<AchievementDisplayEvent>(e => AchievementDisplay(e.DisplayText)));
        }

        public override void OnClose()
        {
            base.OnClose();

            // 注销监听
            BtnAdd.onClick.RemoveAllListeners();
            BtnSub.onClick.RemoveAllListeners();
            BtnExit.onClick.RemoveAllListeners();
            if(unRegisterList != null && unRegisterList.Count > 0)
            {
                foreach(var unRegister in unRegisterList)
                {
                    unRegister.UnRegister();
                }
                unRegisterList.Clear();
            }
        }


        // 更新 分数显示
        void UpdateScoreText(int value)
        {
            ScoreText.text = value.ToString();
        }

        // 成就展示
        void AchievementDisplay(string str)
        {
            AchievementDisplayText.text = str;
        }



        public IArchitecture GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}