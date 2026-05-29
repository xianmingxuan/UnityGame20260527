using System.Collections;
using UnityEngine;
using QFramework;

namespace UG20260527
{
    // Model数据层
    public class CounterModel : AbstractModel
    {
        // 存储工具
        private StorageUtility mStorage;

        // 计数
        private int mCount = 0;
        public int Count 
        {
            get { return mCount; }
            set
            {
                if(mCount != value)
                {
                    mCount = value;
                    // 数据持久化
                    mStorage.SetInt(nameof(Count), value);
                }
            }
        }

        protected override void OnInit()
        {
            // 缓存 存储工具
            mStorage = this.GetUtility<StorageUtility>();

            // 读取数据
            mCount = mStorage.GetInt(nameof(Count), 0);
        }
    }
}