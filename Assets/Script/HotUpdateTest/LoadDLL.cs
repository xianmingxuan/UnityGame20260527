using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UG20260527
{
    /// <summary>
    /// 加载 热更新程序集
    /// </summary>
    public class LoadDLL : MonoBehaviour
    {
        void Start()
        {
#if UNITY_EDITOR
            // 编辑器状态下，无需下载，直接获取 热更新程序集 即可
            Assembly hotUpdateAss = null;
            hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First( a => a.GetName().Name == "HotUpdate" );
#else
            // 非编辑器下（游戏打包运行时），需要到 流送资源目录 加载 热更新程序集
            Assembly hotUpdateAss = null;
            hotUpdateAss = Assembly.Load(File.ReadAllBytes($"{Application.streamingAssetsPath}/HotUpdate.dll.bytes"));
#endif

            // 反射 获取程序集中 Hello脚本，运行 静态函数Run
            if ( hotUpdateAss == null )
            {
                Debug.Log("加载 HotUpdate程序集 为空");
                return;
            }
            Type type = hotUpdateAss.GetType("Hello");
            type.GetMethod("Run").Invoke(null, null);
        }


    }
}