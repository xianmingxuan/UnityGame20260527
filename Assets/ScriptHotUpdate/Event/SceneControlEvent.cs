
using System;

namespace UG20260527
{
    // OnInit Scene
    public struct LoadSceneEvent
    {
        public LoadSceneControllerPayLoad payload;

        public LoadSceneEvent(LoadSceneControllerPayLoad payload) { this.payload = payload; }
    }

    // PreEnter Scene
    public struct PreEnterSceneEvent
    {
        public LoadSceneControllerPayLoad payload;

        public PreEnterSceneEvent(LoadSceneControllerPayLoad payload) { this.payload = payload; }
    }
    
    // Enter Scene
    public struct EnterSceneEvent
    {
        public LoadSceneControllerPayLoad payload;

        public EnterSceneEvent(LoadSceneControllerPayLoad payload) { this.payload = payload; }
    }

    // PreExit Scene
    public struct PreExitSceneEvent
    {
        public Type exitSceneControllerType;

        public PreExitSceneEvent(Type exitSceneControllerType)
        {
            this.exitSceneControllerType = exitSceneControllerType;
        }
    }

    // Exit Scene
    public struct ExitSceneEvent
    {
        public Type exitSceneControllerType;

        public ExitSceneEvent(Type exitSceneControllerType)
        {
            this.exitSceneControllerType = exitSceneControllerType;
        }
    }
}
