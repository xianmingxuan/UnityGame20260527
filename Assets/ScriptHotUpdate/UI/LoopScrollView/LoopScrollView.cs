using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UG20260527
{
    /* ---------------------------------------------------------------- 滚动视图Item基类 -------------------------------------------------------- */

    public abstract class LoopScrollItemBase : PanelBase
    {

#if UNITY_EDITOR

        private RectTransform rectTransform;

        private void Reset()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (rectTransform.pivot != Vector2.up)
            {
                rectTransform.pivot = Vector2.up;
                Debug.LogWarning($"LoopScrollItem 的 rectTransform.pivot 必须为{Vector2.up}");
            }
            if (rectTransform.anchorMin != Vector2.up)
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
        private void OnValidate()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (rectTransform.pivot != Vector2.up)
            {
                Debug.LogWarning($"LoopScrollItem 的 rectTransform.pivot 必须为{Vector2.up}");
            }
            if (rectTransform.anchorMin != Vector2.up)
            {
                Debug.LogWarning($"LoopScrollItem 的 rectTransform.anchorMin 必须为{Vector2.up}");
            }
            if (rectTransform.anchorMax != Vector2.up)
            {
                Debug.LogWarning($"LoopScrollItem 的 rectTransform.anchorMax 必须为{Vector2.up}");
            }
        }
#endif

        /// <summary>
        /// 滚动更新Item
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        public virtual void UpdataItem(List<object> datas, int dataIndex)
        {
            userData = datas[dataIndex];
        }
    }



    /* ---------------------------------------------------------------- 滚动视图 -------------------------------------------------------- */

    public class LoopScrollView : MonoBehaviour, IController
    {
        public enum ScrollType
        {
            Vertical,
            Horizontal
        }

        /// <summary>
        /// 滚动类型
        /// </summary>
        public ScrollType scrollType;
        /// <summary>
        /// 滚动组件
        /// </summary>
        public ScrollRect scrollRect;
        /// <summary>
        /// 滚动内容容器Transform
        /// </summary>
        public RectTransform content;
        /// <summary>
        /// 外层视口
        /// </summary>
        public RectTransform viewPort;

        /// <summary>
        /// Item水平间距
        /// </summary>
        public float itemHorizontalSpace;
        /// <summary>
        /// Item垂直间距
        /// </summary>
        public float itemVerticalSpace;


        /// <summary>
        /// 数据列表
        /// </summary>
        private List<object> _dataList;
        /// <summary>
        /// Item缓存列表
        /// </summary>
        private List<LoopScrollItemBase> _itemCacheList = new List<LoopScrollItemBase>();
        /// <summary>
        /// Item尺寸，宽高（动态获取）
        /// </summary>
        private Vector2 _itemSize;
        /// <summary>
        /// item的数量
        /// </summary>
        private int _ItemCount;
        /// <summary>
        /// 另一个方向上的item数量（水平移动-表示列个数，垂直移动-表示行个数）
        /// </summary>
        private int _itemCountOfOtherAxis;
        /// <summary>
        /// 当前可视窗口中 排在第一位的Item的DataIndex（垂直移动-表示可视窗口左上角Item的DataIndex）
        /// </summary>
        private int _curDataIndex;

        /// <summary>
        /// Item初始化委托（外部绑定的自定义Item显示）
        /// </summary>
        private Action<LoopScrollItemBase, object, int> _onItemInit;


        private void Awake()
        {
            // 自动初始化 引用的组件
            if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
            if (content == null) content = scrollRect.content;
            if (viewPort == null) viewPort = scrollRect.viewport;

            // 判断水平/垂直（代码逻辑互斥）
            scrollRect.horizontal = scrollType == ScrollType.Horizontal;
            scrollRect.vertical = scrollType == ScrollType.Vertical;

            // 记录
            _curDataIndex = 0;
        }

        // 出口（回收）：回收所有的Item实例
        public void Recycle()
        {
            // 回收Item，清空Item缓存列表
            var sys = this.GetSystem<IUISystem>();
            foreach (var item in _itemCacheList)
            {
                sys.CloseSinglePanel(item.panelConfig.panelLayer, new ClosePanelSetting { panelShouldClose = item});
            }
            _itemCacheList.Clear();

            // 重置 当前数据索引
            _curDataIndex = 0;
        }

        // 入口：item脚本，item数据列表
        public async UniTask<List<LoopScrollItemBase>> InitLoopScrollView<T, U>(T itemSC, List<U> dataList) where T : LoopScrollItemBase
        {
            var sys = this.GetSystem<IUISystem>();
            if (itemSC == null)
            {
                return null;
            }

            // 数据列表
            _dataList = new List<object>();
            foreach (var v in dataList) _dataList.Add(v);

            // 计算 Item尺寸
            var trans = itemSC.gameObject.GetComponent<Transform>() as RectTransform;
            _itemSize = new Vector2(trans.rect.width * trans.localScale.x, trans.rect.height * trans.localScale.y);
            itemSC.gameObject.SetActive(false);

            // 计算 Item数量，另一方向上的item数量，Content锚点，尺寸
            var viewPortTrans = viewPort.GetComponent<RectTransform>();  // 可见范围
            int itemCountOfHorizontal = Mathf.FloorToInt((float)viewPortTrans.rect.width / (_itemSize.x + itemHorizontalSpace));
            if (itemCountOfHorizontal == 0) itemCountOfHorizontal = 1;
            int itemCountOfVertical = Mathf.FloorToInt((float)viewPortTrans.rect.height / (_itemSize.y + itemHorizontalSpace));
            if (itemCountOfVertical == 0) itemCountOfVertical = 1;
            // 设置 Content旋转，缩放支点 到左上角
            content.pivot = new Vector2(0, 1);
            switch (scrollType)
            {
                case ScrollType.Horizontal:
                    // 另一方向上的item数量
                    _itemCountOfOtherAxis = itemCountOfVertical;
                    // Stretch_Left 上下拉伸，左
                    content.anchorMin = new Vector2(0, 0);
                    content.anchorMax = new Vector2(0, 1);
                    // Content宽度
                    content.sizeDelta = new Vector2(Mathf.CeilToInt((float)_dataList.Count / (float)_itemCountOfOtherAxis) * (_itemSize.x + itemHorizontalSpace) - itemHorizontalSpace, 0);
                    break;
                case ScrollType.Vertical:
                    // 另一方向上的item数量
                    _itemCountOfOtherAxis = itemCountOfHorizontal;
                    // Top_Stretch 上，左右拉伸
                    content.anchorMin = new Vector2(0, 1);
                    content.anchorMax = new Vector2(1, 1);
                    // Content高度
                    content.sizeDelta = new Vector2(0, Mathf.CeilToInt((float)_dataList.Count / (float)_itemCountOfOtherAxis) * (_itemSize.y + itemVerticalSpace) - itemVerticalSpace);
                    break;
            }
            // item数量
            _ItemCount = (itemCountOfHorizontal * itemCountOfVertical) + (2 * _itemCountOfOtherAxis);

            // 创建所有item实例
            await InitItem<T>();

            scrollRect.onValueChanged.AddListener(OnValueChanged);

            // 先更新一次
            OnValueChanged(content.position);

            return _itemCacheList;
        }

        // 初始化Item
        private async UniTask InitItem<T>() where T : LoopScrollItemBase
        {
            var sys = this.GetSystem<IUISystem>();
            for(int i = 0; i < _ItemCount; i++)
            {
                // 实例化Item
                var itemSC = await sys.OpenSinglePanel<T>(null, null, new OpenPanelSetting { isPushStack = false });
                itemSC.gameObject.name = $"item_{i}";
                itemSC.transform.SetParent(content);
                // 更新数据
                UpdateItemData(itemSC, i);
                // 更新位置
                UpdateItemPosition(itemSC, i);
                // 入缓存
                _itemCacheList.Add(itemSC);
            }
        }

        private void UpdateItemData(LoopScrollItemBase item, int dataIndex)
        {
            if (dataIndex >= _dataList.Count)  // 数据量 <= 显示窗口Item的数量
            {
                // 将多出的item隐藏
                item.gameObject.SetActive(false);
            }
            else  // 数据量 > 显示窗口Item的数量
            {
                // 显示所有item，更新数据
                item.gameObject.SetActive(true);
                item.UpdataItem(_dataList, dataIndex);
            }
        }

        private void UpdateItemPosition(LoopScrollItemBase item, int dataIndex)
        {
            var trans = item.transform as RectTransform;
            trans.localPosition = Vector3.zero;
            trans.anchoredPosition = GetLocalPositionByDataIndex(dataIndex);
        }

        private Vector3 GetLocalPositionByDataIndex(int dataIndex)
        {
            float x, y, z;
            x = y = z = 0;

            int remain = dataIndex % _itemCountOfOtherAxis;
            dataIndex /= _itemCountOfOtherAxis;
            switch (scrollType)
            {
                case ScrollType.Horizontal:
                    x = dataIndex * (_itemSize.x + itemHorizontalSpace);
                    y = -remain * (_itemSize.y + itemVerticalSpace);
                    break;
                case ScrollType.Vertical:
                    x = remain * (_itemSize.x + itemHorizontalSpace);
                    y = -dataIndex * (_itemSize.y + itemVerticalSpace);
                    break;
            }

            return new Vector3(x, y, z);
        }


        private void OnValueChanged(Vector2 value)
        {
            if(scrollType == ScrollType.Horizontal)
            {
                int curLeftDataIndex = Mathf.FloorToInt(Mathf.Abs(content.anchoredPosition.x) / (_itemSize.x + itemHorizontalSpace)) * _itemCountOfOtherAxis;
                if (curLeftDataIndex == _curDataIndex) return;
                _curDataIndex = curLeftDataIndex;
                for(int i = 0; i < _itemCacheList.Count; i++)
                {
                    var dataIndex = _curDataIndex + i;
                    if (dataIndex < _dataList.Count)
                    {
                        UpdateItemData(_itemCacheList[i], dataIndex);
                        UpdateItemPosition(_itemCacheList[i], dataIndex);
                    }
                    else
                    {
                        UpdateItemData(_itemCacheList[i], dataIndex);
                    }
                }
            }
            else
            {
                int curTopDataIndex = Mathf.FloorToInt(Mathf.Abs(content.anchoredPosition.y) / (_itemSize.y + itemVerticalSpace)) * _itemCountOfOtherAxis;
                if (curTopDataIndex == _curDataIndex) return;
                _curDataIndex = curTopDataIndex; 
                for (int i = 0; i < _itemCacheList.Count; i++)
                {
                    int dataIndex = _curDataIndex + i;
                    if (dataIndex < _dataList.Count)
                    {
                        UpdateItemData(_itemCacheList[i], dataIndex);
                        UpdateItemPosition(_itemCacheList[i], dataIndex);
                    }
                    else
                    {
                        // 当划到最末尾时，最后2个ItemObj没有对应的ItemData，
                        // 这时，这2个ItemObj在更新数据时，会自动失活隐藏
                        // 这时，没必要更新位置，减少计算量
                        UpdateItemData(_itemCacheList[i], dataIndex);
                    }
                }
            }
        }


        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }



    
}