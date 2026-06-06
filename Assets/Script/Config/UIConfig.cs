using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UG20260527
{
    /* -------------------------------------------------------------- UI 配置数据 -------------------------------------------------------- */

    [System.Serializable]
    public class PanelConfig
    {
        [Tooltip("面板控制脚本")]
        public MonoScript panelScript;

        [Tooltip("面板层级类型")]
        public PanelLayer panelLayer = PanelLayer.NormalLayer;

        [Tooltip("面板资源")]
        public AssetReference panelAssetRef;

        
    }

    [CreateAssetMenu(fileName = "NewUIConfig", menuName = "Config/UIConfig")]
    public class UIConfig : ScriptableObject
    {
        public List<PanelConfig> panelConfigList = new List<PanelConfig>();

        public Dictionary<Type, PanelConfig> panelConfigDic = new Dictionary<Type, PanelConfig>();


#if UNITY_EDITOR
        // 数据更新时
        private void OnValidate()
        {
            panelConfigDic.Clear();
            foreach (var item in panelConfigList)
            {
                if(item.panelScript == null) return;
                panelConfigDic.Add(item.panelScript.GetClass(), item);
                //Debug.Log(item.panelScript.GetClass().Name);
            }
        }
#endif

    }
}
