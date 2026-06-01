using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UG20260527
{
    /* -------------------------------------------------------------- UI Panel 配置数据 -------------------------------------------------------- */

    [CreateAssetMenu(fileName = "NewPanelConfig", menuName = "Config/PanelConfig")]
    public class PanelConfig : ScriptableObject
    {
        [Tooltip("UIPanel预制体 资源引用列表")]
        public List<AssetReference> panelAssetRefList = new List<AssetReference>();

        /// <summary>
        /// 资源名 - 资源地址
        /// </summary>
        public Dictionary<string, AssetReference> panelAssetRefDic = new Dictionary<string, AssetReference>();


#if UNITY_EDITOR
        // 数据更新时
        private void OnValidate()
        {
            panelAssetRefDic.Clear();
            foreach (var item in panelAssetRefList)
            {

                if (item.editorAsset == null) return;
                panelAssetRefDic.Add(item.editorAsset.name, item);

            }
        }
#endif

    }
}
