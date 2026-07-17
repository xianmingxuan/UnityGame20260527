using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UG20260527
{
    // AudioSource组件池
    public class AudioSourcePoolTool
    {
        /// <summary>
        /// 组件 挂载的 对象
        /// </summary>
        public GameObject root;
        /// <summary>
        /// 回收的对象池
        /// </summary>
        private List<AudioSource> pool = new List<AudioSource>();

        public AudioSourcePoolTool(GameObject root)
        {
            this.root = root;
        }

        public AudioSource Get()
        {
            if(pool ==  null) pool = new List<AudioSource>();
            if(pool.Count > 0)
            {
                var source = pool[pool.Count - 1];
                pool.Remove(source);
                source.enabled = true;
                return source;
            }
            else
            {
                return root.AddComponent<AudioSource>();
            }

        }

        public void Recycle(AudioSource audioSource)
        {
            if(audioSource ==  null) return;
            audioSource.Stop();
            audioSource.enabled = false;
            audioSource.volume = 1f;
            audioSource.clip = null;
            audioSource.pitch = 1f;
            audioSource.loop = false;
            audioSource.playOnAwake = true;
            audioSource.outputAudioMixerGroup = null;
            pool.Add(audioSource);
        }

        public void Clear()
        {
            foreach(var item in pool)
            {
                GameObject.Destroy(item);
            }
            pool.Clear();
        }
    }

    public interface IAudioSystem : ISystem
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


        /// <summary>
        /// 开始播放2D音频
        /// </summary>
        /// <param name="audioClip"></param>
        /// <returns></returns>
        public AudioSource Play2D(AudioClip audioClip);
        /// <summary>
        /// 停止播放2D音频，并回收AudioSource
        /// </summary>
        /// <param name="audioSource"></param>
        /// <returns></returns>
        public bool Stop2D(ref AudioSource audioSource);
        /// <summary>
        /// 播放短时2D音效（如UI）
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="complete"></param>
        /// <param name="duration"></param>
        public void Play2DOneShot(AudioClip audioClip, Action complete = null, float duration = -1f);

    }

    public class AudioSystem : AbstractSystem, IAudioSystem
    {
        // 场景中同时存在的 音频监听器（保证同一时间，只有一个Listener被启用）
        private List<AudioListener> _audioListeners = new List<AudioListener>();

        // 2D音频源gameObject载体（用于 统一挂载2DAudioSource）（2d音频源不需要挂载到具体的发声物上，所以统一挂载到指定gameObject，方便管理）
        private GameObject _2DAduioSourceRoot;
        // 2D音频源（正在播放）
        private List<AudioSource> _2dAudioSource = new List<AudioSource>();
        // AudioSource管理工具
        private AudioSourcePoolTool _2dAudioSourcePool;


        protected override void OnInit()
        {
            if(_2DAduioSourceRoot == null) _2DAduioSourceRoot = new GameObject("2DAduioSourceRoot");
            _2dAudioSourcePool = new AudioSourcePoolTool(_2DAduioSourceRoot);
        }

        /* -------------------------------------------------- 2D音频源 管理 -------------------------------------------------- */

        /// <summary>
        /// 开始播放2D音频
        /// </summary>
        /// <param name="audioClip"></param>
        /// <returns></returns>
        public AudioSource Play2D(AudioClip audioClip)
        {
            if (audioClip == null) return null;

            AudioSource source = _2dAudioSourcePool.Get();
            source.clip = audioClip;
            source.Play();

            _2dAudioSource.Add(source);

            return source;
        }

        /// <summary>
        /// 停止播放2D音频，并回收AudioSource
        /// </summary>
        /// <param name="audioSource"></param>
        /// <returns></returns>
        public bool Stop2D(ref AudioSource audioSource)
        {
            if(audioSource == null) return false;
            
            if(_2dAudioSource.Contains(audioSource)) _2dAudioSource.Remove(audioSource);

            audioSource.Stop();
            _2dAudioSourcePool.Recycle(audioSource);
            audioSource = null;

            return true;
        }

        /// <summary>
        /// 播放短时2D音效（如UI）
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="complete"></param>
        /// <param name="duration"></param>
        public async void Play2DOneShot(AudioClip audioClip, Action complete = null, float duration = -1f)
        {
            if(audioClip == null) return;
            AudioSource source = _2dAudioSourcePool.Get();
            source.clip = audioClip;
            source.loop = false;
            source.Play();
            if (duration == -1f)
            {
                await UniTask.WaitUntil(() =>
                {
                    if (source == null) return true;
                    return source.isPlaying == false;
                });
                if (source != null)
                {
                    source.Stop();
                    _2dAudioSourcePool.Recycle(source);
                }
                complete?.Invoke();
            }
            else
            {
                await UniTask.WaitForSeconds(duration);
                if (source != null)
                {
                    source.Stop();
                    _2dAudioSourcePool.Recycle(source);
                }
                complete?.Invoke();
            }
        }


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
                for(int i = _audioListeners.Count - 1; i >= 0; i--)
                {
                    if( _audioListeners[i] == null) 
                    {
                        // 记录 失效的listener
                        removeListener.Add(_audioListeners[i]);
                        continue;
                    }
                    _audioListeners[i].enabled = false;
                }
                // 清除 失效的listener
                foreach(var lis in removeListener) _audioListeners.Remove(lis);
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
            if (_audioListeners == null ||  _audioListeners.Count <= 0) return;
            if(!_audioListeners.Contains(listener)) return;

            // 移除 指定listener
            _audioListeners.Remove(listener);
            // 激活 最新的listener
            if (_audioListeners.Count > 0) _audioListeners[_audioListeners.Count - 1].enabled = true;
        }

    }
}
