using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UG20260527
{
    public class CameraController : MonoBehaviour
    {
        Transform target;

        void Start()
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        void LateUpdate()
        {
            // 更新摄像机位置
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
        }
    }
}

