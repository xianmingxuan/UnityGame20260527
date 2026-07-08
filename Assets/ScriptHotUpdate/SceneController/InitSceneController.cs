using QFramework;
using UnityEngine;

namespace UG20260527
{
    public class InitSceneController : MonoBehaviour, IController
    {
        private ISceneSystem _sceneSystem; //场景系统

        private void Awake()
        {
            // 初始化引用
            _sceneSystem = this.GetSystem<ISceneSystem>();
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