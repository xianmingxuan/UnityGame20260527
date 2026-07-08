using UnityEngine;
using QFramework;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace UG20260527
{
    public class VehicleSpawnController : MonoBehaviour, IController
    {
        // 资源路径引用
        public List<string> vehiclePrefabPaths = new List<string>();  // 预制体路径
        private List<GameObject> vehiclePrefabs = new List<GameObject>();  // 预制体资源

        // 局内游戏数据
        private ITrafficGameModel _gameModel;

        private async void Awake()
        {
            // 获取 局内数据
            _gameModel = this.GetModel<ITrafficGameModel>();

            // 获取 预制体路径引用


            // 加载 预制体资源 到内存中
            var resSystem = this.GetSystem<IResourceSystem>();
            foreach (string path in vehiclePrefabPaths)
            {
                var pre = await resSystem.LoadAssetAsync<GameObject>(path);
                if(pre != null) vehiclePrefabs.Add(pre);
            }
        }

        /// <summary>
        /// 正式开启游戏
        /// </summary>
        public async UniTask BeginGame()
        {
            // 生成车辆
            GameObject obj = null;
            GameObject self = gameObject;

            for(int i = 0; i < _gameModel.numberOfVehicles.Value; i++)
            {
                // 协程：等待时，自身可能被销毁（退出场景），要检查自身是否存在
                if (self == null) return;

                // 实例化
                obj = this.GetSystem<IResourceSystem>().Instantiate(vehiclePrefabs[Random.Range(0,2)], gameObject.scene);
                if (obj == null) continue;
                // 车辆属性
                PathsPresetInfo.DirectionType directionType = (PathsPresetInfo.DirectionType)Random.Range(0, 4);
                PathsPresetInfo.MovementType movementType = (PathsPresetInfo.MovementType)Random.Range(0, 3);
                obj.GetComponent<VehicleController>().Init(directionType, movementType, Random.Range(_gameModel.GetCSVInsideData().VehicleMinSpeed, _gameModel.GetCSVInsideData().VehicleMaxSpeed));
                // 出车间隔时间
                await UniTask.WaitForSeconds(Random.Range(1, 3));
            }
        }


        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}