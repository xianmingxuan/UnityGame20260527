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
        public AssetReference PlayerPrefab;

        async void Start()
        {
            GameObject pre = await PlayerPrefab.LoadAssetAsync<GameObject>().Task;
            GameObject obj = Instantiate(pre, transform.position, transform.rotation, transform.parent);
        }

        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}