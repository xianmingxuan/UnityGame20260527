using UnityEngine;
using QFramework;
using System.Collections.Generic;
using System.IO;

namespace UG20260527
{
    public interface ISaveGameSystem : ISystem
    {

    }

    /// <summary>
    /// 存档系统
    /// </summary>
    public class SaveGameSystem : AbstractSystem
    {
        /* -------------------------------------------------- 属性 -------------------------------------------------- */

        // 目录
        private string _directory = Path.Combine(Application.persistentDataPath, "SaveGame");

        // 字典：存档名(带后缀.json) -> jsonString数据
        public Dictionary<string, string> saveGame = new Dictionary<string, string>();


        /* -------------------------------------------------- 生命周期 -------------------------------------------------- */

        protected override void OnInit()
        {
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

        // 反序列化 所有存档
        public void DeserializeAll()
        {
            // 不存在目录 直接返回
            if (!Directory.Exists(_directory)) return;

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
            var persistenceUtility = this.GetUtility<IPersistenceUtility>();
            foreach (string fileName in fileNames)
            {
                saveGame.Add(fileName, persistenceUtility.DeserializeToJsonString(_directory, fileName));
            }
        }

        // 序列化 所有存档
        public void SerializeAll()
        {
            if(saveGame.Count <= 0) return;

            // 序列化
            var persistenceUtility = this.GetUtility<IPersistenceUtility>();
            foreach (var fileName in saveGame.Keys)
            {
                persistenceUtility.SerializeFromJsonString(saveGame[fileName], _directory, fileName);
            }
        }

    }
}
