using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UG20260527
{
    [System.Serializable]
    public class SceneConfigData
    {
        /// <summary>
        /// 场景控制器类型名
        /// </summary>
        public string sceneControllerTypeName;

        /// <summary>
        /// 场景资源路径
        /// </summary>
        public string sceneAssetPath;
    }

    [CreateAssetMenu(fileName = "NewSceneConfig", menuName = "Config/SceneConfig")]
    public class SceneConfig : ScriptableObject
    {
#if UNITY_EDITOR
        [System.Serializable]
        public class SceneConfigItem
        {
            [Tooltip("场景控制器脚本 资源")]
            public MonoScript sceneController;

            [Tooltip("场景 资源")]
            public SceneAsset sceneAsset;

            [Tooltip("脚本类型名（自动生成，不需要手动填写）")]
            public string sceneControllerTypeName;
        }


        [Tooltip("编辑器中：手动配置 SceneController和Scene 之间的关系")]
        public List<SceneConfigItem> items;

#endif

        /// <summary>
        /// 真正序列化保存的数据
        /// </summary>
        [SerializeField, Tooltip("真正序列化的Scene配置表，Inspector 中只读，不可手动编辑！！！")]
        private List<SceneConfigData> sceneConfigDataList;

        /// <summary>
        /// 场景配置字典（运行时构建）
        /// </summary>
        public Dictionary<string, SceneConfigData> sceneConfigDict = new Dictionary<string, SceneConfigData>();


        /// <summary>
        /// 初始化场景配置字典
        /// </summary>
        public void InitConfig()
        {
            for (int i = 0; i < sceneConfigDataList.Count; i++)
            {
                sceneConfigDict.Add(sceneConfigDataList[i].sceneControllerTypeName, sceneConfigDataList[i]);
            }
        }



#if UNITY_EDITOR
        private void OnValidate()
        {
            Debug.Log($"{AssetDatabase.GetAssetPath(this)} - 刷新成功！");
            if (sceneConfigDataList != null && sceneConfigDataList.Count > 0) sceneConfigDataList.Clear();
            foreach(var item in items)
            {
                if (item.sceneController == null) return;

                // 记录 TypeName
                item.sceneControllerTypeName = item.sceneController.GetClass().Name;

                // 更新 序列化数据列表
                SceneConfigData data = new SceneConfigData();
                data.sceneControllerTypeName = item.sceneControllerTypeName;
                data.sceneAssetPath = AssetDatabase.GetAssetPath(item.sceneAsset);
                sceneConfigDataList.Add(data);

                //Debug.Log(item.sceneController.GetClass().Name);
                //Debug.Log(AssetDatabase.GetAssetPath(item.sceneAsset));
            }
        }
#endif
    }
}