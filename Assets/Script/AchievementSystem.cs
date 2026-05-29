using QFramework;
using UnityEngine;

namespace UG20260527
{
    // 系统层 成就系统
    public class AchievementSystem : AbstractSystem
    {
        protected override void OnInit()
        {
            var counterModel = this.GetModel<CounterModel>();

            // 监听事件
            this.RegisterEvent<CountChangedEvent>(e =>
            {
                if (counterModel.Count == 10) Debug.Log("射击10次");
                else if (counterModel.Count == 20) Debug.Log("射击20次");
            });
            
        }
    }
}
