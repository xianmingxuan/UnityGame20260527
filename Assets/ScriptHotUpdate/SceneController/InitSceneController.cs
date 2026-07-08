using QFramework;
using UnityEngine;

namespace UG20260527
{
    public class InitSceneController : MonoBehaviour, IController
    {
        private ISceneSystem _sceneSystem; //场景系统
        private ISaveGameSystem _saveGameSystem;  // 存档系统

        private void Awake()
        {
            // 初始化引用 触发Init
            _sceneSystem = this.GetSystem<ISceneSystem>();
            _saveGameSystem = this.GetSystem<ISaveGameSystem>();
            _saveGameSystem.DeserializeAll();
        }

        private void Start()
        {
            // 加载UI场景
            _sceneSystem.EnterScencePayLoadAsync<UISceneController>();
        }




        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}