using QFramework;
using System.Collections;
using UnityEngine;

namespace UG20260527
{
    public class InitSceneController : MonoBehaviour, IController
    {
        // 场景系统
        private ISceneSystem _sceneSystem;

        private void Awake()
        {
            _sceneSystem = this.GetSystem<ISceneSystem>();
        }

        private void Start()
        {
            // 加载UI场景
            _sceneSystem.EnterScencePayLoadAsync<UISceneController>();
        }

        /* -------------------------------------------------- 游戏流程 -------------------------------------------------- */

        // 进入 InitScene

        // 进入 UIScene

        // 进入 游玩Scene

        // 开始游戏




        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}