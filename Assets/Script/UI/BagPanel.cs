using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.Progress;

namespace UG20260527
{
    public class BagPanel : PanelBase
    {
        public override async UniTask OnInit<T>(Action<T> onInit = null)
        {
            var scroll = GetComponentInChildren<LoopScrollView>("Scroll View");
            var item = GetComponentInChildren<TestItem>("TestItem");
            if (item == null)
            {
                item = await this.GetSystem<IUISystem>().OpenSinglePanel<TestItem>(null, false);
                item.transform.SetParent(scroll.content);
                item.transform.localPosition = Vector3.zero;
            }

            List<TestItemData> list = new List<TestItemData>();
            for (int i = 0; i < 100; i++)
            {
                TestItemData d = new TestItemData();
                if(i == 20) d.select = true;
                list.Add(d);
            }

            await scroll.InitLoopScrollView<TestItem, TestItemData>(item, list);

            await base.OnInit(onInit);
        }
    }
}
