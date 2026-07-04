using QFramework;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UG20260527
{
    public class InitTrafficGameModelCommand : AbstractCommand<AsyncOperationHandle<TextAsset>>
    {
        private int _level = 0;
        private ITrafficGameModel _gameModel;

        // 传入数据
        public InitTrafficGameModelCommand(int Level)
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
