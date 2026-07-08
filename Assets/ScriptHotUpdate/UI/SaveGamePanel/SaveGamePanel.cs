using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UG20260527
{
    /// <summary>
    /// 游戏存档面板
    /// </summary>
    public class SaveGamePanel : PanelBase
    {
        // UI 绑定
        private Button Btn_CreateSaveGame;
        private Button Btn_DeleteSaveGame;
        private Button Btn_EnterSaveGame;
        private Button Btn_Exit;

        // 存档数据
        private ISaveGameModel _saveGameModel;
        private List<SaveGameItemData> itemDatas = new List<SaveGameItemData>();

        // 无限滚动窗口
        private LoopScrollView scroll;
        // 占位符Item
        SaveGameItem item;
        // 所有子Item引用
        List<LoopScrollItemBase> itemCacheList;


        public override async UniTask OnInit<T>(Action<T> onInit = null, object userData = null)
        {
            await base.OnInit(onInit, userData);

            // 无限滚动视口脚本
            scroll = GetComponentInChildren<LoopScrollView>("Scroll View");
            // UI
            Btn_CreateSaveGame = GetComponentInChildren<Button>("Btn_CreateSaveGame");
            Btn_DeleteSaveGame = GetComponentInChildren<Button>("Btn_DeleteSaveGame");
            Btn_EnterSaveGame = GetComponentInChildren<Button>("Btn_EnterSaveGame");
            Btn_Exit = GetComponentInChildren<Button>("Btn_Exit");
            // 存档数据
            _saveGameModel = this.GetModel<ISaveGameModel>();

            // 创建 占位符Item
            if (item == null)
            {
                item = await this.GetSystem<IUISystem>().OpenSinglePanel<SaveGameItem>
                (
                    null, 
                    new SaveGameItemData(0),
                    new OpenPanelSetting { isPushStack = false, isSetLayerParent = false }
                );
                item.transform.SetParent(scroll.content);
                item.transform.localPosition = Vector3.zero;
                item.gameObject.SetActive(false);
            }

        }

        public override async void OnOpen()
        {
            base.OnOpen();

            // 获取Item数据列表
            UpdataItemDatas();

            // 初始化 无限滚动列表
            itemCacheList = await scroll.InitLoopScrollView<SaveGameItem, SaveGameItemData>(item, itemDatas);



            // 创建存档
            Btn_CreateSaveGame?.onClick.AddListener(async () =>
            {
                // 新建空白存档 并 更新Item数据列表
                _saveGameModel.saveGames.Value.Add(0);
                UpdataItemDatas();
                // 回收 无限滚动列表
                scroll.Recycle();
                // 重新初始化 无限滚动列表
                itemCacheList = await scroll.InitLoopScrollView<SaveGameItem, SaveGameItemData>(item, itemDatas);

            });
            // 删除存档
            Btn_DeleteSaveGame?.onClick.AddListener(() =>
            {

            });
            // 进入存档
            Btn_EnterSaveGame?.onClick.AddListener(() => 
            { 
                
            });
            // 关闭面板
            Btn_Exit?.onClick.AddListener(() =>
            {
                this.GetSystem<IUISystem>().CloseSinglePanel(PanelLayer.NormalLayer);
            });
        }

        public override void OnClose()
        {
            base.OnClose();

            // 回收 无限滚动面板
            scroll.Recycle();
            itemCacheList = null;
            itemDatas.Clear();
            // 解除绑定
            Btn_CreateSaveGame?.onClick.RemoveAllListeners();
            Btn_DeleteSaveGame?.onClick.RemoveAllListeners();
            Btn_EnterSaveGame?.onClick.RemoveAllListeners();
            Btn_Exit?.onClick.RemoveAllListeners();
        }

        private void UpdataItemDatas()
        {
            itemDatas.Clear();
            for (int i = 0; i < _saveGameModel.saveGames.Value.Count; i++)
            {
                int temp = (int)_saveGameModel.saveGames.Value[i];
                itemDatas.Add(new SaveGameItemData(temp));
            }
        }

    }
}
