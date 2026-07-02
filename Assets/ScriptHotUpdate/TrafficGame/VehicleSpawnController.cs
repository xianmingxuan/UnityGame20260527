using System.Collections;
using UnityEngine;
using QFramework;
using System.Collections.Generic;

namespace UG20260527
{
    public class VehicleSpawnController : MonoBehaviour, IController
    {
        public GameObject prefab;
        public PathsPresetController pathsPresetController;

        public async void Init()
        {
            List<GameObject> prefabList = new List<GameObject>();

            IResourceSystem resourceSystem = this.GetSystem<IResourceSystem>();
            List<string> prefabPaths = resourceSystem.GetPrefabPathInFile("");
            if (prefabPaths != null && prefabPaths.Count > 0)
            {
                foreach (string path in prefabPaths)
                {
                    var pre = await resourceSystem.LoadAssetAsync<GameObject>(path);
                    prefabList.Add(pre);
                }
            }
        }

        void Start()
        {
            GameObject obj;
            obj = this.GetSystem<IResourceSystem>().Instantiate(prefab, gameObject.scene);
            obj.GetComponent<VehicleController>().Init(pathsPresetController.GetPath(PathsPresetInfo.DirectionType.East, PathsPresetInfo.MovementType.TurnLeft), 15f);
        }




        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}