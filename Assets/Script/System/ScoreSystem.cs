using QFramework;
using UnityEngine;

namespace UG20260527
{
    // 系统层 （单例模式？）
    public interface IScoreSystem : ISystem
    {

    }
    public class ScoreSystem : AbstractSystem, IScoreSystem
    {
        // 分数数据层
        IScoreModel ScoreModel;

        protected override void OnInit()
        {
            // 缓存 分数模型数据
            ScoreModel  = this.GetModel<IScoreModel>();

            ScoreModel.Score.Register(value =>
            {
                if (value == 10)
                {
                    // 广播：成就展示事件
                    this.SendEvent(new AchievementDisplayEvent("分数达到10"));
                }
                else if (value == 20)
                {
                    // 广播：成就展示事件
                    this.SendEvent(new AchievementDisplayEvent("分数达到20"));
                }
            });
        }
    }
}