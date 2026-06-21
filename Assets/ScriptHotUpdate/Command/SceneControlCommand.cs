using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;

namespace UG20260527
{
    /* ------------------------------------------------------------------------- TheLatestScene 场景（最新创建的场景） ---------------------------------------------------------------------------- */

    // 退出 最新创建的场景
    public class ExitLatestSceneCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            string latestScene = this.GetModel<ISceneModel>().GetLatestSceneController();
            if(string.IsNullOrEmpty(latestScene))
            {
                Debug.Log("没有正在活跃中的场景");
                return;
            }

            // PlayScene场景
            if (latestScene == typeof(PlaySceneController).Name)
            {
                this.SendCommand<ExitPlaySceneCommand>();
            }
            else if(latestScene == typeof(InitSceneController).Name)
            {
                Debug.Log($"最新场景为 {latestScene}，无法退出");
            }
            else if(latestScene == typeof(UISceneController).Name)
            {
                Debug.Log($"最新场景为 {latestScene}，无法退出");
            }
        }
    }



    /* ------------------------------------------------------------------------- PlayScene 场景 ---------------------------------------------------------------------------- */

    // 进入 PlayScene 场景
    public class EnterPlaySceneCommand : AbstractCommand
    {
        protected override async void OnExecute()
        {
            // 开始加载 PlayScene（UISceneController监听，打开 Loading界面）
            var payload = await this.GetSystem<ISceneSystem>().EnterScencePayLoadAsync<PlaySceneController>(false);
            // 通知：开始加载 PlayScene
            this.SendEvent(new LoadSceneEvent<PlaySceneController>(payload));

            // 等待加载中
            await payload.handle.Task;

            // 通知：加载完成，正式进入 PlayScene
            await this.GetSystem<ISceneSystem>().ActivateAsync(payload.sceneController, payload.sceneController.sceneInstance);
            this.SendEvent(new EnterSceneEvent<PlaySceneController>(payload));
        }
    }

    // 退出 PlayScene 场景
    public class ExitPlaySceneCommand : AbstractCommand
    {
        protected override async void OnExecute()
        {
            // 通知：准备退出 PlayScene
            this.SendEvent(new PreExitSceneEvent<PlaySceneController>());

            // 等待退出中
            await this.GetSystem<ISceneSystem>().ExitScenceAsync<PlaySceneController>();

            // 通知：已退出 PlayScene
            this.SendEvent(new ExitSceneEvent<PlaySceneController>());

        }
    }
}
