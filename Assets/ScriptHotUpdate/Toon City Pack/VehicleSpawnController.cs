using System.Collections;
using UnityEngine;
using QFramework;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace UG20260527
{
    public class VehicleSpawnController : MonoBehaviour, IController
    {
        public GameObject prefab;
        public Transform spawnPoint;

        public void Init()
        {
            GameObject obj;
            obj = this.GetSystem<IResourceSystem>().Instantiate(prefab, gameObject.scene);
            obj.transform.position = spawnPoint.position;
        }

        void Start()
        {
            
        }




        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}