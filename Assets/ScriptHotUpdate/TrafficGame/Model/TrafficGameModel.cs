using Cysharp.Threading.Tasks;
using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace UG20260527
{
    // CSV数据
    public struct TrafficGame_InsideData
    {
        // Level
        public int Level;

        // HP
        public int HP;

        // NumberOfVehicles
        public int NumberOfVehicles;

        // VehicleSpeedRange
        public int VehicleMinSpeed;
        public int VehicleMaxSpeed;
    }

    public interface ITrafficGameModel : IModel
    {
        /* ---------------------------------------------- 数据 --------------------------------------------- */

        /// <summary>
        /// 当前所在关卡
        /// </summary>
        public BindableProperty<int> Level { get; set; }
        /// <summary>
        /// 当前血量
        /// </summary>
        public BindableProperty<int> HP { get; set; }
        /// <summary>
        /// 本局总车数
        /// </summary>
        public BindableProperty<int> numberOfVehicles { get; set; }
        /// <summary>
        /// 以回收的车数
        /// </summary>
        public BindableProperty<int> numberOfRecycledVehicles { get; set; }


        /* ---------------------------------------------- 函数 --------------------------------------------- */

        /// <summary>
        /// 初始化 Model局内数据
        /// </summary>
        public UniTask InitModel(int level);

        /// <summary>
        /// CSV局内数据
        /// </summary>
        /// <returns></returns>
        public TrafficGame_InsideData GetInsideData();

        // 12条路线缓存
        public string GetKey(PathsPresetInfo.DirectionType directionType, PathsPresetInfo.MovementType movementType);
        public void SetPath(Dictionary<string, List<Vector3>> paths);
        public List<Vector3> GetPath(PathsPresetInfo.DirectionType directionType, PathsPresetInfo.MovementType movementType);

    }

    public class TrafficGameModel : AbstractModel, ITrafficGameModel
    {

        /* ---------------------------------------------- 局内数据 --------------------------------------------- */

        // 局内数据（读表）
        private string _insideDataPath = "Assets/AddressablesAsset/Config/CSV/TrafficGame_InsideData.CSV";
        public TrafficGame_InsideData _insideData {  get; private set; }

        // Level
        public BindableProperty<int> Level { get; set; } = new BindableProperty<int>();
        // HP
        public BindableProperty<int> HP { get; set; } = new BindableProperty<int>();
        // 总车数
        public BindableProperty<int> numberOfVehicles { get; set; } = new BindableProperty<int>();
        // 已经销毁的车数
        public BindableProperty<int> numberOfRecycledVehicles { get; set; } = new BindableProperty<int>();


        // 12条路线缓存（由PathsPresetController计算并传入）
        public Dictionary<string, List<Vector3>> pathsCache = new Dictionary<string, List<Vector3>>();



        /* ---------------------------------------------- 生命周期 --------------------------------------------- */

        protected override void OnInit() { }


        /* ---------------------------------------------- API函数 --------------------------------------------- */

        // 初始化局内数据（根据关卡索引）
        public async UniTask InitModel(int level)
        {
            // 反序列化读表，初始化局内属性
            TextAsset csvFile = await this.GetSystem<IResourceSystem>().LoadAssetAsync<TextAsset>(_insideDataPath);
            _insideData = ParseCSV(csvFile.text, level);

            // Level
            Level.Value = _insideData.Level;
            // HP
            HP.Value = _insideData.HP;
            // 车辆总数
            numberOfVehicles.Value = _insideData.NumberOfVehicles;
            numberOfRecycledVehicles.Value = 0;
            // 车辆速度范围

        }


        /* ---------------------------------------------- TrafficGame_InsideData局内数据 API --------------------------------------------- */

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

        // 返回CSV局内数据
        public TrafficGame_InsideData GetInsideData()
        {
            return _insideData;
        }


        /* ---------------------------------------------- 12条路线数据 API --------------------------------------------- */

        public string GetKey(PathsPresetInfo.DirectionType directionType, PathsPresetInfo.MovementType movementType)
        {
            return $"{directionType}-{movementType}";
        }

        public void SetPath(Dictionary<string, List<Vector3>> paths)
        {
            this.pathsCache = paths;
        }

        public List<Vector3> GetPath(PathsPresetInfo.DirectionType directionType, PathsPresetInfo.MovementType movementType)
        {
            return pathsCache[GetKey(directionType, movementType)];
        }
    }
}
