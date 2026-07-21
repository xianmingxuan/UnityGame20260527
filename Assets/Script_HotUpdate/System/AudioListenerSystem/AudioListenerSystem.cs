using QFramework;
using System.Collections.Generic;
using UnityEngine;


namespace UG20260527
{
    public interface IAudioListenerSystem : ISystem
    {
        /* ------------------- 音频监听器 管理 --------------------- */

        /// <summary>
        /// 入栈 新的listener
        /// </summary>
        /// <param name="listener"></param>
        public void PushAudioListener(AudioListener listener);
        /// <summary>
        /// 出栈 listener
        /// </summary>
        /// <param name="listener"></param>
        public void PopAudioListener(AudioListener listener);
    }

    public class AudioListenerSystem : AbstractSystem, IAudioListenerSystem
    {
        // 场景中同时存在的 音频监听器（保证同一时间，只有一个Listener被启用）
        private List<AudioListener> _audioListeners = new List<AudioListener>();


        protected override void OnInit() { }


        /* -------------------------------------------------- 音频监听器 管理 -------------------------------------------------- */

        /// <summary>
        /// 入栈 新的listener
        /// </summary>
        /// <param name="listener"></param>
        public void PushAudioListener(AudioListener listener)
        {
            if (listener == null) return;
            // 启用 新的listener
            listener.enabled = true;
            // 关闭 旧的listener
            if (_audioListeners != null && _audioListeners.Count > 0)
            {
                List<AudioListener> removeListener = new List<AudioListener>();
                for (int i = _audioListeners.Count - 1; i >= 0; i--)
                {
                    if (_audioListeners[i] == null)
                    {
                        // 记录 失效的listener
                        removeListener.Add(_audioListeners[i]);
                        continue;
                    }
                    _audioListeners[i].enabled = false;
                }
                // 清除 失效的listener
                foreach (var lis in removeListener) _audioListeners.Remove(lis);
            }
            // 入栈 新的listener
            _audioListeners.Add(listener);
        }

        /// <summary>
        /// 出栈 listener
        /// </summary>
        /// <param name="listener"></param>
        public void PopAudioListener(AudioListener listener)
        {
            if (listener == null) return;
            if (_audioListeners == null || _audioListeners.Count <= 0) return;
            if (!_audioListeners.Contains(listener)) return;

            // 移除 指定listener
            _audioListeners.Remove(listener);
            // 激活 最新的listener
            if (_audioListeners.Count > 0) _audioListeners[_audioListeners.Count - 1].enabled = true;
        }
    }
}
