using QFramework;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UG20260527
{
    public class PlayerSpawnPoint : MonoBehaviour, IController
    {
        [Tooltip("玩家资源 弱引用")]
        public GameObject playerPrefab;

        void Start()
        {
            GameObject obj = this.GetSystem<IResourceSystem>().Instantiate(playerPrefab, transform.parent);
            obj.transform.localPosition = transform.localPosition;
            obj.transform.localRotation = transform.localRotation;
        }

        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}