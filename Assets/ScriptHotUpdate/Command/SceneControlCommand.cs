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
            string latestScene = this.GetModel<ISceneModel>().GetLatestSceneController().GetType().Name;
            if(string.IsNullOrEmpty(latestScene))
            {
                Debug.Log("没有正在活跃中的场景");
                return;
            }

            // PlayScene场景
            if(latestScene == typeof(InitSceneController).Name)
            {
                Debug.Log($"最新场景为 {latestScene}，无法退出");
            }
            else if(latestScene == typeof(UISceneController).Name)
            {
                Debug.Log($"最新场景为 {latestScene}，无法退出");
            }
            else if(latestScene == typeof(PlaySceneController).Name)
            {
                this.SendCommand<ExitPlaySceneCommand>();
            }
            else if(latestScene == typeof(TrafficSceneController).Name)
            {
                this.SendCommand<ExitTrafficSceneCommand>();
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
            Debug.Log($"OnInit 场景：{payload.sceneControllerType.Name}");
            this.SendEvent(new LoadSceneEvent(payload));

            // 绑定 通知：场景加载完成，执行PreEnter PlayScene
            payload.onPreEnterComplete += v =>
            {
                Debug.Log($"PreEnter 场景：{payload.sceneControllerType.Name}");
                this.SendEvent(new PreEnterSceneEvent(payload));
            };

            // 绑定 通知：PreEnter完成，执行Enter PlayScene
            payload.onEnterComplete += v =>
            {
                Debug.Log($"Enter 场景：{payload.sceneControllerType.Name}");
                this.SendEvent(new EnterSceneEvent(payload));
            };

            // 等待场景加载 并 激活 中
            await payload.handle.Task;
            await this.GetSystem<ISceneSystem>().ActivateAsync(payload.sceneController, payload.sceneController.sceneInstance);
        }
    }

    // 退出 PlayScene 场景
    public class ExitPlaySceneCommand : AbstractCommand
    {
        protected override async void OnExecute()
        {
            // 通知：准备退出 PlayScene
            Debug.Log($"PreExit 场景：{typeof(PlaySceneController).Name}");
            this.SendEvent(new PreExitSceneEvent(typeof(PlaySceneController)));

            // 等待退出中
            await this.GetSystem<ISceneSystem>().ExitScenceAsync<PlaySceneController>();

            // 通知：已退出 PlayScene
            Debug.Log($"Exit 场景：{typeof(PlaySceneController).Name}");
            this.SendEvent(new ExitSceneEvent(typeof(PlaySceneController)));

        }
    }


    /* ------------------------------------------------------------------------- TrafficScene 场景 ---------------------------------------------------------------------------- */

    // 进入 TrafficScene 场景
    public class EnterTrafficSceneCommand : AbstractCommand
    {
        protected override async void OnExecute()
        {
            var payload = await this.GetSystem<ISceneSystem>().EnterScencePayLoadAsync<TrafficSceneController>();

            Debug.Log($"OnInit 场景：{payload.sceneControllerType.Name}");
            this.SendEvent(new LoadSceneEvent(payload));

            payload.onPreEnterComplete += v =>
            {
                Debug.Log($"PreEnter 场景：{payload.sceneControllerType.Name}");
                this.SendEvent(new PreEnterSceneEvent(payload));
            };

            payload.onEnterComplete += v =>
            {
                Debug.Log($"Enter 场景：{payload.sceneControllerType.Name}");
                this.SendEvent(new EnterSceneEvent(payload));
            };

            await payload.handle.Task;
        }
    }

    // 退出 TrafficScene 场景
    public class ExitTrafficSceneCommand : AbstractCommand
    {
        protected override async void OnExecute()
        {
            // 通知：准备退出 PlayScene
            Debug.Log($"PreExit 场景：{typeof(TrafficSceneController).Name}");
            this.SendEvent(new PreExitSceneEvent(typeof(TrafficSceneController)));

            // 等待退出中
            await this.GetSystem<ISceneSystem>().ExitScenceAsync<TrafficSceneController>();

            // 通知：已退出 PlayScene
            Debug.Log($"Exit 场景：{typeof(TrafficSceneController).Name}");
            this.SendEvent(new ExitSceneEvent(typeof(TrafficSceneController)));
        }
    }

}
