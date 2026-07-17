using QFramework;
using UnityEngine;


namespace UG20260527
{
    /// <summary>
    /// 音频Listener管理器（挂载在拥有AudioListener组件的GameObject上，将该组件纳入统一管理）
    /// </summary>
    public class AudioListenerController : MonoBehaviour, IController
    {

        private void OnEnable()
        {
            this.GetSystem<IAudioSystem>().PushAudioListener(gameObject.GetComponent<AudioListener>());
        }

        private void OnDisable()
        {
            this.GetSystem<IAudioSystem>().PopAudioListener(gameObject.GetComponent<AudioListener>());
        }


        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}
