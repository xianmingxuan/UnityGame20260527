using System;
using System.Collections;
using UnityEngine;

namespace UG20260527
{
    // Mono单例基类
    public class MonoSingle<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance 
        {
            get 
            {
                if(instance == null)
                {
                    var obj = new GameObject(typeof(T).Name);
                    instance = obj.AddComponent<T>();
                    GameObject.DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }

        
    }

    // 公共Mono类
    public class PublicMono : MonoSingle<PublicMono>
    {
        public event Action OnFixedUpdate;
        public event Action OnUpdate;
        public event Action OnLateUpdate;

        private void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }
    }
}