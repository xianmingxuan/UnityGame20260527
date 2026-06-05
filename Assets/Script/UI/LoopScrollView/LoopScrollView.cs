using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace UG20260527
{
    /* ---------------------------------------------------------------- 滚动视图Item基类 -------------------------------------------------------- */

    public abstract class LoopScrollItemBase : PanelBase
    {
        /// <summary>
        /// 当前Item 在数据列表中的 索引
        /// </summary>
        public int DataIndex { get; private set; }

        /// <summary>
        /// 滚动更新Item
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        public virtual void UpdataItem(object data, int dataIndex)
        {
            DataIndex = dataIndex;
        }

        /// <summary>
        /// 获取ItemSize
        /// </summary>
        /// <returns></returns>
        public virtual Vector2 GetSize()
        {
            var trans = transform as RectTransform;
            return new Vector2(trans.rect.width * trans.localScale.x, trans.rect.height * trans.localScale.y);
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

        // 滚动类型
        public ScrollType scrollType;
        // 滚动组件
        public ScrollRect scrollRect;
        // 滚动内容容器Transform
        public RectTransform content;
        // 外层视口
        public RectTransform viewPort;

        // Item水平间距
        public float itemHorizontalSpace;
        // Item垂直间距
        public float itemVerticalSpace;


        // 数据列表
        private List<object> _dataList;
        // Item缓存列表
        private List<LoopScrollItemBase> _itemCacheList = new List<LoopScrollItemBase>();
        // Item尺寸，宽高（动态获取）
        private Vector2 _itemSize;
        // item的数量
        private int _ItemCount;
        // 另一个方向上的item数量（水平移动-表示列个数，垂直移动-表示行个数）
        private int _itemCountOfOtherAxis;
        // 当前可视窗口中 排在第一位的Item的DataIndex（垂直移动-表示可视窗口左上角Item的DataIndex）
        private int _curDataIndex;

        // Item初始化委托（外部绑定的自定义Item显示）
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


        // 入口：item预制体，item数据列表
        public async UniTask InitLoopScrollView<T, U>(T itemSC, List<U> dataList) where T : LoopScrollItemBase
        {
            if(itemSC == null)
            {
                return;
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
            int itemCountOfVertical = Mathf.FloorToInt((float)viewPortTrans.rect.height / (_itemSize.y + itemHorizontalSpace));
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
        }

        // 初始化Item
        private async UniTask InitItem<T>() where T : LoopScrollItemBase
        {
            var sys = this.GetSystem<IUISystem>();
            for(int i = 0; i < _ItemCount; i++)
            {
                // 实例化Item
                var itemSC = await sys.OpenSinglePanel<T>(null, false);
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
            item.UpdataItem(_dataList[dataIndex], dataIndex);
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
                    if (dataIndex >= _dataList.Count) return;
                    UpdateItemData(_itemCacheList[i], dataIndex);
                    UpdateItemPosition(_itemCacheList[i], dataIndex);
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
                    if (dataIndex >= _dataList.Count) return;
                    UpdateItemData(_itemCacheList[i], dataIndex);
                    UpdateItemPosition(_itemCacheList[i], dataIndex);
                }
            }
        }


        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }



    
}