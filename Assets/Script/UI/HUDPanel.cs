using System.Collections;
using UnityEngine;
using QFramework;
using UnityEngine.UI;

namespace UG20260527
{
    public class HUDPanel : PanelBase
    {
        public Text ScoreText;
        public Button BtnAdd;
        public Button BtnSub;
        public Text AchievementDisplayText;

        private IScoreModel mScoreModel;

        public override void OnEnter()
        {
            base.OnEnter();

            // 初始化 子UI组件
            ScoreText = GetComponentInChildren<Text>("ScoreText");
            AchievementDisplayText = GetComponentInChildren<Text>("AchievementDisplayText");
            BtnAdd = GetComponentInChildren<Button>("BtnAdd");
            BtnSub = GetComponentInChildren<Button>("BtnSub");

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

            // 表现逻辑：监听分数变化，更新UI
            mScoreModel.Score.RegisterWithInitValue(value => UpdateScoreText(value))
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            // 成就表现
            this.RegisterEvent<AchievementDisplayEvent>(e => AchievementDisplay(e.DisplayText))
                .UnRegisterWhenGameObjectDestroyed(gameObject);
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