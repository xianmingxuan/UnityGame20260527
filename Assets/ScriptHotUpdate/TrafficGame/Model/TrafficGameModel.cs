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
        /// CSV表加载路径
        /// </summary>
        public BindableProperty<string> insideDataPath { get; }
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
        /// 设置CSV局内数据
        /// </summary>
        /// <param name="data"></param>
        public void SetCSVInsideData(TrafficGame_InsideData data);
        /// <summary>
        /// 获取CSV局内数据
        /// </summary>
        /// <returns></returns>
        public TrafficGame_InsideData GetCSVInsideData();

        // 12条路线缓存
        public string GetKey(PathsPresetInfo.DirectionType directionType, PathsPresetInfo.MovementType movementType);
        public void SetPath(Dictionary<string, List<Vector3>> paths);
        public List<Vector3> GetPath(PathsPresetInfo.DirectionType directionType, PathsPresetInfo.MovementType movementType);

    }

    public class TrafficGameModel : AbstractModel, ITrafficGameModel
    {

        /* ---------------------------------------------- 局内数据 --------------------------------------------- */

        // 局内数据（读表）
        public BindableProperty<string> insideDataPath { get; private set; } = new BindableProperty<string>("Assets/AddressablesAsset/Config/CSV/TrafficGame_InsideData.CSV");
        public TrafficGame_InsideData _insideData;

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


        /* ---------------------------------------------- TrafficGame_InsideData局内数据 API --------------------------------------------- */

        // 设置CSV局内数据
        public void SetCSVInsideData(TrafficGame_InsideData data)
        {
            _insideData = data;
        }

        // 返回CSV局内数据
        public TrafficGame_InsideData GetCSVInsideData()
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
