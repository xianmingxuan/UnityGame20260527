using QFramework;

namespace UG20260527
{
    // ScoreModel 增删改 操作命令

    public class AddScoreCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetModel<IScoreModel>().Score.Value++;
        }
    }

    public class SubScoreCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetModel<IScoreModel>().Score.Value--;
        }
    }
}