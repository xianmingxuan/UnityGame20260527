using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace UG20260527
{
    // ViewController表现层
    public class CounterAppController : MonoBehaviour, IController
    {
        // view
        public Text countText;
        public Button BtnAdd;
        public Button BtnSub;

        // model
        private CounterModel mModel;

        // 返回 注册中心的操作接口
        public IArchitecture GetArchitecture()
        {
            return CounterAppArchitecture.Interface;
        }

        void Start()
        {
            // 获取 数据层
            mModel = this.GetModel<CounterModel>();

            BtnAdd.onClick.AddListener(() => 
            {
                // 交互逻辑（表现层 -> 命令 -> 数据层）
                this.SendCommand<AddCountCommand>();
            });

            BtnSub.onClick.AddListener(() =>
            {
                // 交互逻辑
                this.SendCommand<SubCountCommand>();
            });

            // 表现逻辑
            this.RegisterEvent<CountChangedEvent>(e =>
            {
                // 更新UI
                UpdateView();
            }).UnRegisterWhenGameObjectDestroyed(gameObject);  // 链式调用：当gameObject被销毁时，自动移除事件

            // 表现逻辑
            UpdateView();
        }

        void UpdateView()
        {
            countText.text = mModel.Count.ToString();
        }
    }
}

