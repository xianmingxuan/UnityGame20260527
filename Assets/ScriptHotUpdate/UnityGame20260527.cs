using QFramework;

namespace UG20260527
{
    public class UnityGame20260527 : Architecture<UnityGame20260527>
    {
        protected override void Init()
        {
            // 系统层
            RegisterSystem<IScoreSystem>(new ScoreSystem());
            RegisterSystem<ICameraSystem>(new CameraSystem());
            RegisterSystem<IInputSystem>(new InputSystem());
            RegisterSystem<IUISystem>(new UISystem());
            RegisterSystem<IResourceSystem>(new ResourceSystem());
            RegisterSystem<ISceneSystem>(new SceneSystem());

            // 数据层
            RegisterModel<IScoreModel>(new ScoreModel());
            RegisterModel<ISceneModel>(new SceneModel());
            RegisterModel<ITrafficGameModel>(new TrafficGameModel());

            // 工具层
            RegisterUtility<IStorageUtility>(new StorageUtility());
            RegisterUtility<IPoolUtility>(new PoolUtility());

        }
    }
}