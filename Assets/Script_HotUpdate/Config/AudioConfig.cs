using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace UG20260527
{
    [System.Serializable]
    public class AudioData
    {
        [Tooltip("音频片段")]
        public AudioClip audioClip;
        [Tooltip("音频混合通道")]
        public AudioMixerGroup audioMixerGroup;
        [Tooltip("根据该名字进行加载")]
        public string name;
        [Tooltip("音量"), Range(0f, 1f)]
        public float volume = 1f;
        [Tooltip("音调"), Range(-3f,3f)]
        public float pitch = 1f;
        [Tooltip("是否循环")]
        public bool loop;
    }

    [CreateAssetMenu(fileName = "NewAudioConfig", menuName = "Config/AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        [SerializeField]
        public List<AudioData> audioDatas;
    }
}
