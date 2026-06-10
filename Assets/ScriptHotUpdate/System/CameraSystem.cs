using QFramework;
using UnityEngine;
using UnityEngine.UIElements;

namespace UG20260527
{
    public interface ICameraSystem : ISystem
    {
        public void SetTarget(Transform target);
    }
    public class CameraSystem : AbstractSystem, ICameraSystem
    {
        private Camera Cam;

        private Transform Target;
        private float SmoothSpeed = 0.05f;
        private Vector3 Offset = new Vector3(0f, 0f, -10f);

        protected override void OnInit()
        {
            Cam = Camera.main;
            //PublicMono.Instance.OnLateUpdate += LateUpdate;  // 若摄像机在LateUpdate中移动，因为帧率不匹配，会出现角色抖动的问题
            PublicMono.Instance.OnFixedUpdate += LateUpdate;  // 角色在FixedUpdate中移动，所以摄像机也要在这里移动
        }

        private void LateUpdate()
        {
            if (Target == null) return;

            // 死区
            if (Vector3.Distance(Cam.transform.position, Target.position + Offset) < 0.01f) return;

            // 插值 摄像机原pos 和 目标pos
            Vector3 smoothPos = Vector3.Lerp(Cam.transform.position, Target.position + Offset, SmoothSpeed);
            Cam.transform.position = smoothPos;
        }

        void ICameraSystem.SetTarget(Transform target)
        {
            Target = target; 
        }
    }
}
