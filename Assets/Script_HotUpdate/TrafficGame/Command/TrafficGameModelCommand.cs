using QFramework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UG20260527
{
    /// <summary>
    /// TrafficGameModel的HP 减一
    /// </summary>
    public class TrafficGameModel_HP_MinusOneCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetModel<ITrafficGameModel>().HP.Value--;
        }
    }

    /// <summary>
    /// TrafficGameModel的numberOfRecycledVehicles 加一
    /// </summary>
    public class TrafficGameModel_NumberOfRecycledVehicles_PushOneCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetModel<ITrafficGameModel>().numberOfRecycledVehicles.Value++;
        }
    }

    /// <summary>
    /// 设置TrafficGameModel的12条路线缓存
    /// </summary>
    public class TrafficGameModel_PathsCache_SetCommand : AbstractCommand
    {
        private Dictionary<string, List<Vector3>> paths;

        public TrafficGameModel_PathsCache_SetCommand(Dictionary<string, List<Vector3>> paths)
        {
            this.paths = paths;
        }

        protected override void OnExecute()
        {
            this.GetModel<ITrafficGameModel>().SetPath(paths);
        }
    }

    /// <summary>
    /// 初始化 TrafficGameModel 局内数据（返回Handle，可等待）
    /// </summary>
    public class TrafficGameModel_InitCommand : AbstractCommand<AsyncOperationHandle<TextAsset>>
    {
        private int _level = 0;
        private ITrafficGameModel _gameModel;

        public TrafficGameModel_InitCommand(int Level)
        {
            this._level = Level;
        }

        protected override AsyncOperationHandle<TextAsset> OnExecute()
        {
            _gameModel = this.GetModel<ITrafficGameModel>();

            // 反序列化读表，初始化局内属性
            AsyncOperationHandle<TextAsset> handle = this.GetSystem<IResourceSystem>().LoadAssetHandleAsync<TextAsset>(_gameModel.insideDataPath.Value);
            handle.Completed += h =>
            {
                TextAsset csvFile = h.Result;
                TrafficGame_InsideData data = ParseCSV(csvFile.text, _level);
                _gameModel.SetCSVInsideData(data);

                // Level
                _gameModel.Level.Value = data.Level;
                // HP
                _gameModel.HP.Value = data.HP;
                // 车辆总数
                _gameModel.numberOfVehicles.Value = data.NumberOfVehicles;
                _gameModel.numberOfRecycledVehicles.Value = 0;
            };

            // 返回到外部进行等待
            return handle;
        }

        // 解析CSV数据
        private TrafficGame_InsideData ParseCSV(string csvText, int levelIndex)
        {
            TrafficGame_InsideData tempInsideData = new TrafficGame_InsideData();
            // 按 行 分割数据
            string[] lines = csvText.Split('\n');
            if (levelIndex >= lines.Length - 2)  // 这里 Length - 2，要排除 表头 和 尾部自动生成的空行
            {
                Debug.LogWarning($"TrafficGame 没有 关卡{levelIndex} 对应的局内数据");
                return tempInsideData;
            }
            // 读取 对应关卡行 的数据，用 逗号分割
            string[] header = lines[0].Split(',');
            string[] data = lines[levelIndex + 1].Split(',');
            for (int i = 0; i < header.Length; i++)
            {
                switch (header[i].Trim())
                {
                    case "Level": tempInsideData.Level = int.Parse(data[i].Trim()); break;
                    case "HP": tempInsideData.HP = int.Parse(data[i].Trim()); break;
                    case "NumberOfVehicles": tempInsideData.NumberOfVehicles = int.Parse(data[i].Trim()); break;
                    case "VehicleSpeedRange":
                        string[] range = data[i].Split('~');
                        tempInsideData.VehicleMinSpeed = int.Parse(range[0].Trim());
                        tempInsideData.VehicleMaxSpeed = int.Parse(range[1].Trim());
                        break;
                }
            }

            return tempInsideData;
        }
    }
}
