
using System;

namespace UG20260527
{
    // OnInitScene 场景控制器OnInit执行完成后通知
    public struct LoadSceneEvent
    {
        public LoadSceneControllerPayLoad payload;

        public LoadSceneEvent(LoadSceneControllerPayLoad payload) { this.payload = payload; }
    }

    // PreEnterScene 场景控制器PreEnter执行完成
    public struct PreEnterSceneEvent
    {
        public LoadSceneControllerPayLoad payload;

        public PreEnterSceneEvent(LoadSceneControllerPayLoad payload) { this.payload = payload; }
    }

    // EnterScene 场景控制器Enter执行完成
    public struct EnterSceneEvent
    {
        public LoadSceneControllerPayLoad payload;

        public EnterSceneEvent(LoadSceneControllerPayLoad payload) { this.payload = payload; }
    }

    // PreExitScene 场景控制器PreExit执行前通知
    public struct PreExitSceneEvent
    {
        public Type exitSceneControllerType;

        public PreExitSceneEvent(Type exitSceneControllerType)
        {
            this.exitSceneControllerType = exitSceneControllerType;
        }
    }

    // ExitScene 场景控制器PreExit执行完成后通知
    public struct ExitSceneEvent
    {
        public Type exitSceneControllerType;

        public ExitSceneEvent(Type exitSceneControllerType)
        {
            this.exitSceneControllerType = exitSceneControllerType;
        }
    }
}
