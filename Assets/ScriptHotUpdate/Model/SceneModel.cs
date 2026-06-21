using QFramework;
using System.Collections.Generic;

namespace UG20260527
{

    public interface ISceneModel : IModel
    {
        /// <summary>
        /// 获取最新的场景控制器
        /// </summary>
        /// <returns></returns>
        public string GetLatestSceneController();

        /// <summary>
        /// 添加活跃中的场景控制器（去重）
        /// </summary>
        /// <param name="sceneController"></param>
        /// <returns></returns>
        public bool AddActiveSceneControllerList(string sceneController);

        /// <summary>
        /// 移除活跃中的场景控制器
        /// </summary>
        /// <param name="sceneController"></param>
        /// <returns></returns>
        public bool RemoveActiveSceneControllerList(string sceneController);
    }


    public class SceneModel : AbstractModel, ISceneModel
    {
        // 正在活跃中的场景
        private List<string> activeSceneControllerList { get; set; } = new List<string>();

        protected override void OnInit()
        {

        }


        /// <summary>
        /// 获取最新的场景控制器
        /// </summary>
        /// <returns></returns>
        string ISceneModel.GetLatestSceneController()
        {
            if(activeSceneControllerList == null || activeSceneControllerList.Count <= 0)
            {
                return null;
            }

            return activeSceneControllerList[activeSceneControllerList.Count - 1];
        }

        // 添加 活跃中场景（去重）
        bool ISceneModel.AddActiveSceneControllerList(string sceneController)
        {
            if(!activeSceneControllerList.Contains(sceneController))
            {
                activeSceneControllerList.Add(sceneController);
                return true;
            }
            return false;
        }

        // 移除 活跃中场景
        bool ISceneModel.RemoveActiveSceneControllerList(string sceneController)
        {
            return activeSceneControllerList.Remove(sceneController);
        }
    }
}