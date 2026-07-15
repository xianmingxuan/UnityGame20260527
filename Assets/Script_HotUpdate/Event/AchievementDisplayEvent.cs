
namespace UG20260527
{
    // 事件广播 载荷Payload
    public struct AchievementDisplayEvent
    {
        public string DisplayText;

        public AchievementDisplayEvent(string str)
        {
            // 成就展示的内容
            DisplayText = str;
        }
    }
}