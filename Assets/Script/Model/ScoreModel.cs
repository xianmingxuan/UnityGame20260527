using QFramework;

namespace UG20260527
{
    // 数据层

    public interface IScoreModel : IModel
    {
        public BindableProperty<int> Score { get; }
    }

    public class ScoreModel : AbstractModel, IScoreModel
    {
        public BindableProperty<int> Score { get; } = new BindableProperty<int>(0);

        protected override void OnInit()
        {
            // 缓存 存储工具
            var StorageUtility = this.GetUtility<IStorageUtility>();

            // 读取 分数
            Score.Value = StorageUtility.LoadInt(nameof(Score), 0);

            // 保存 分数
            Score.Register(value =>
            {
                StorageUtility.SaveInt(nameof(Score), value);
            });
        }
    }
}