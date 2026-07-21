using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        


        /// <summary>
        /// 开始播放2D音频
        /// </summary>
        /// <param name="audioClip"></param>
        /// <returns></returns>
        public UniTask<bool> Play2D(string audioClipPath);
        /// <summary>
        /// 停止播放2D音频，并回收AudioSource
        /// </summary>
        /// <param name="audioSource"></param>
        /// <returns></returns>
        public bool Stop2D(string audioClipPath);
        /// <summary>
        /// 播放短时2D音效（如UI）
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="complete"></param>
        /// <param name="duration"></param>
        public UniTask<bool> PlayOneShot2D(string audioClipPath, float duration = -1f);

    }

    public class AudioSystem : AbstractSystem, IAudioSystem
    {
        // 2D音频源gameObject载体（用于 统一挂载2DAudioSource）（2d音频源不需要挂载到具体的发声物上，所以统一挂载到指定gameObject，方便管理）
        private GameObject _2DAduioSourceRoot;
        // AudioSource管理工具
        private AudioSourcePoolTool _2dAudioSourcePool;

        // 字典 addressableKey -> 音频源组件（脚本）
        private Dictionary<string, List<AudioSource>> _audioSourceDict = new Dictionary<string, List<AudioSource>>();
        // 字典
        private Dictionary<string, AudioClip> _audioClipDict = new Dictionary<string, AudioClip>();



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
        public async UniTask<bool> Play2D(string audioClipPath)
        {
            if (String.IsNullOrEmpty(audioClipPath)) return false;

            // 获取 音频片段
            AudioClip audioClip = null;
            if (_audioClipDict.ContainsKey(audioClipPath)) audioClip = _audioClipDict[audioClipPath];
            else audioClip = await Addressables.LoadAssetAsync<AudioClip>(audioClipPath);
            if(audioClip == null)
            {
                Debug.LogWarning($"获取不到AudioClip {audioClipPath}");
                return false;
            }

            // 获取 2D音频源
            AudioSource source = _2dAudioSourcePool.Get();
            source.clip = audioClip;
            source.Play();

            // 记录 音频片段字典
            _audioClipDict.Add(audioClipPath, audioClip);
            // 记录 音频源字典
            if (!_audioSourceDict.ContainsKey(audioClipPath)) _audioSourceDict.Add(audioClipPath, new List<AudioSource>());
            _audioSourceDict[audioClipPath].Add(source);

            return source;
        }

        /// <summary>
        /// 停止播放2D音频，并回收AudioSource
        /// </summary>
        /// <param name="audioSource"></param>
        /// <returns></returns>
        public bool Stop2D(string audioClipPath)
        {
            if(String.IsNullOrEmpty(audioClipPath)) return false;

            // 获取 音频片段
            if (!_audioClipDict.ContainsKey(audioClipPath))
            {
                Debug.LogWarning($"音频片段 {audioClipPath} 不受音频管理器控制");
                return false;
            }
            AudioClip audioClip = _audioClipDict[audioClipPath];

            // 获取 字典音频源
            if (!_audioSourceDict.ContainsKey(audioClipPath) || _audioSourceDict[audioClipPath].Count <= 0)
            {
                Debug.LogWarning($"音频源 {audioClipPath} 不受音频管理器控制");
                // 移除 音频片段字典
                _audioClipDict.Remove(audioClipPath);
                return false;
            }
            AudioSource audioSource = _audioSourceDict[audioClipPath][0];

            // 回收 2D音频源
            audioSource.Stop();
            _2dAudioSourcePool.Recycle(audioSource);

            // 移除 音频源字典
            _audioSourceDict[audioClipPath].RemoveAt(0);
            // 移除 音频片段字典
            if(_audioSourceDict[audioClipPath].Count <= 0) _audioClipDict.Remove(audioClipPath);

            return true;
        }

        /// <summary>
        /// 播放短时2D音效（如UI）
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="complete"></param>
        /// <param name="duration"></param>
        public async UniTask<bool> PlayOneShot2D(string audioClipPath, float duration = -1f)
        {
            if(String.IsNullOrEmpty(audioClipPath)) return false;

            // 获取 音频片段
            AudioClip audioClip = null;
            if (_audioClipDict.ContainsKey(audioClipPath)) audioClip = _audioClipDict[audioClipPath];
            else audioClip = await Addressables.LoadAssetAsync<AudioClip>(audioClipPath);
            if (audioClip == null)
            {
                Debug.LogWarning($"获取不到AudioClip {audioClipPath}");
                return false;
            }

            // 获取 2D音频源
            AudioSource source = _2dAudioSourcePool.Get();
            source.clip = audioClip;
            source.loop = false;
            source.Play();

            // 等待返回
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
                    return true;
                }
            }
            else
            {
                await UniTask.WaitForSeconds(duration);
                if (source != null)
                {
                    source.Stop();
                    _2dAudioSourcePool.Recycle(source);
                    return true;
                }
            }
            return false;
        }

    }
}
