using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UG20260527
{
    /* -------------------------------------------------------------- UI 配置数据 -------------------------------------------------------- */

    [System.Serializable]
    public class PanelConfig
    {
#if UNITY_EDITOR
        [Tooltip("面板控制脚本")]
        public MonoScript panelScript;
#endif

        [Tooltip("面板层级类型")]
        public PanelLayer panelLayer = PanelLayer.NormalLayer;

        [Tooltip("面板资源")]
        public AssetReference panelAssetRef;

        [Tooltip("脚本类型名（自动生成，不需要手动填写）")]
        public string panelTypeName;

    }


    [CreateAssetMenu(fileName = "NewUIConfig", menuName = "Config/UIConfig")]
    public class UIConfig : ScriptableObject
    {
        public List<PanelConfig> panelConfigList = new List<PanelConfig>();

        public Dictionary<string, PanelConfig> panelConfigDic = new Dictionary<string, PanelConfig>();


        /// <summary>
        /// 初始化UI配置字典
        /// </summary>
        public void InitConfig()
        {
            for(int i = 0; i < panelConfigList.Count; i++)
            {
                panelConfigDic.Add(panelConfigList[i].panelTypeName, panelConfigList[i]);
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
            Debug.Log("刷新UI配置");
            foreach (var item in panelConfigList)
            {
                if (item.panelScript == null) return;
                item.panelTypeName = item.panelScript.GetClass().Name;

                //Debug.Log(item.panelScript.GetClass().Name);
            }
        }
#endif

    }
}
