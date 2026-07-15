using Cysharp.Threading.Tasks;
using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UG20260527
{

    /// <summary>
    /// 游戏存档面板
    /// </summary>
    public class SaveGamePanel : PanelBase
    {
        // 游戏存档系统
        private ISaveGameSystem _saveGameSystem;

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

            // 存档系统
            _saveGameSystem = this.GetSystem<ISaveGameSystem>();
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
                    new SaveGameItemData("000", 0),
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
                string saveGameName;
                if (_saveGameSystem.CreateNewSaveGame(new TrafficGameSaveData(2), out saveGameName) == false) return;
                // 更新Item数据列表
                UpdataItemDatas();
                // 回收 无限滚动列表
                scroll.Recycle();
                // 重新初始化 无限滚动列表
                itemCacheList = await scroll.InitLoopScrollView<SaveGameItem, SaveGameItemData>(item, itemDatas);

            });
            // 删除存档
            Btn_DeleteSaveGame?.onClick.AddListener(async () =>
            {
                if(_saveGameSystem.DeleteSaveGame(_saveGameModel.selectSaveGameKey.Value))
                {
                    // 更新Item数据列表
                    UpdataItemDatas();
                    // 回收 无限滚动列表
                    scroll.Recycle();
                    // 重新初始化 无限滚动列表
                    itemCacheList = await scroll.InitLoopScrollView<SaveGameItem, SaveGameItemData>(item, itemDatas);
                }
                else
                {
                    Debug.Log("删除存档失败，选择要删除的存档");
                }
            });
            // 进入存档
            Btn_EnterSaveGame?.onClick.AddListener(() => 
            {
                // 取出选中的存档数据
                if(!_saveGameModel.saveGames.Value.ContainsKey(_saveGameModel.selectSaveGameKey.Value))
                {
                    Debug.Log("进入存档失败，要进入的存档不存在");
                    return;
                }
                string jsonString = _saveGameModel.saveGames.Value[_saveGameModel.selectSaveGameKey.Value];
                TrafficGameSaveData data = this.GetUtility<IPersistenceUtility>().FromJson(jsonString, typeof(TrafficGameSaveData)) as TrafficGameSaveData;
                this.SendCommand(new EnterTrafficSceneCommand(data));

                // 关闭面板
                this.GetSystem<IUISystem>().CloseSinglePanel(PanelLayer.NormalLayer);
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
            // 序列化
            _saveGameSystem.SerializeAll();
        }

        private void UpdataItemDatas()
        {
            // 获取 SaveGame数据
            List<string> keys = _saveGameModel.saveGames.Value.Keys.ToList();
            keys.Sort();
            keys.Reverse();

            // 更新item数据列表
            itemDatas.Clear();
            for (int i = 0; i < keys.Count; i++)
            {
                string jsonString = _saveGameModel.saveGames.Value[keys[i]];
                TrafficGameSaveData data = this.GetUtility<IPersistenceUtility>().FromJson(jsonString, typeof(TrafficGameSaveData)) as TrafficGameSaveData;
                itemDatas.Add(new SaveGameItemData(keys[i], data.levelIndex));
            }
        }

    }
}
