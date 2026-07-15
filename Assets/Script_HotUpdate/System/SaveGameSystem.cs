using QFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UG20260527
{
    

    public interface ISaveGameSystem : ISystem
    {
        /// <summary>
        /// 创建新存档 返回对应key(存档文件名)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool CreateNewSaveGame(object data, out string key, string saveGameName = "");
        /// <summary>
        /// 删除存档 根据存档名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool DeleteSaveGame(string fileName);


        /// <summary>
        /// 反序列化 所有存档
        /// </summary>
        public void DeserializeAll();
        /// <summary>
        /// 序列化 所有存档
        /// </summary>
        public void SerializeAll();
    }

    /// <summary>
    /// 存档系统
    /// </summary>
    public class SaveGameSystem : AbstractSystem, ISaveGameSystem
    {
        /* -------------------------------------------------- 属性 -------------------------------------------------- */

        // 目录
        private string _directory = Path.Combine(Application.persistentDataPath, "SaveGame");

        // SaveGame数据
        private ISaveGameModel _saveGameModel;

        // 工具
        private IPersistenceUtility _persistenceUtility;



        /* -------------------------------------------------- 生命周期 -------------------------------------------------- */

        protected override void OnInit()
        {
            _saveGameModel = this.GetModel<ISaveGameModel>();
            _persistenceUtility = this.GetUtility<IPersistenceUtility>();

            // 反序列化 所有游戏存档
            DeserializeAll();
        }

        protected override void OnDeinit()
        {
            base.OnDeinit();

            // 序列化 游戏存档
            SerializeAll();
        }


        /* -------------------------------------------------- API -------------------------------------------------- */

        /// <summary>
        /// 反序列化 所有存档
        /// </summary>
        public void DeserializeAll()
        {
            // 不存在目录 直接返回
            if (!Directory.Exists(_directory)) return;

            if(_saveGameModel == null || _persistenceUtility == null)
            {
                _saveGameModel = this.GetModel<ISaveGameModel>();
                _persistenceUtility = this.GetUtility<IPersistenceUtility>();
            }

            // 遍历目录下的所有.json文件
            string[] paths = Directory.GetFiles(_directory, "*.json");
            List<string> fileNames = new List<string>();
            foreach (string path in paths)
            {
                // 带有.json后缀
                fileNames.Add(Path.GetFileName(path));
            }

            // 反序列化 填充字典
            if(fileNames.Count <= 0) return;
            Dictionary<string, string> saveGame = new Dictionary<string, string>();
            foreach (string fileName in fileNames)
            {
                saveGame.Add(fileName, _persistenceUtility.DeserializeToJsonString(_directory, fileName));
            }

            // 更新SaveGameModel数据
            _saveGameModel.saveGames.Value = saveGame;
        }

        /// <summary>
        /// 序列化 所有存档
        /// </summary>
        public void SerializeAll()
        {
            // 获取SaveGame数据
            Dictionary<string, string> saveGame = _saveGameModel.saveGames.Value;
            if (saveGame.Count <= 0) return;

            // 序列化
            var persistenceUtility = this.GetUtility<IPersistenceUtility>();
            foreach (var fileName in saveGame.Keys)
            {
                persistenceUtility.SerializeFromJsonString(saveGame[fileName], _directory, fileName);
            }
        }


        /// <summary>
        /// 创建新存档 返回对应key
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool CreateNewSaveGame(object data, out string key, string saveGameName = "")
        {
            // key 文件名
            string fileName = GetJsonFileNameByTime();
            if (!String.IsNullOrEmpty(saveGameName)) fileName = saveGameName + "_" + fileName;
            // value jsonString
            string jsonString = _persistenceUtility.ToJson(data);

            // 添加 SaveGame数据
            if (_saveGameModel.saveGames.Value.ContainsKey(fileName))
            {
                key = null;
                return false;
            }
            _saveGameModel.saveGames.Value.Add(fileName, jsonString);

            // 序列化
            SerializeAll();

            key = fileName;
            return true;
        }

        /// <summary>
        /// 删除存档 根据存档名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool DeleteSaveGame(string fileName)
        {
            if (fileName == null || !_saveGameModel.saveGames.Value.ContainsKey(fileName)) return false;

            // 移除 对应存档
            _saveGameModel.saveGames.Value.Remove(fileName);

            // 文件路径
            string path = Path.Combine(_directory, fileName);
            // 删除文件
            File.Delete(path);

            return true;
        }

        /// <summary>
        /// 生成Json文件名（.json） 根据 当前时间
        /// </summary>
        /// <returns></returns>
        private string GetJsonFileNameByTime()
        {
            string timeNow = DateTime.Now.ToString();
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string fileName = $"{new string(timeNow.Where(ch => !invalidChars.Contains(ch)).ToArray()).Replace(" ", "")}.json";

            return fileName;
        }

    }
}
