using QFramework;
using System.Threading.Tasks;

namespace UG20260527
{
    // 事件：开始加载 PlayScene
    public struct LoadPlaySceneEvent<T> where T : SceneControllerBase
    {
        public LoadSceneControllerPayLoad<T> payload;

        public LoadPlaySceneEvent(LoadSceneControllerPayLoad<T> payload) { this.payload = payload; }
    }

    // 事件：加载完成，进入 PlayScene
    public struct EnterPlaySceneEvent<T> where T : SceneControllerBase
    {
        public LoadSceneControllerPayLoad<T> payload;

        public EnterPlaySceneEvent(LoadSceneControllerPayLoad<T> payload) { this.payload = payload; }
    }

    // 命令：进入 PlayScene
    public class EnterPlaySceneCommand : AbstractCommand
    {
        // 执行命令
        protected override async void OnExecute()
        {
            // 开始加载 PlayScene（UISceneController监听，打开 Loading界面）
            var payload = await this.GetSystem<ISceneSystem>().EnterScencePayLoadAsync<PlaySceneController>(false);
            // 通知：开始加载 PlayScene
            this.SendEvent(new LoadPlaySceneEvent<PlaySceneController>(payload));

            // 等待加载中
            await payload.handle.Task;

            // 通知：加载完成，正式进入 PlayScene
            payload.sceneController.sceneInstance.ActivateAsync();
            this.SendEvent(new EnterPlaySceneEvent<PlaySceneController>(payload));
        }
    }
}
