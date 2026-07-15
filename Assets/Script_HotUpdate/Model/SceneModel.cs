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
        public SceneControllerBase GetLatestSceneController();

        /// <summary>
        /// 添加活跃中的场景控制器（去重）
        /// </summary>
        /// <param name="sceneController"></param>
        /// <returns></returns>
        public bool AddActiveSceneControllerList(SceneControllerBase sceneController);

        /// <summary>
        /// 移除活跃中的场景控制器
        /// </summary>
        /// <param name="sceneController"></param>
        /// <returns></returns>
        public bool RemoveActiveSceneControllerList(SceneControllerBase sceneController);
    }


    public class SceneModel : AbstractModel, ISceneModel
    {
        // 正在活跃中的场景
        private List<SceneControllerBase> activeSceneControllerList { get; set; } = new List<SceneControllerBase>();

        protected override void OnInit()
        {

        }


        /// <summary>
        /// 获取最新的场景控制器
        /// </summary>
        /// <returns></returns>
        SceneControllerBase ISceneModel.GetLatestSceneController()
        {
            if(activeSceneControllerList == null || activeSceneControllerList.Count <= 0)
            {
                return null;
            }

            return activeSceneControllerList[activeSceneControllerList.Count - 1];
        }

        /// <summary>
        /// 添加 活跃中场景（去重）
        /// </summary>
        /// <param name="sceneController"></param>
        /// <returns></returns>
        bool ISceneModel.AddActiveSceneControllerList(SceneControllerBase sceneController)
        {
            if(!activeSceneControllerList.Contains(sceneController))
            {
                activeSceneControllerList.Add(sceneController);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 移除 活跃中场景
        /// </summary>
        /// <param name="sceneController"></param>
        /// <returns></returns>
        bool ISceneModel.RemoveActiveSceneControllerList(SceneControllerBase sceneController)
        {
            return activeSceneControllerList.Remove(sceneController);
        }
    }
}