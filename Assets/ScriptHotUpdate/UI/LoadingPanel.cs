using Cysharp.Threading.Tasks;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;


namespace UG20260527
{
    public class LoadingPanelData
    {
        public AsyncOperationHandle loadingHandle;

        public LoadingPanelData(AsyncOperationHandle loadingHandle)
        {
            this.loadingHandle = loadingHandle;
        }
    }

    public class LoadingPanel : PanelBase
    {
        private Scrollbar _scrollbar;
        private Text _text;
        private AsyncOperationHandle _loadingHandle;

        public override UniTask OnInit<T>(Action<T> onInit = null, object userData = null)
        {
            return base.OnInit(onInit, userData);
        }

        public override async void OnOpen()
        {
            base.OnOpen();

            _loadingHandle = (userData as LoadingPanelData).loadingHandle;
            _scrollbar = GetComponentInChildren<Scrollbar>("Scrollbar");
            _text = GetComponentInChildren<Text>("Text");

            await Loading(_loadingHandle);
        }

        public override void OnClose()
        {
            base.OnClose();

        }


        /* -------------------------------------------------- API函数 -------------------------------------------------- */

        // 加载中
        public async UniTask Loading(AsyncOperationHandle loadingHandle)
        {
            // 文本
            _text.text = "正在加载场景中... ...";
            _scrollbar.size = 0;

            // 滚动条
            DownloadStatus downloadState;
            while (!loadingHandle.IsDone)
            {
                // 下载状态
                downloadState = loadingHandle.GetDownloadStatus();

                // 已经下载的资源字节数
                //long downloadBytes = downloadState.DownloadedBytes;
                // 总的资源字节数
                //long totalBytes = downloadState.TotalBytes;

                // 下载百分比
                float percent = downloadState.Percent;

                // 更新滚动条
                _scrollbar.size = percent;

                await UniTask.Yield();
            }
            if (loadingHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _text.text = "加载场景完成，准备进入！";
                _scrollbar.size = 1;
            }
            else
            {
                _text.text = "加载场景失败！";
            }
        }

    }
}
