using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UG20260527
{
    public class BagPanel : PanelBase
    {
        // 滚动窗口
        private LoopScrollView scroll;

        public override async void OnOpen()
        {
            base.OnOpen();

            scroll = GetComponentInChildren<LoopScrollView>("Scroll View");
            var item = GetComponentInChildren<TestItem>("TestItem");
            if (item == null)
            {
                item = await this.GetSystem<IUISystem>().OpenSinglePanel<TestItem>(null, null, new OpenPanelSetting { isPushStack = false });
                item.transform.SetParent(scroll.content);
                item.transform.localPosition = Vector3.zero;
            }

            List<TestItemData> list = new List<TestItemData>();
            for (int i = 0; i < 100; i++)
            {
                TestItemData d = new TestItemData();
                if (i == 20) d.select = true;
                list.Add(d);
            }

            // 创建 无限滚动窗口
            await scroll.InitLoopScrollView<TestItem, TestItemData>(item, list);

            // 回收 占位符Item
            await this.GetSystem<IUISystem>().CloseSinglePanel(item.panelConfig.panelLayer, new ClosePanelSetting { panelShouldClose = item });
        }

        public override void OnClose()
        {
            base.OnClose();

            // 回收 无限滚动窗口
            scroll.Recycle();
        }
    }
}
