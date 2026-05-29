using System.Collections;
using UnityEngine;
using QFramework;

namespace UG20260527
{
    // 命令 负责：修改数据层，事件广播
    public class AddCountCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            // 缓存 数据层
            var counterModel = this.GetModel<CounterModel>();

            // 修改数据层
            counterModel.Count++;
            // 事件广播
            this.SendEvent<CountChangedEvent>();
        }
    }
    
    public class SubCountCommand : AbstractCommand
    {
        protected override void OnExecute() 
        {
            // 减少计数
            this.GetModel<CounterModel>().Count--;
            // 事件广播
            this.SendEvent<CountChangedEvent>();
        }
    }
}