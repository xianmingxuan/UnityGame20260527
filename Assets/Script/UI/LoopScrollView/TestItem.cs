using Cysharp.Threading.Tasks;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UG20260527
{
    public class TestItemData
    {
        public bool select = false;
    }

    public class TestItem : LoopScrollItemBase
    {
        private RectTransform rectTransform;

        private Text text;
        private Image img;
        private Color color;


        private void Reset()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (rectTransform.pivot != Vector2.up)
            {
                rectTransform.pivot = Vector2.up;
                Debug.LogWarning($"LoopScrollItem 的 rectTransform.pivot 必须为{Vector2.up}");
            }
            if(rectTransform.anchorMin != Vector2.up)
            {
                rectTransform.anchorMin = Vector2.up;
                Debug.LogWarning($"LoopScrollItem 的 rectTransform.anchorMin 必须为{Vector2.up}");
            }
            if (rectTransform.anchorMax != Vector2.up)
            {
                rectTransform.anchorMax = Vector2.up;
                Debug.LogWarning($"LoopScrollItem 的 rectTransform.anchorMax 必须为{Vector2.up}");
            }
        }

        public override UniTask OnInit<T>(Action<T> onInit = null)
        {
            text = GetComponentInChildren<Text>("Text");
            img = GetComponentInChildren<Image>("Image");
            color = img.color;

            return base.OnInit(onInit);
        }

        public override void UpdataItem(object data, int dataIndex)
        {
            base.UpdataItem(data, dataIndex);

            TestItemData itemData = data as TestItemData;
            text.text = dataIndex.ToString();
            if (itemData.select) img.color = Color.red;
            else img.color = color;
        }
    }
}

