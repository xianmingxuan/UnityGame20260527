using QFramework;
using UnityEngine;

namespace UG20260527
{
    // 工具层 封装存储相关的方法
    public class StorageUtility : IUtility 
    {
        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public int GetInt(string key, int defaultVale)
        {
            return PlayerPrefs.GetInt(key, defaultVale);
        }
    }
}
