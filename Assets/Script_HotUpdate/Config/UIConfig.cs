using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace UG20260527
{
    /* -------------------------------------------------------------- UI 配置数据 -------------------------------------------------------- */

    [System.Serializable]
    public class PanelConfig
    {
        /// <summary>
        /// 面板层级类型
        /// </summary>
        public PanelLayer panelLayer = PanelLayer.NormalLayer;

        /// <summary>
        /// 面板资源路径
        /// </summary>
        public string panelAssetPath;

        /// <summary>
        /// 脚本类型名
        /// </summary>
        public string panelTypeName;
    }


    [CreateAssetMenu(fileName = "NewUIConfig", menuName = "Config/UIConfig")]
    public class UIConfig : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// 编辑器中使用
        /// </summary>
        [System.Serializable]
        public class Item
        {
            [Tooltip("面板控制脚本")]
            public MonoScript panelScript;

            [Tooltip("面板层级类型")]
            public PanelLayer panelLayer = PanelLayer.NormalLayer;

            [Tooltip("面板资源")]
            public GameObject panelPrefab;

            [Tooltip("脚本类型名（自动生成，不需要手动填写）")]
            public string panelTypeName;
        }

        /// <summary>
        /// 编辑器中：可视化配置的数据
        /// </summary>
        [ReadOnly, Tooltip("UI配置列表，Inspector 中手动配置")]
        public List<Item> panelConfigList = new List<Item>();

#endif

        /// <summary>
        /// 真正序列化保存的数据
        /// </summary>
        [SerializeField, Tooltip("真正序列化的UI配置表，Inspector 中只读，不可手动编辑！！！")]
        private List<PanelConfig> serializePanelConfigList = new List<PanelConfig>();

        /// <summary>
        /// UI配置字典（运行时初始化，UI系统调用）
        /// </summary>
        public Dictionary<string, PanelConfig> panelConfigDic = new Dictionary<string, PanelConfig>();


        /// <summary>
        /// 初始化UI配置字典
        /// </summary>
        public void InitConfig()
        {
            for(int i = 0; i < serializePanelConfigList.Count; i++)
            {
                panelConfigDic.Add(serializePanelConfigList[i].panelTypeName, serializePanelConfigList[i]);
            }
        }




#if UNITY_EDITOR
        // 数据更新时
        private void OnValidate()
        {
            UpdataUIConfig();
        }

        [ContextMenu("刷新UI配置")]
        public void UpdataUIConfig()
        {
            Debug.Log($"{AssetDatabase.GetAssetPath(this)} - 刷新成功！");
            if (serializePanelConfigList != null && serializePanelConfigList.Count > 0) serializePanelConfigList.Clear();
            foreach (var item in panelConfigList)
            {
                if (item.panelScript == null) return;

                // 记录 TypeName
                item.panelTypeName = item.panelScript.GetClass().Name;

                // 更新 序列化数据列表
                PanelConfig config = new PanelConfig();
                config.panelLayer = item.panelLayer;
                config.panelAssetPath = AssetDatabase.GetAssetPath(item.panelPrefab);
                config.panelTypeName = item.panelTypeName;
                serializePanelConfigList.Add(config);

                //Debug.Log(item.panelScript.GetClass().Name);
            }
        }
#endif

    }
}
