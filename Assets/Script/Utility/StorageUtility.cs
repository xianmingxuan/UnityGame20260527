using QFramework;
using UnityEngine;

namespace UG20260527
{
    // 工具层

    public interface IStorageUtility : IUtility
    {
        public void SaveInt(string key, int value);
        public int LoadInt(string key, int defaultValue);
    }

    public class StorageUtility : IStorageUtility
    {
        public void SaveInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public int LoadInt(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }
    }
}