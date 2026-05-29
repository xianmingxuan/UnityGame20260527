using System.Collections;
using UnityEngine;
using QFramework;

namespace UG20260527
{
    // 框架的注册中心
    public class CounterAppArchitecture : Architecture<CounterAppArchitecture>
    {
        // 注册
        protected override void Init()
        {
            // 注册 数据层
            RegisterModel(new CounterModel());

            // 注册 工具层
            RegisterUtility(new StorageUtility());

            // 注册 系统层
            RegisterSystem(new AchievementSystem());
        }
    }
}