
namespace UG20260527
{
    // 开始加载 Scene
    public struct LoadSceneEvent<T> where T : SceneControllerBase
    {
        public LoadSceneControllerPayLoad<T> payload;

        public LoadSceneEvent(LoadSceneControllerPayLoad<T> payload) { this.payload = payload; }
    }

    // 加载完成，进入 Scene
    public struct EnterSceneEvent<T> where T : SceneControllerBase
    {
        public LoadSceneControllerPayLoad<T> payload;

        public EnterSceneEvent(LoadSceneControllerPayLoad<T> payload) { this.payload = payload; }
    }

    // 准备 退出场景
    public struct PreExitSceneEvent<T> where T : SceneControllerBase
    {

    }

    // 退出场景
    public struct ExitSceneEvent<T> where T : SceneControllerBase
    {

    }
}
